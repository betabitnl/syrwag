using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Syrwag.Model;
using Syrwag.Movies.TinkerPop.GremlinClient;
using Syrwag.Movies.CosmosDb.MsDocumentClient;
using Syrwag.Movies.Neo4j.Neo4jClient;

namespace Syrwag.MoviesApp
{
    public class Program
    {
        public static async Task Main()
        {
            var config = GetConfiguration();

            await RunNeo4jDemo(config);
            await RunCosmosDbDemo(config);
            await RunTinkerPopDemo(config);
        }

        private static async Task RunCosmosDbDemo(IConfiguration config)
        {
            var docRepo = new CosmosDbDocumentClientMoviesRepo(config);

            await RunDemo(docRepo);
        }

        private static async Task RunTinkerPopDemo(IConfiguration config)
        {
            using (var gremlinRepo = new TinkerPopGremlinClientMoviesRepo(config))
            {
                await RunDemo(gremlinRepo);
            }
        }

        private static async Task RunNeo4jDemo(IConfiguration config)
        {
            using (var docRepo = new Neo4jClientMoviesRepo(config))
            {
                await RunDemo(docRepo);
            }
        }

        private static async Task RunDemo(IMovies repo)
        {
            try
            {
                DataSet dataSet = new DataSet();
                var jKRowling = dataSet.Persons["JKRowling"];
                await repo.InitialiseAsync();
                await repo.DropAllAsync();
                await repo.SaveAsync(dataSet.Movies["HarryPotterM1"]);
                await repo.SaveAsync(jKRowling);
                var movies = await repo.FindByWriter(jKRowling);
                foreach (var movie in movies)
                {
                    Console.WriteLine($"{jKRowling.Name} wrote: {movie.Title}.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static IConfiguration GetConfiguration()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", false, true)
                .AddJsonFile("appSettings.local.json", true, true)
                .Build();
            return config;
        }
    }
}
