using Microsoft.Extensions.Configuration;

namespace Syrwag.Movies.Neo4j.Neo4jClient
{
    public class Neo4jClientMoviesRepoSettings
    {
        private readonly IConfiguration _config;

        public Neo4jClientMoviesRepoSettings(IConfiguration config)
        {
            _config = config;
        }

        public string Uri => _config.GetValue("Neo4jClient:Uri", "http://localhost:7474/db/data");

        public string Username => _config.GetValue<string>("Neo4jClient:UserName", "neo4j");

        public string Password => _config.GetValue<string>("Neo4jClient:Password", null);
    }
}