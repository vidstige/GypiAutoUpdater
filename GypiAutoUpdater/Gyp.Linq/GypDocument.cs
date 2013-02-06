using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GypiAutoUpdater.Model
{
    public class GypDocument: IGypParseListener
    {
        private string _propertyName = "root";
        private readonly Stack<GypElement> _stack = new Stack<GypElement>();
        private GypElement _root;

        public GypElement Root
        {
            get { return _root; }
        }

        public static GypDocument Load(FileInfo file)
        {
            var doc = new GypDocument();
            var parser = new GypParser(doc);
            parser.Parse(file);
            return doc;
        }

        public void CreateObject()
        {
            _stack.Push(new GypElement(_propertyName));
        }

        public void EndObject()
        {
            var x = _stack.Pop();
            if (_stack.Any())
            {
                _stack.Peek().Elements.Add(x);
            }
            else
            {
                _root = x;
            }
        }

        public void CreatePropertyName()
        {
        }

        public void EndPropertyName(string name)
        {
            _propertyName = name;
        }

        public void CreateArray()
        {
            _stack.Push(new GypElement(_propertyName));
        }

        public void EndArray()
        {
            var arr = _stack.Pop();
            _stack.Peek().Elements.Add(arr);
        }

        public void CreatePropertyValue()
        {
        }

        public void EndPropertyValue(string value)
        {
            _stack.Peek().Value = value;
        }

        public void AddStringToArray(string value)
        {
            _stack.Peek().Elements.Add(new GypElement(null) {Value = value});
        }
    }
}