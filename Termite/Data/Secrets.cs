using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Termite.Data
{
    public class Secrets
    {
        public Secrets()
        {
            FORGE_CLIENT_ID = "your id";
            FORGE_CLIENT_SECRET = "your secret";
        }

        public string FORGE_CLIENT_ID { get; set; }
        public string FORGE_CLIENT_SECRET { get; set; }
    }
}
