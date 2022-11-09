using ImGuiNET;

namespace TA1;

public struct LSA
{
    private Dictionary<int,(string, string)> _gotos;

    private struct Rules
    {
        public List<string> Keyword { get; set; }
        public List<string> Separator{ get; set; }

        public Rules()
        {
            Keyword = new List<string>{"Y","X","b","e","W","\\","/"};
            Separator = new List<string>{"(",")"};
        }
    };

    private readonly Rules _rule;

    public LSA()
    {
        _rule = new Rules();
        _gotos = new Dictionary<int, (string, string)>();
        _pairOfTokens = new List<(Token, Token)>();
    }

    public interface INode
    {
        public string Name { get; set; }
        public void DrawVertex(Graph graph);
        public void DrawEdges(Graph graph,Stack<string> names,bool isArrow, string text);
    }

    private class BeginNode : INode
    {
        private string _name = "";
        private readonly INode _nextNode;
        public BeginNode(INode nextNode)
        {
            Name = "begin";
            _nextNode = nextNode;
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public void DrawVertex(Graph graph)
        {
            graph.Add(new TA1.BeginNode(),Name);
            _nextNode.DrawVertex(graph);
        }

        public void DrawEdges(Graph graph,Stack<string> names,bool isArrow, string text)
        {
            names.Push(Name);
            _nextNode.DrawEdges(graph, names,false,"");
            names.Pop();
        }
    };

    private struct EndNode : INode
    {
        private bool _wasDraw = false;
        private string _name = "";

        public EndNode()
        {
            Name = "end";
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public void DrawVertex(Graph graph)
        {
            if(_wasDraw)
                return;
            graph.Add(new TA1.EndNode(),Name);
            _wasDraw = true;
        }

        public void DrawEdges(Graph graph,Stack<string> names,bool isArrow, string text)
        {
            graph.Connect(names.Peek(),Name, isArrow, text);
        }

    };

    private class EdgeNode: INode
    {
        private int _num;

        private string _name = "";
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        private (string,string) _edge;
        public INode? NextNode;
        private bool _isGoto;
        public EdgeNode(int num)
        {
            _num = num;
        }

        public void Set(string edge)
        {
            _edge.Item2 = edge;
            _isGoto = false;
        }

        public void Set((string, string) edge)
        {
            _edge = edge;
            _isGoto = true;
        }

        public void DrawVertex(Graph graph)
        {
            if(!_isGoto)
                NextNode?.DrawVertex(graph);

        }

        public void DrawEdges(Graph graph,Stack<string> names,bool isArrow, string text)
        {
            if(_isGoto)
                graph.Connect(names.Peek(),_edge,isArrow,text);
            else
                NextNode?.DrawEdges(graph, names,isArrow,text);
        }
    };

    private class StatementNode: INode
    {
        private bool _wasDraw;
        private string _name = "";
        private readonly INode _trueNode;
        private readonly INode _falseNode;
        private readonly int _num;

        public string Name
        {
            get => _name;
            set => _name = value;
        }
        public StatementNode(string name, int num,INode tr, INode fl)
        {
            Name = name;
            _num = num;
            _trueNode = tr;
            _falseNode = fl;
        }
        public void DrawVertex(Graph graph)
        {
            if(! _wasDraw)
            {
                graph.Add(new TA1.StatementNode(_num),Name);
                _wasDraw = true;
            }
            _trueNode.DrawVertex(graph);
            _falseNode.DrawVertex(graph);
        }

    public void DrawEdges(Graph graph,Stack<string> names,bool isArrow, string text)
        {
            graph.Connect(names.Peek(),Name,isArrow, text);
            names.Push(Name);
            _trueNode.DrawEdges(graph,names,true,"1");
            _falseNode.DrawEdges(graph,names,false,"0");
            names.Pop();
        }
    };

    private class StateNode: INode
    {
        private bool _wasDraw;
        private string _name = "";
        private INode _nextNode;
        private int _num;

        public string Name
        {
            get => _name;
            set => _name = value;
        }
        
        public StateNode(string name, int num,INode tr)
        {
            Name = name;
            _num = num;
            _nextNode = tr;
        }

        public void DrawVertex(Graph graph)
        {
            if(! _wasDraw)
            {
                graph.Add(new TA1.StateNode(_num),Name);
                _wasDraw = true;
            }
            _nextNode.DrawVertex(graph);
        }

