using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Autodesk.Forge;
using System.IO;
using Autodesk.Forge.Model;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace Termite.Controllers
{
    public class HomeController : Controller
    {
        private readonly Data.Secrets _mySecrets;

        public HomeController(IOptions<Data.Secrets> optionsAccessor)
        {
            _mySecrets = optionsAccessor.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("Home/Upload")]
        public async Task<IActionResult> Post(IFormFile file)
        {
            if (file == null || file.Length == 0) return Content("Item not found");

            long size = file.Length;
            string fileName = file.FileName;

            // full path to file in temp location
            var tempFilePath = Path.GetTempFileName();

            if (file.Length > 0)
            {
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            // process uploaded files
            // Don't rely on or trust the FileName property without validation.

            // get a write enabled token
            TwoLeggedApi oauthApi = new TwoLeggedApi();
            dynamic bearer = await oauthApi.AuthenticateAsync(
                _mySecrets.FORGE_CLIENT_ID,
                _mySecrets.FORGE_CLIENT_SECRET,
                "client_credentials",
                new Scope[] { Scope.BucketCreate, Scope.DataCreate, Scope.DataWrite, Scope.DataRead });

            // create a randomg bucket name (fixed prefix + randomg guid)
            string bucketKey = "forgeapp" + Guid.NewGuid().ToString("N").ToLower();

            // create the Forge bucket
            PostBucketsPayload postBucket = new PostBucketsPayload(bucketKey, null, PostBucketsPayload.PolicyKeyEnum.Transient /* erase after 24h*/ );
            BucketsApi bucketsApi = new BucketsApi();
            bucketsApi.Configuration.AccessToken = bearer.access_token;
            dynamic newBucket = await bucketsApi.CreateBucketAsync(postBucket);

            // upload file (a.k.a. Objects)
            ObjectsApi objectsApi = new ObjectsApi();
            oauthApi.Configuration.AccessToken = bearer.access_token;
            dynamic newObject;
            using (StreamReader fileStream = new StreamReader(tempFilePath))
            {
                newObject = await objectsApi.UploadObjectAsync(bucketKey, fileName,
                    (int)fileStream.BaseStream.Length, fileStream.BaseStream,
                    "application/octet-stream");
            }

            // translate file
            string objectIdBase64 = ToBase64(newObject.objectId);
            List<JobPayloadItem> postTranslationOutput = new List<JobPayloadItem>()
    {
        new JobPayloadItem(
        JobPayloadItem.TypeEnum.Svf /* Viewer*/,
        new List<JobPayloadItem.ViewsEnum>()
        {
           JobPayloadItem.ViewsEnum._3d,
           JobPayloadItem.ViewsEnum._2d
        })
    };
            JobPayload postTranslation = new JobPayload(
                new JobPayloadInput(objectIdBase64),
                new JobPayloadOutput(postTranslationOutput));
            DerivativesApi derivativeApi = new DerivativesApi();
            derivativeApi.Configuration.AccessToken = bearer.access_token;
            dynamic translation = await derivativeApi.TranslateAsync(postTranslation);

            // check if is ready
            int progress = 0;
            do
            {
                System.Threading.Thread.Sleep(1000); // wait 1 second
                try
                {
                    dynamic manifest = await derivativeApi.GetManifestAsync(objectIdBase64);
                    progress = (string.IsNullOrWhiteSpace(Regex.Match(manifest.progress, @"\d+").Value) ? 100 : Int32.Parse(Regex.Match(manifest.progress, @"\d+").Value));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error on line 103: " + ex.Message);
                }
            } while (progress < 100);

            // ready!!!!

            // register a client-side script to show this model
            //Page.ClientScript.RegisterStartupScript(this.GetType(), "ShowModel", string.Format("<script>showModel('{0}');</script>", objectIdBase64));

            // clean up
            Directory.Delete(tempFilePath, true);

            return Ok(new { size, tempFilePath });
        }

        /// <summary>
        /// Convert a string into Base64 (source http://stackoverflow.com/a/11743162)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string ToBase64(string input)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(input);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
