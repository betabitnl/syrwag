using System.Collections.Generic;

namespace Syrwag.Model
{
    public class Person : Node
    {
        public string Name { get; set; }

        public int BornInYear { get; set; }

        public List<Wrote> Movies { get; set; } = new List<Wrote>();
    }
}