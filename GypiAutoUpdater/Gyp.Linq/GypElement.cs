using System.Collections.Generic;
using System.Linq;

namespace GypiAutoUpdater.Model
{
    public class GypElement
    {
        private readonly string _name;
        private readonly List<GypElement> _children = new List<GypElement>();

        public GypElement(string name)
        {
            _name = name;
        }

        public string Value { get; set; }

        public string Name
        {
            get { return _name; }
        }

        public IList<GypElement> Children { get { return _children; } }
        public IEnumerable<GypElement> Elements(string name) { return _children.Where(e => e.Name == name); }
        public GypElement Element(string name) { return _children.FirstOrDefault(e => e.Name == name); }
    }
}