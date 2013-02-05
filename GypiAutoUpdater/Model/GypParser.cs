using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GypiAutoUpdater.Model
{
    class GypParser
    {
        private enum State
        {
            None,
            Obj,
            Property,
            Str,
            Array,
            Comment
        }

        private readonly Stack<State> _stack = new Stack<State>();
        private StringBuilder _currentString;

        public void Parse(TextReader input)
        {
            _stack.Push(State.None);

            _currentString = new StringBuilder();
            var debug = new StringBuilder();
            int x = input.Read();
            while (x >= 0)
            {
                var c = (char)x;
                debug.Append(c);

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
                        else if (c == '\'') { Push(State.Property); _currentString = new StringBuilder();}
                        else if (c == '{') Push(State.Obj);
                        else if (c == ',') Eat();
                        else if (c == '[') Push(State.Array);
                        else if (c == '#') Push(State.Comment);
                        else if (c == ':') Eat();
                        break;
                    case State.Property:
                        if (c == '\'') Pop();
                        else _currentString.Append(c);
                        break;
                    case State.Str:
                        if (c == '\'') Pop();
                        else _currentString.Append(c);
                        break;
                    case State.Array:
                        if (c == ']') Pop();
                        else if (c == '{') Push(State.Obj);
                        else if (c == '\'') Push(State.Str);
                        else Eat();
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
            Console.WriteLine("Start " + state);
            _stack.Push(state);
        }

        private void Pop()
        {
            Console.WriteLine("End" + _stack.Pop() + " (" + _currentString + ")");
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