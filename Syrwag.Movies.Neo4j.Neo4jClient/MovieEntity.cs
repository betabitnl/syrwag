namespace Syrwag.Movies.Neo4j.Neo4jClient
{
    public class MovieEntity : Entity
    {
        public string Title { get; set; }

        public int Released { get; set; }

        public string Language { get; set; }
    }
}