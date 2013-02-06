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
        private bool _wasModified;

        public GypStreamEditor(string path, TextWriter output)
        {
            _path = path;
            _output = output;
        }

        public bool Go()
        {
            _wasModified = false;
            var parser = new GypParser(this);
            parser.Parse(new FileInfo(_path));
            _output.Flush();
            return _wasModified;
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
                    _wasModified = true;
                    _output.Write(string.Format("\n'{0}',", addition)); // TODO: Intendation not respected :-/
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
