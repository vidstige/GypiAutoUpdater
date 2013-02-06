using System.Collections.Generic;

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

        public IList<GypElement> Elements { get { return _children; } }
    }
}