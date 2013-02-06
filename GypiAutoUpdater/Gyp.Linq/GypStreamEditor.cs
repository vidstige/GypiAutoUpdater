using System.Collections.Generic;
using System.IO;
using GypiAutoUpdater.Model;

namespace GypiAutoUpdater.Gyp.Linq
{
    public class GypStreamEditor : IGypParseListener
    {
        private readonly string _path;
        private readonly TextWriter _output;
        private readonly Dictionary<string, IEnumerable<string>> _arrayAdditions = new Dictionary<string, IEnumerable<string>>();
        private string _tmp = string.Empty;

        public GypStreamEditor(string path, TextWriter output)
        {
            _path = path;
            _output = output;
        }

        public void Go()
        {
            var parser = new GypParser(this);
            parser.Parse(new FileInfo(_path));
        }

        public void AddStringToArray(string arrayName, IEnumerable<string> values)
        {
            _arrayAdditions.Add(arrayName, values);
        }

        public void CreateObject()
        {
            
        }

        public void EndObject()
        {
        }

        public void CreatePropertyName()
        {
        }

        public void EndPropertyName(string name)
        {
            _tmp = name;
        }

        public void CreateArray()
        {
            if (_arrayAdditions.ContainsKey(_tmp))
            {
                foreach (var addition in _arrayAdditions[_tmp])
                {
                    _output.Write(string.Format("'{0}',", addition));
                }
            }
        }

        public void EndArray()
        {
        }

        public void CreatePropertyValue()
        {
        }

        public void EndPropertyValue(string value)
        {
        }

        public void AddStringToArray(string value)
        {
        }

        public void Character(char c)
        {
            _output.Write(c);
        }
    }
}
