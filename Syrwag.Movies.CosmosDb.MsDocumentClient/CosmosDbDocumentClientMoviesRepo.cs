using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Graphs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Syrwag.Model;

namespace Syrwag.Movies.CosmosDb.MsDocumentClient
{
    public class CosmosDbDocumentClientMoviesRepo : IMovies
    {
        private readonly CosmosDbDocumentClientMoviesRepoSettings _settings;
        private DocumentClient _client;
        private DocumentCollection _collection;

        public CosmosDbDocumentClientMoviesRepo(IConfiguration config)
        {
            _settings = new CosmosDbDocumentClientMoviesRepoSettings(config);
        }

        public async Task InitialiseAsync(CancellationToken cancellationToken = default)
        {
            var databaseName = _settings.Database;
            var collectionName = _settings.Collection;

            // Create the client.
            _client = new DocumentClient(new Uri(_settings.Endpoint), _settings.AuthKey);

            // Create the database, if it did not exist yet.
            await _client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseName });

            // Create the collection if it did not exist yet. 
            _collection = await _client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(databaseName),
                new DocumentCollection
                {
                    Id = collectionName
                },
                new RequestOptions
                {
                    OfferThroughput = 400,
                    ConsistencyLevel = ConsistencyLevel.Session
                }
            );
        }

        public async Task DropAllAsync(CancellationToken cancellationToken = default)
        {
            var queryText = "g.V().drop()";
            await RunQuery(queryText, cancellationToken);
        }

        public async Task SaveAsync(Movie movie, CancellationToken cancellationToken = default)
        {
            if (!movie.IsSaved)
            {
                var queryText = GenerateQueryFromMovie(movie);
                await RunQuery(queryText, cancellationToken);
                movie.IsSaved = true;
            }
        }

        public async Task SaveAsync(Person person, CancellationToken cancellationToken = default)
        {
            if (!person.IsSaved)
            {
                await RunQuery(GenerateQueryFromPerson(person), cancellationToken);
                person.IsSaved = true;
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

        private async Task RunQuery(string queryText, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Running Gremlin query: {queryText}");
            IDocumentQuery<dynamic> query = _client.CreateGremlinQuery<dynamic>(_collection, queryText);
            while (query.HasMoreResults)
            {
                foreach (dynamic result in await query.ExecuteNextAsync(cancellationToken))
                {
                    Console.WriteLine($"\t {JsonConvert.SerializeObject(result)}");
                }
            }
        }

        private string GenerateQueryFromMovie(Movie movie)
        {
            StringBuilder query = new StringBuilder();
            query.Append("g.addV('Movie')");
            AddProperty(query, "id", movie.Id);
            AddProperty(query, "title", movie.Title);
            AddProperty(query, "released", movie.Released);
            AddProperty(query, "language", movie.Language);
            return query.ToString();
        }

        private string GenerateQueryFromPerson(Person person)
        {
            StringBuilder query = new StringBuilder();

            query.Append("g.addV('Person')");
            AddProperty(query, "id", person.Id);
            AddProperty(query, "name", person.Name);
            AddProperty(query, "born", person.BornInYear);
            return query.ToString();
        }


        private void AddProperty(StringBuilder query, string name, string value)
        {

            if (!string.IsNullOrEmpty(value))
            {
                var escapedValue = value.Replace("\\", "\\\\").Replace("'", "\\'");
                query.Append($".property('{name}', '{escapedValue}')");
            }
        }

        private void AddProperty(StringBuilder query, string name, int value)
        {
            query.Append($".property('{name}', {value})");
        }
    }
}
