using System.Collections.Generic;
using System.Linq;
using Syrwag.Model;

namespace Syrwag.MoviesApp
{
    public class DataSet
    {
        public DataSet()
        {
            GenerateMovies();
            GeneratePersons();
        }

        public Dictionary<string, Movie> Movies { get; private set; }
        public Dictionary<string, Person> Persons { get; private set; }

        private void GenerateMovies()
        {
            Movies = new List<Movie>
            {
                new Movie
                {
                    Id = "HarryPotterM1",
                    Title = "Harry Potter and the Philosopher's Stone",
                    Released = 2001,
                    Language = "English"
                },
                new Movie
                {
                    Id = "HarryPotterM2",
                    Title = "Harry Potter and the Chamber of Secrets",
                    Released = 2002,
                    Language = "English"
                },
                new Movie
                {
                    Id = "HarryPotterM3",
                    Title = "Harry Potter and the Prisoner of Azkaban",
                    Released = 2004,
                    Language = "English"
                }
            }.ToDictionary(movie => movie.Id);
        }

        private void GeneratePersons()
        {
            Persons = new List<Person>
            {
                new Person
                {
                    Id = "JKRowling",
                    Name = "J. K. Rowling",
                    BornInYear = 1965,
                }

            }.ToDictionary(person => person.Id);
            var jKRowling = Persons["JKRowling"];
            jKRowling.Movies.AddRange(new []
            {
                new Wrote
                {
                    Movie = Movies["HarryPotterM1"],
                    Writer = jKRowling,
                    Using = "quill"
                },
                new Wrote
                {
                    Movie = Movies["HarryPotterM2"],
                    Writer = jKRowling,
                    Using = "typewriter"
                },
                new Wrote
                {
                    Movie = Movies["HarryPotterM3"],
                    Writer = jKRowling,
                    Using = "computer"
                }
            });
        }
    }
}