    public void DrawEdges(Graph graph,Stack<string> names, bool isArrow, string text)
        {
            graph.Connect(names.Peek(),Name, isArrow,text);
            names.Push(Name);
            _nextNode.DrawEdges(graph,names,false,"");
            names.Pop();
        }
    };

    private struct Token
    {
        public enum Types
        {
            Keyword,
            Separator,
            Number,
            None
        };

        public Types Type;
        public string Value;

        public Token()
        {
            Type = Types.None;
            Value = "";
        }
    };

    private readonly List<(Token, Token)> _pairOfTokens;

    private bool _endBlock = false;
    private bool _startBlock = false;

    private void ParseNumber(ref Token token,string symbols,int pos,ref int offset)
    {
        token.Type = Token.Types.Number;
        while('0' <= symbols[pos+offset] && '9' >= symbols[pos+offset])
            token.Value += symbols[pos + (offset++)];
    }

    private void ParseSeparator(ref Token token,string symbols, int pos, ref int offset)
    {
        while(pos + offset < symbols.Length )
        {
            token.Value += symbols[pos + (offset++)];
            if (!_rule.Separator.Contains(token.Value)) continue;
            token.Type = Token.Types.Separator;
            break;
        }
    }

    private void ParseKeyword(ref Token token,string symbols, int pos, ref int offset)
    {
        while(pos + offset < symbols.Length )
        {
            token.Value += symbols[pos + (offset++)];
            if (!_rule.Keyword.Contains(token.Value)) continue;
            token.Type = Token.Types.Keyword;
            break;
        }
    }

    private static bool IsWhitespace(char ch)
    {
        return ch is ' ' or '\n' or '\t' or '\r';
    }

    private Token GetToken(string symbols, int pos, out int offset)
    {
        offset = 0;
        var t = new Token();
        if(symbols.Length <= pos)
            return t;

        while(pos + offset < symbols.Length && IsWhitespace(symbols[pos+offset]))
            offset++;

        if(symbols.Length <= pos + offset)
            return t;
        
        char sym = symbols[pos + offset];
        if('0' <= symbols[pos+offset] && '9' >= symbols[pos+offset] )
            ParseNumber(ref t,symbols,pos,ref offset);
        else if (_rule.Separator.Any((x)=>x[0] == sym))
            ParseSeparator(ref t,symbols,pos,ref offset);
        else if (_rule.Keyword.Any((x)=>x[0] == sym))
            ParseKeyword(ref t,symbols,pos,ref offset);

        return t;
    }

    private void Lexer(string symbols)
    {
        _endBlock = false;
        _startBlock = false;

        if(symbols.Length == 0)
            throw new Exception("");

        List<Token> tokens = new List<Token>();
        int pos = 0;
        Token token;
        do
        {
            token = GetToken(symbols,pos,out var offset);
            pos += offset;
            tokens.Add(token);
        }while(token.Type != Token.Types.None);

        tokens.RemoveAt(tokens.Count - 1);

        Stack<Token> stackBracket = new Stack<Token>();
        Stack<Token> stackKeyword = new Stack<Token>();
        foreach (var item in tokens)
        {
            // item is keyword
            if(item.Type == Token.Types.Keyword)
            {
                if(stackKeyword.Count == 0)
                    stackKeyword.Push(item);
                else if (stackKeyword.Peek().Value == "Y" && (item.Value == "b" || item.Value == "e"))
                {
                    _pairOfTokens.Add((stackKeyword.Peek(),item));
                    stackKeyword.Pop();
                }
                else
                    throw new Exception("Два состояния разом");
            }
            else if(item.Type == Token.Types.Number) // item is keyword
            {
                if(stackKeyword.Count != 0 )
                {
                    if(stackKeyword.Peek().Value != "b" && stackKeyword.Peek().Value != "e")
                    {
                        _pairOfTokens.Add((stackKeyword.Peek(),item));
                        stackKeyword.Pop();
                    }
                    else
                        throw new Exception("(BEGIN OR END) and number");
                }
                else
                    throw new Exception("Нет состояния");
            }
            else if(item.Type == Token.Types.Separator)
            {
                if(item.Value == "(")
                {
                    if(stackBracket.Count == 0)
                        stackBracket.Push(item);
                    else
                        throw new Exception("Две ( подряд");
                }
                else if(item.Value == ")")
                {
                    if(stackBracket.Count != 0)
                        stackBracket.Pop();
                    else
                        throw new Exception("не парная правая скобка");
                }
            }
        }

        int beginCount = 0,endCount = 0;
        Dictionary<string ,int> downSymbols = new Dictionary<string, int>();
        foreach (var item in _pairOfTokens)
        {
            if(item.Item1.Value == "Y" && item.Item2.Value == "b")
                beginCount++;
            if(item.Item1.Value == "Y" && item.Item2.Value == "e")
                endCount++;
            if(item.Item1.Value == "\\")
            {
                if(downSymbols.ContainsKey(item.Item2.Value))
                    throw new Exception("count(\\) > 1");
                else
                {
                    downSymbols.Add(item.Item2.Value,0);
                    downSymbols[item.Item2.Value]++;
                }

            }
        }
        string err = "";
        if(beginCount > 1)
            err += "(count(Yb) > 1)\n";
        if(endCount > 1)
            err += "(count(Ye) > 1)\n";
        if(_pairOfTokens[0].Item1.Value != "Y" || _pairOfTokens[0].Item2.Value != "b")
            err += "Yb must be first\n";
        if(_pairOfTokens.Count > 0)
            if(_pairOfTokens[^1].Item1.Value != "Y" || _pairOfTokens[^1].Item2.Value != "e")
                err += "Ye must be last\n";

        if(err != "")
            throw new Exception(err);
    }

