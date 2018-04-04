using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Syrwag.Model;
using Neo4jClient;

namespace Syrwag.Movies.Neo4j.Neo4jClient
{
    public class Neo4jClientMoviesRepo : IMovies, IDisposable
    {
        private readonly Neo4jClientMoviesRepoSettings _settings;
        private GraphClient _client;
        private readonly IMapper _mapper;

        public Neo4jClientMoviesRepo(IConfiguration config)
        {
            _settings = new Neo4jClientMoviesRepoSettings(config);

            _mapper = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<Movie, MovieEntity>();
                    cfg.CreateMap<MovieEntity, Movie>();
                    cfg.CreateMap<Person, PersonEntity>();
                    cfg.CreateMap<Wrote, WroteEntity>();
                })
                .CreateMapper();
        }

        public async Task InitialiseAsync(CancellationToken cancellationToken = default)
        {
            _client = new GraphClient(new Uri(_settings.Uri), _settings.Username, _settings.Password);
            await _client.ConnectAsync();
        }

        public async Task DropAllAsync(CancellationToken cancellationToken = default)
        {
            // MATCH (n) OPTIONAL MATCH (n)-[r]->() DELETE n, r
            await _client
                .Cypher
                .Match("(n)")
                .OptionalMatch("(n)-[r]->()")
                .Delete("n, r")
                .ExecuteWithoutResultsAsync();
        }

        public async Task SaveAsync(Movie movie, CancellationToken cancellationToken = default)
        {
            if (!movie.IsSaved)
            {
                var entity = _mapper.Map<MovieEntity>(movie);
                // CREATE (:Movie {Id: 'id', Title: 'title', ...})
                await _client
                    .Cypher
                    .Create("(:Movie {entity})")
                    .WithParam("entity", entity)
                    .ExecuteWithoutResultsAsync();
                movie.IsSaved = true;
            }
        }


        public async Task SaveAsync(Person person, CancellationToken cancellationToken = default)
        {
            if (!person.IsSaved)
            {
                var entity = _mapper.Map<PersonEntity>(person);

                var query = _client.Cypher;

                int pos = 1;
                foreach (var wrote in person.Movies)
                {
                    var movie = wrote.Movie;
                    if (movie.IsSaved)
                    {
                        // CREATE (m1:Movie {Id: 'id', Title: 'title', ...})
                        query = query
                            .Match($"(m{pos}:Movie {{Id: {{id{pos}}}}})")
                            .WithParam($"id{pos}", movie.Id);
                    }

                    pos++;
                }

                // CREATE (p:Person {Id: 'id', Name: 'name', ...})
                query = query
                    .Create("(p:Person {person})")
                    .WithParam("person", entity);
                pos = 1;
                foreach (var wrote in person.Movies)
                {
                    var movie = wrote.Movie;
                    var movieEntity = _mapper.Map<MovieEntity>(movie);
                    var wroteEntity = _mapper.Map<WroteEntity>(wrote);
                    if (!movie.IsSaved)
                    {
                        query = query
                            .Create($"(m{pos}:Movie {{entity{pos}}})")
                            .WithParam($"entity{pos}", movieEntity);
                    }
                    // CREATE (p)-[:WROTE { Using: 'using' }]->(m1)
                    query = query
                        .Create($"(p)-[:WROTE {{wrote{pos}}}]->(m{pos})")
                        .WithParam($"wrote{pos}", wroteEntity);

                    pos++;
                }

                await query.ExecuteWithoutResultsAsync();
                person.IsSaved = true;
            }
        }

        public async Task<IEnumerable<Movie>> FindByWriter(Person writer, CancellationToken cancellationToken = default)
        {
            var writerEntity = _mapper.Map<PersonEntity>(writer);
            var movies = await _client.Cypher
                .Match("(:Person {Name: $name})-[:WROTE]->(movie:Movie)")
//                .Match("(movie:Movie)<-[:WROTE]-(:Person {Name: $name})")
                .WithParam("name", writerEntity.Name)
                .Return<MovieEntity>("movie")
                .ResultsAsync;
            return movies.Select(movie => _mapper.Map<Movie>(movie)).ToList();
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}