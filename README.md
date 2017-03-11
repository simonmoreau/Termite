
# Termite

A small web viewer using Autodesk Forge and ASP.NET Core

## Getting Started

Open the solution in Visual Studio 2017, retrieve the packages and hit Run.

### Prerequisites

* Create a new application on the [Autodesk Forge website](https://developer.autodesk.com/myapps/create)
* Create two secrets, the FORGE_CLIENT_SECRET and the FORGE_CLIENT_ID by using the
[Secret Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)
* Copy your Client ID and Client Secret in the secret manager

## Deployment

This solution can be published to an [Azure Web Server](azure.microsoft.com). Don't forget to copy your client ID and secret in the Application Settings of the Azure Portal.

## Built With

* [ASP.NET Core](https://www.microsoft.com/net/core#windowsvs2017) - The back-end framework
* [Material Design Light](https://getmdl.io/) - The front-end library
* [The Noun Project](thenounproject.com) - For the icon

## Contributing

Please feel free to propose pull request, I would be happy to integrate your improvements.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

Thanks to [Augusto Goncalves](https://forge.autodesk.com/author/augusto-goncalves) for his [tutorial](https://forge.autodesk.com/blog/forge-aspnet-zero-hero-30-minutes).