    private struct GraphCreator
    {
        public struct Token
        {
            public enum Types
            {
                None,
                Begin,
                End,
                Y,
                X,
                W,
                Up,
                Down,
                Count
            };

            public readonly Types Type;
            public readonly int Data;

            public Token(Types type, int data)
            {
                Type = type;
                Data = data;
            }
        };
        
        public GraphCreator()
        {
            _pairs = new List<Token>();
            _from = new Dictionary<int, List<INode>>();
            _to = new Dictionary<int, (INode, string)>();
            _endNode = new EndNode();
        }

        private readonly List<Token> _pairs;
        private readonly Dictionary<int,List<INode>> _from;
        private readonly Dictionary<int,(INode,string)> _to;
        private INode _endNode;

        public void ConvertTokens(List<(LSA.Token, LSA.Token)> pairs)
        {
            _pairs.Clear();
            foreach (var pair in pairs)
            {
                switch (pair.Item1.Value)
                {
                    case "Y" when pair.Item2.Value == "b":
                        _pairs.Add(new Token(Token.Types.Begin,0));
                        break;
                    case "Y" when pair.Item2.Value == "e":
                        _pairs.Add(new Token(Token.Types.End,0));
                        break;
                    case "Y":
                        _pairs.Add(new Token(Token.Types.Y, Convert.ToInt32(pair.Item2.Value)));
                        break;
                    case "X":
                        _pairs.Add(new Token(Token.Types.X, Convert.ToInt32(pair.Item2.Value)));
                        break;
                    case "W":
                        _pairs.Add(new Token(Token.Types.W, Convert.ToInt32(pair.Item2.Value)));
                        break;
                    case "\\":
                        _pairs.Add(new Token(Token.Types.Down, Convert.ToInt32(pair.Item2.Value)));
                        break;
                    case "/":
                        _pairs.Add( new Token(Token.Types.Up, Convert.ToInt32(pair.Item2.Value)));
                        break;
                }
            }
        }

        private string GetName(int pos)
        {
            var pair = _pairs[pos];
            return pair.Type switch
            {
                Token.Types.Begin => "begin",
                Token.Types.Y => "State " + pair.Data.ToString() + "##" + pos.ToString(),
                Token.Types.X => "Statement " + pair.Data.ToString() + "##" + pos.ToString(),
                _ => ""
            };
        }

        public void Optimize()
        {
            bool afterW = false;
            for(int i = 0; i < _pairs.Count; i++)
                if(_pairs[i].Type == Token.Types.Down)
                    afterW = false;
                else if(afterW)
                    _pairs.RemoveAt(i--);
                else if(_pairs[i].Type == Token.Types.W)
                    afterW = true;

            if(_pairs.All(x => x.Type != Token.Types.End))
                throw new Exception("Нет конечного состояния");
            
            // convert sequence like "w(1)\(1)" to "\(1)"
            for(int i = 0; i < _pairs.Count; i++)
                if(_pairs[i].Type == Token.Types.W)
                    if(_pairs[i+1].Type == Token.Types.Down && _pairs[i].Data == _pairs[i+1].Data)
                        _pairs.RemoveAt(i--);
        }

