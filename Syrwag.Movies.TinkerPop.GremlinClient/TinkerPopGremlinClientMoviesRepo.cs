using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Syrwag.Model;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Remote;
using Gremlin.Net.Process.Traversal;
using Gremlin.Net.Structure;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Syrwag.Movies.TinkerPop.GremlinClient
{
    public class TinkerPopGremlinClientMoviesRepo : IMovies, IDisposable
    {
        private readonly TinkerPopGremlinClientMoviesRepoSettings _settings;
        private IGremlinClient _client;
        private DriverRemoteConnection _remote;

        public TinkerPopGremlinClientMoviesRepo(IConfiguration config)
        {

            _settings = new TinkerPopGremlinClientMoviesRepoSettings(config);
        }

        public Task InitialiseAsync(CancellationToken cancellationToken = default)
        {
            var gremlinServer = new GremlinServer(
                _settings.Hostname,
                _settings.Port,
                _settings.EnableSsl,
                _settings.Username,
                _settings.Password);
            _client = new Gremlin.Net.Driver.GremlinClient(gremlinServer);
            _remote = new DriverRemoteConnection(_client); 
            return Task.CompletedTask;

        }

        public async Task DropAllAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Dropping all:");
            var g = GetGraph();
            var result = await g.V().Drop().Promise(p => p.ToList());
            Console.WriteLine($"\t {JsonConvert.SerializeObject(result)}");
        }

        public async Task SaveAsync(Movie movie, CancellationToken cancellationToken = default)
        {
            if (!movie.IsSaved)
            {
                Console.WriteLine($"Saving Movie '{movie.Id}':");
                var result = await GenerateQuery(GetGraph(), movie).Promise(p => p.ToList());
                movie.IsSaved = true;
                Console.WriteLine($"\t {JsonConvert.SerializeObject(result)}");
            }
        }

        public async Task SaveAsync(Person person, CancellationToken cancellationToken = default)
        {
            if (!person.IsSaved)
            {
                Console.WriteLine($"Saving Person '{person.Id}':");
                var g = GetGraph();
                var query = GenerateQuery(g, person);
                var result = await query.Promise(p => p.ToList());
                person.IsSaved = true;
                Console.WriteLine($"\t {JsonConvert.SerializeObject(result)}");
            }

            foreach (var wrote in person.Movies)
            {
                await SaveAsync(wrote.Movie, cancellationToken);
            }
        }

        public Task<IEnumerable<Movie>> FindByWriter(Person writer, CancellationToken cancellationToken = default)
        {
            // TODO: Implement
            return Task.FromResult(new List<Movie>().AsEnumerable());
        }

        private static GraphTraversal<Vertex, Vertex> GenerateQuery(GraphTraversalSource g, Movie movie)
        {
            return g.AddV("Movie")
                .Property("id", movie.Id)
                .Property("title", movie.Title)
                .Property("released", movie.Released)
                .Property("language", movie.Language);
        }

        private static GraphTraversal<Vertex, Vertex> GenerateQuery(GraphTraversalSource g, Person person)
        {
            return g.AddV("Person")
                .Property("id", person.Id)
                .Property("name", person.Name)
                .Property("born", person.BornInYear);
        }

        private GraphTraversalSource GetGraph() => new Graph().Traversal().WithRemote(_remote);

        public void Dispose()
        {
            _client?.Dispose();
            _remote?.Dispose();
        }
    }
}