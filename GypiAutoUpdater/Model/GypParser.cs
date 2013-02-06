using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GypiAutoUpdater.Model
{
    abstract class GypElement
    {
        private readonly string _name;

        protected GypElement(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public abstract IEnumerable<GypElement> Elements();
        public abstract IEnumerable<GypElement> Elements(string name);
    }

    class GypObject: GypElement
    {
        private readonly List<GypElement> _children = new List<GypElement>();
        public GypObject(string name) : base(name)
        {
        }

        public IList<GypElement> Children { get { return _children; } }

        public override IEnumerable<GypElement> Elements()
        {
            return _children;
        }
        public override IEnumerable<GypElement> Elements(string name)
        {
            return _children.Where(element => element.Name == name);
        }
    }

    class GypArray: GypElement
    {
        private readonly List<GypObject> _children = new List<GypObject>();
        public GypArray(string name) : base(name)
        {
        }

        public override IEnumerable<GypElement> Elements()
        {
            return _children;
        }

        public override IEnumerable<GypElement> Elements(string name)
        {
            throw new InvalidOperationException("Arrays have only objects");
        }
    }


    public class GypDocument: IGypParseListener
    {
        private string _propertyName = "root";
        private readonly Stack<GypElement> _stack = new Stack<GypElement>();
        private GypElement _root;

        public static GypDocument Load(FileInfo file)
        {
            var doc = new GypDocument();
            var parser = new GypParser(doc);
            parser.Parse(file);
            return doc;
        }

        public void CreateObject()
        {
            _stack.Push(new GypObject(_propertyName));
        }

        public void EndObject()
        {
            var x = _stack.Pop();
            if (!_stack.Any()) _root = x;
        }

        public void CreatePropertyName()
        {
        }

        public void EndPropertyName(string name)
        {
            _propertyName = name;
        }
    }

    interface IGypParseListener
    {
        void CreateObject();
        void EndObject();

        void CreatePropertyName();
        void EndPropertyName(string name);
    }

    class GypParser
    {
        private readonly IGypParseListener _listner;

        private enum State
        {
            None,
            Obj,
            PropertyName,
            PropertyValue,
            Str,
            Array,
            Comment
        }

        private readonly Stack<State> _stack = new Stack<State>();
        private StringBuilder _currentString;

        public GypParser(IGypParseListener listner)
        {
            _listner = listner;
        }

        public void Parse(FileInfo gypFile)
        {
            using (var reader = new StreamReader(gypFile.OpenRead()))
            {
                Parse(reader);
            }
        }

        public void Parse(TextReader input)
        {
            _stack.Push(State.None);

            _currentString = new StringBuilder();
            int x = input.Read();
            while (x >= 0)
            {
                var c = (char)x;

                switch (_stack.Peek())
                {
                    case State.None:
                        if (IsWhiteSpace(c)) Eat();
                        else if (c == '{') Push(State.Obj);
                        else Fail();
                        break;
                    case State.Obj:
                        if (IsWhiteSpace(c)) Eat();
                        else if (c == '}') Pop();
                        else if (c == '\'') { Push(State.PropertyValue); Push(State.PropertyName); _currentString = new StringBuilder(); }
                        else if (c == '#') Push(State.Comment);
                        else if (c == ',') Eat();
                        else Fail();
                        break;
                    case State.PropertyName:
                        if (c == '\'') Pop();
                        else _currentString.Append(c);
                        break;
                    case State.PropertyValue:
                        if (c == '\'' || c == '"') { Push(State.Str); _currentString = new StringBuilder();}
                        else if (IsWhiteSpace(c)) Eat();
                        else if (c == '{') Push(State.Obj);
                        else if (c == '[') Push(State.Array);
                        else if (c == ',') Pop();
                        else if (c == '#') Push(State.Comment);
                        else if (c == ':') Eat();
                        else Fail();
                        break;
                    case State.Str:
                        if (c == '\'') Pop();
                        else if (c == '"') Pop();
                        else _currentString.Append(c);
                        break;
                    case State.Array:
                        if (c == ']') Pop();
                        else if (c == '#') Push(State.Comment);
                        else if (c == '{') Push(State.Obj);
                        else if (c == '[') Push(State.Array);
                        else if (c == '\'' || c == '"') Push(State.Str);
                        else if (IsWhiteSpace(c)) Eat();
                        else if (c == ',') Eat();
                        else Fail();
                        break;
                    case State.Comment:
                        if (c == '\n') Pop();
                        else Eat();
                        break;
                }
                x = input.Read();
            }   
        }

        private void Push(State state)
        {
            switch (state)
            {
                case State.Obj: _listner.CreateObject(); break;
                case State.PropertyName: _listner.CreatePropertyName(); break;
            }
            _stack.Push(state);
        }

        private void Pop()
        {
            var state = _stack.Pop();

            switch (state)
            {
                case State.Obj: _listner.EndObject(); break;
                case State.PropertyName: _listner.EndPropertyName(_currentString.ToString()); break;
            }

            if (IsAutoClose(state) && _stack.Peek() == State.PropertyValue) Pop();
        }

        private static bool IsAutoClose(State state)
        {
            return state == State.Array || state == State.Obj;
        }

        private void Fail()
        {
            throw new ApplicationException("Could not parse");
        }

        private bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t' || c == '\n' || c == '\r';
        }

        private void Eat()
        {
        }
    }
}