        private INode Gen(int pos, ref int maxDepth)
        {
            INode? nodesOfGraph = null;
            var m1 = ++maxDepth;
            var pair = _pairs[pos];

            if(pair.Type == Token.Types.Begin)
                nodesOfGraph = new BeginNode(Gen(pos + 1,ref m1));
            else if(pair.Type == Token.Types.End)
                nodesOfGraph = _endNode;
            else if(pair.Type == Token.Types.Y)
                nodesOfGraph = new StateNode(GetName(pos),pair.Data,Gen(pos + 1,ref m1));
            else if(pair.Type == Token.Types.X)
            {
                m1++;
                nodesOfGraph = new StatementNode(GetName(pos), pair.Data,
                    Gen(pos + 2, ref m1), Gen(pos + 1, ref pos));
            }
            else if(pair.Type == Token.Types.W || pair.Type == Token.Types.Up)
            {
                nodesOfGraph = new EdgeNode(pair.Data);
                if(!_from.ContainsKey(_pairs[pos].Data))
                    _from.Add(_pairs[pos].Data,new List<INode>());
                _from[_pairs[pos].Data].Add(nodesOfGraph);
            }
            else if(pair.Type == Token.Types.Down)
            {
                if(_to.ContainsKey(pair.Data))
                    throw new Exception("double defined \\");
                nodesOfGraph = Gen(pos+1,ref m1);
                var name = "";
                if(_pairs[pos - 1].Type == Token.Types.Begin || _pairs[pos - 1].Type == Token.Types.Y)
                    name = GetName(pos - 1);
                if(!_to.ContainsKey(pair.Data))
                    _to.Add(pair.Data,new ());
                _to[pair.Data] = (nodesOfGraph,name);
            }
            maxDepth = m1;
            return nodesOfGraph;
        }

        public void MainGen(Graph graph)
        {
            _endNode = new EndNode();
            List<INode> nodes = new List<INode>();
            int pos = 0;
            while(pos < _pairs.Count)
                nodes.Add(Gen(pos,ref pos));

            if(_from.Count != _to.Count)
                throw new Exception("from.size() != to.size()");

            foreach (var item in _to)
                if(! _from.ContainsKey(item.Key))
                    throw new Exception("not from.contains(item.first)");

            foreach (var item in _to)
            {
                if(item.Value.Item1 == null)
                    throw new Exception("null ptr");
                var arr = _from[item.Key];
                if(item.Value.Item2 == "")
                {
                    for(int i = 1; i < arr.Count;i++)
                        ((EdgeNode)arr[i]).Set((arr[0].Name,item.Value.Item1.Name));
                    ((EdgeNode)arr[0]).Set(item.Value.Item1.Name);
                    ((EdgeNode)arr[0]).NextNode = item.Value.Item1;
                }
                else
                    foreach (var t in arr)
                        ((EdgeNode)t).Set((item.Value.Item2,item.Value.Item1.Name));
            }
            
            if(nodes.Count != 0)
            {
                Stack<string> stackOfNames = new Stack<string>();
                nodes[0].DrawVertex(graph);
                nodes[0].DrawEdges(graph,stackOfNames,false,"");
            }
        }
    };

    public void CreateGraph(Graph graph)
    {
        GraphCreator creator = new GraphCreator();
        creator.ConvertTokens(_pairOfTokens);
        creator.Optimize();
        graph.Clear();
        creator.MainGen(graph);
    }

    public void Generator(string symbols)
    {
        Lexer(symbols);
    }
};

public class LSAwindow
{
    private string _lsaText = "";
    private string _str = "";
    private string _err = "";
    
    public void Draw(ref Graph graph)
    {
        if( ImGui.Begin("LSA"))
        {
            if (ImGui.InputText("file",ref _lsaText,256,ImGuiInputTextFlags.EnterReturnsTrue))
            {
                if(File.Exists(_lsaText))
                {
                    StreamReader stream = new(_lsaText);
                    _str = stream.ReadToEnd();
                }
            }
            var size = ImGui.GetWindowSize();
            size.Y -= ImGui.GetFrameHeight() * 6;
            ImGui.InputTextMultiline("str",ref _str,1024,size);
            ImGui.Text("Данные будут утерены, если нажать кнопку принять");
            if (ImGui.Button("Принять"))
            {
                LSA lsa = new LSA();
                    lsa.Generator(_str);
                    lsa.CreateGraph(graph);
                try
                {

                }
                catch (Exception except)
                {
                    _err = except.Message;
                }
            }
            ImGui.Text(_err);
        }
        ImGui.End();
    }
}