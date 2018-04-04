using Microsoft.Extensions.Configuration;

namespace Syrwag.Movies.CosmosDb.MsDocumentClient
{
    /// <summary>
    /// The defaults in this class can be used to run against de Cosmos DB emulator.
    /// </summary>
    public class CosmosDbDocumentClientMoviesRepoSettings
    {
        private readonly IConfiguration _config;

        public CosmosDbDocumentClientMoviesRepoSettings(IConfiguration config)
        {
            _config = config;
        }

        public string Endpoint => $@"https://{HostName}:{Port}/";

        public string AuthKey => _config.GetValue("CosmosDbDocumentClient:AuthKey", _config.GetValue("CosmosDb:AuthKey", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="));

        public string Database => _config.GetValue("CosmosDbDocumentClient:Database", _config.GetValue("CosmosDb:Database", "Cyrwag"));

        public string Collection => _config.GetValue("CosmosDbDocumentClient:Collection", _config.GetValue("CosmosDb:Collection", "Movies"));

        private string HostName
        {
            get
            {
                var hostname = _config.GetValue<string>("CosmosDbDocumentClient:HostName");
                if (string.IsNullOrWhiteSpace(hostname))
                {
                    var accountName = _config.GetValue<string>("CosmosDb:AccountName");
                    if (!string.IsNullOrWhiteSpace(accountName))
                    {
                        hostname = $@"{accountName}.documents.azure.com";
                    }
                }

                if (string.IsNullOrWhiteSpace(hostname))
                {
                    hostname = "localhost";
                }

                return hostname;
            }
        }
        

        private int Port => _config.GetValue("CosmosDbDocumentClient:Port", _config.GetValue("CosmosDb:Port", 8081));
    }
}