using Microsoft.Extensions.Configuration;

namespace Syrwag.Movies.TinkerPop.GremlinClient
{
    public class TinkerPopGremlinClientMoviesRepoSettings
    {
        private readonly IConfiguration _config;

        public TinkerPopGremlinClientMoviesRepoSettings(IConfiguration config)
        {
            _config = config;
        }

        public string Hostname => _config.GetValue("CosmosDbGremlinClient:Hostname", "localhost");

        public int Port => _config.GetValue("CosmosDbGremlinClient:Port", 8182);

        public bool EnableSsl=> _config.GetValue("CosmosDbGremlinClient:EnableSsl", false);

        public string Username => _config.GetValue<string>("CosmosDbGremlinClient:UserName", null);

        public string Password => _config.GetValue<string>("CosmosDbGremlinClient:Password", null);
    }
}