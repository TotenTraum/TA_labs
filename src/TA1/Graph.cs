using System.Numerics;
using ImGuiNET;
using Newtonsoft.Json;

namespace TA1;

public class GraphIterator
{
    private readonly Graph _graph;
    private Stack<string> Names { get; }
    private readonly Dictionary<string, List<RefEdge>> _gotos;
    private readonly Dictionary<string, int> _counterVertex;
    public Node? CurrentNode;
    public bool IsCountingEdges;
    public int MaxCountValue;

    public GraphIterator(Graph graph)
    {
        _graph = graph;
        Names = new Stack<string>();
        _counterVertex = new Dictionary<string, int>();
        _gotos = new Dictionary<string, List<RefEdge>>();
        CurrentNode = null;
        IsCountingEdges = false;
        MaxCountValue = 20;
    }

    //Очистка счётчика посещённых вершин
    public void ClearCounter()
    {
        _counterVertex.Clear();
    }
    
    //Установить стартовую вершину
    public bool SetStartNode(string name)
    {
        if (!_graph.Nodes.ContainsKey(name)) return false;
        CurrentNode?.SetActive(false);
        CurrentNode = _graph.Nodes[name];
        CurrentNode.SetActive(true);
        _counterVertex.Clear();
        _counterVertex.Add(name,1);
        Names.Push(name);
        return true;
    }

    //Возвращает true, если не достигнута стартовая вершина 
    public bool HasPrev()
    {
        if(Names.Count > 1)
            return _graph.Nodes.ContainsKey(Names.Peek());
        return false;
    }

    //Возвращает красивое название вершины
    public string? GetPrettyName()
    {
        return CurrentNode?.GetName();
    }
    
    //Возвращается к стартовой вершине
    public void ReturnToStart()
    {
        while (Names.Count > 1)
            if (HasPrev())
                    Prev();
    }
    //Возвращается к предыдущей вершине
    public void Prev()
    {
        if(CurrentNode == null)
            throw new Exception("Выберите начальный узел");

        if(Names.Count > 1)
        {
            var curNode = Names.Pop();
            var prevNode = Names.Peek();
            if( _graph.Nodes.ContainsKey(prevNode))
            {
                if(_counterVertex[curNode] != 0)
                    _counterVertex[curNode]--;
                CurrentNode.SetActive(false);
                CurrentNode = _graph.Nodes[prevNode];
                CurrentNode.SetActive(true);
            }
            else
            {
                Names.Push(curNode);
                throw new Exception("Такой узел не зарегистрирован");
            }
        }
        else
            throw new Exception("Это первый элемент");
    }
    
    /// <summary>
    /// Переходит к следующим вершинам, используя входные данные
    /// </summary>
    /// <param name="inputData"> Входные данные</param>
    /// <exception cref="Exception"></exception>
    public void Next(Queue<int> inputData)
    {
        if(CurrentNode == null)
            return;

        switch (CurrentNode.Type)
        {
            case Node.Types.Statement:
            {
                foreach (var item in CurrentNode.Nexts.Where(item => inputData.Peek().ToString() == item.Text))
                {
                    if (item.UseGotos)
                    {
                        if (!_gotos.ContainsKey(Names.Peek())) continue;
                        var arr = _gotos[Names.Peek()];
                        foreach (var to in from tp in arr where tp.Text == item.Text select tp.To)
                        {
                            if(!_counterVertex.ContainsKey(to))
                                _counterVertex.Add(to,0);
                            if(IsCountingEdges && _counterVertex[to] + 1 == MaxCountValue)
                                throw new Exception("Программа возможно зацикливается");
                            _counterVertex[to]++;
                            CurrentNode.SetActive(false);
                            CurrentNode = _graph.Nodes[to];
                            Names.Push(to);
                            CurrentNode.SetActive(true);
                        }
                        inputData.Dequeue();
                    }
                    else
                    {
                        if(_graph.Nodes.ContainsKey(item.To))
                        {
                            if(IsCountingEdges && _counterVertex[item.To] + 1 == MaxCountValue)
                                throw new Exception("Программа возможно зацикливается");
                            if(! _counterVertex.ContainsKey(item.To))
                                _counterVertex.Add(item.To,0);
                            _counterVertex[item.To]++;
                            CurrentNode.SetActive(false);
                            CurrentNode = _graph.Nodes[item.To];
                            Names.Push(item.To);
                            CurrentNode.SetActive(true);
                            inputData.Dequeue();
                        }
                        else
                            throw new Exception("Такой узел не зарегистрирован");
                    }
                    return;
                }

                break;
            }
            case Node.Types.Edge:
                throw new Exception("Данный узел некорректен");
            case Node.Types.Begin:
            case Node.Types.State:
            case Node.Types.End:
            default:
            {
                if(CurrentNode.Nexts.Count != 0)
                {
                    if( _graph.Nodes.ContainsKey(CurrentNode.Nexts[0].To))
                    {
                        if(IsCountingEdges && _counterVertex[CurrentNode.Nexts[0].To] + 1 == MaxCountValue)
                            throw new Exception("Программа возможно зацикливается");
                        if(! _counterVertex.ContainsKey(CurrentNode.Nexts[0].To))
                            _counterVertex.Add(CurrentNode.Nexts[0].To, 0);
                        _counterVertex[CurrentNode.Nexts[0].To]++;
                        CurrentNode.SetActive(false);
                        Names.Push(CurrentNode.Nexts[0].To);
                        CurrentNode = _graph.Nodes[CurrentNode.Nexts[0].To];
                        CurrentNode.SetActive(true);

                    }
                    else
                        throw new Exception("Такой узел не зарегистрирован");
                }
                else
                    throw new Exception("Дальше нет узлов");

                break;
            }
        }
    }
    /// <summary>
    /// Переходит к следующим вершинам, используя входные данные
    /// </summary>
    /// <param name="inputData"> Входные данные</param>
    /// <exception cref="Exception"></exception>
    public void Next(int inputData)
    {
        var input = new Queue<int>(inputData); 
        Next( input);
    }

    //Возвращает true, если имеются переходы в другие вершины 
    public bool HasNext()
    {
        if(CurrentNode != null)
            return CurrentNode.Nexts.Count != 0;
        return false;
    }
}

struct RefEdge
{
    public string From;
    public string To;
    public bool IsArrow;
    public string Text;

    public RefEdge()
    {
        From = "";
        To = "";
        IsArrow = false;
        Text = "";
    }
}

public class Graph
{
    [JsonProperty("Nodes")]
    public Dictionary<string, Node> Nodes;
    [JsonProperty("Edges")]
    //[JsonConverter(typeof(TupleKeyConverter))]
    private Dictionary<Tuple<string, string>, EdgeNode> _edges;
    [JsonProperty("Gotos")]
    private Dictionary<string, List<RefEdge>> _gotos;

    private static Vector2 Sizes { get; set; }

    static Graph()
    {
        Sizes = new Vector2();
    }

    public int CountStatement()
    {
        return Nodes.Count(node => node.Value.Type == Node.Types.Statement);
    }

    public static void SetPaintArea(Vector2 size)
    {
        Sizes = size;
    }

    public Graph()
    {
        Nodes = new Dictionary<string, Node>();
        _edges = new Dictionary<Tuple<string, string>, EdgeNode>();
        _gotos = new Dictionary<string,List<RefEdge>>();
    }

    public void Clear()
    {
        Nodes.Clear();
        _edges.Clear();
        _gotos.Clear();
    }

    public int GetNumberOfStatement()
    {
        int counter = 0;
        foreach (var node in Nodes)
            if (node.Value.Type == Node.Types.Statement)
                counter++;
        return counter;
    }

    public bool Add(Node node, string name = "")
    {
        string Name;
        Name = name == "" ? node.UniqueName : name;

        if(Nodes.ContainsKey(Name))
            return false;

        Nodes[Name] = node;
        return true;
    }
    
    public void DeleteNode(string name)
    {
        Nodes.Remove(name);
        var listOfLinks =  _edges.Where((x) =>
                                                    x.Key.Item1 == name || x.Key.Item2 == name);
        List<KeyValuePair<string, RefEdge>> listOfRefs = new List<KeyValuePair<string, RefEdge>>();
        
        //TODO Завершить удаление узла
        /*
        foreach (var item in listOfLinks)
        {
            var .AddRange(Gotos.Where((x) =>
                x.Value.To == item.Key.Item1 && x.Value.From == item.Key.Item2));
            if (Edges.ContainsKey(item.Key))
                Edges.Remove(item.Key);
        }

        foreach (var refs in listOfRefs)
            if (Gotos.ContainsKey(refs.Key))
                Gotos.Remove(refs.Key);
        */
    }
    
    public bool Connect(string from, string to, bool isArrow = false, string text = "" )
    {
        if(Nodes[from].UniqueName == Nodes[to].UniqueName)
            return false;

        Edge edge = new Edge
        {
            Text = text,
            To = to,
            IsArrow = isArrow,
            UseGotos = false
        };
        if (! Nodes[from].CanPush(edge))
            return false;
        Nodes[from].Push(edge);
        Vector2 pCenter = CalcEdgeCenter(from,to);

        _edges[new Tuple<string, string>(from,to)] = new EdgeNode(pCenter);
        _edges[new Tuple<string, string>(from,to)].UniqueName += from + ";"+ to + "  " 
                                                                       + Random.Shared.Next().ToString();
        return true;
    }

    private Vector2 CalcEdgeCenter(string from, string to)
    {
        Vector2 p1 = new Vector2(Nodes[from].Position.X + Nodes[from].Size.X/2,
            Nodes[from].Position.Y + Nodes[from].Size.Y/2);
        Vector2 p2 = new Vector2(Nodes[to].Position.X + Nodes[to].Size.X/2,
            Nodes[to].Position.Y + Nodes[to].Size.Y/2);
        Vector2 pMin = new Vector2( Math.Min(p1.X,p2.X),Math.Min(p1.Y,p2.Y));
        Vector2 pDiff = new Vector2(Math.Max(p1.X,p2.X) - pMin.X,
                                    Math.Max(p1.Y,p2.Y) - pMin.Y);
        return new Vector2(pMin.X + pDiff.X/2,pMin.Y + pDiff.Y/2);
    }
    
    public bool Connect(string from, (string ,string) to, bool isArrow = false, string text = "" )
    {
        if(Nodes[to.Item1].UniqueName == Nodes[to.Item2].UniqueName)
            return false;

        Edge edge = new Edge
        {
            Text = text,
            To = to.Item2,
            IsArrow = isArrow,
            UseGotos = true
        };
        if(Nodes[from].CanPush(edge))
        {
            Nodes[from].Push(edge);
            RefEdge refEdge = new RefEdge
            {
                From = to.Item1,
                To = to.Item2,
                IsArrow = isArrow,
                Text = text
            };
            if(!_gotos.ContainsKey(from))
                _gotos.Add(from,new List<RefEdge>());
            _gotos[from].Add(refEdge);
            return true;
        }
        return false;
    }
    
    public void ReloadEdges()
    {
        foreach (var edge in _edges)
        {
            Vector2 pCenter = CalcEdgeCenter(edge.Key.Item1,edge.Key.Item2);
            edge.Value.Position = pCenter;
            edge.Value.Repos = true;
        }
    }

    private void DrawRefEdges(string from, Edge edge)
    {
        foreach (var line in _gotos[from])
        {
            if (edge.Text == line.Text)
            {
                var flags = ImGuiWindowFlags.NoTitleBar| ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize
                            | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoMouseInputs 
                            | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBackground;
                // TODO clone method
                var center = Nodes[from].Position;
                center.X += Nodes[from].Size.X / 2;
                center.Y += Nodes[from].Size.Y / 2;
                var pTo = _edges[new Tuple<string, string>(line.From, line.To)].Position;
                bool opened = true;
                var zeroPos = new Vector2(0, 0);
                var blackColor = Utils.Convert(0, 0, 0, 255);
                
                //Drawing edge
                ImGui.Begin("## EdgeToEdge"+from+line.From+line.To, ref opened, flags);
                ImGui.SetWindowPos(zeroPos);
                ImGui.SetWindowSize(Sizes);
                
                ImGui.GetWindowDrawList().AddLine(center,pTo, blackColor.Convert(),3);

                var pMin = new Vector2(Math.Min(center.X, pTo.X), Math.Min(center.Y, pTo.Y));
                var pDiff = new Vector2( Math.Max(center.X, pTo.X) - pMin.X,  Math.Max(center.Y, pTo.Y) - pMin.Y);
                var pCenter = new Vector2(pMin.X + pDiff.X / 2, pMin.Y + pDiff.Y / 2);
                if (edge.IsArrow)
                {
                    var angle = Math.Atan((pTo.Y - center.Y) / (pTo.X - center.X));
                    var b = (pTo.X * center.Y - center.Y * pTo.X) / (center.X - pTo.X);
                    var halfPi = Math.PI / 2;
                    angle += (angle >= 0) ? -(halfPi) : (halfPi);
                    
                    var pHalfCenter = pCenter;
                    if (center.X > pTo.X)
                        pHalfCenter.X += pDiff.X / 4;
                    else if (center.X < pTo.X)
                        pHalfCenter.X -= pDiff.X / 4;

                    if (center.Y > pTo.Y)
                        pHalfCenter.Y += pDiff.Y / 5;
                    else if (center.Y < pTo.Y)
                        pHalfCenter.Y -= pDiff.Y / 5;
                    var func = new Func<double, double, double, double>((x, k, b) => x * k + b);
                    double y1 = Math.Clamp(func(10.0f, Math.Tan(angle), b), -15.0f, 15.0f);
                    double y2 = Math.Clamp(func(-10.0f, Math.Tan(angle), b), -15.0f, 15.0f);
                    Vector2 p1 = new Vector2(pHalfCenter.X + 10.0f, (float)(pHalfCenter.Y + y1));
                    Vector2 p2 = new Vector2(pHalfCenter.X - 10.0f, (float)(pHalfCenter.Y + y2));
                    ImGui.GetWindowDrawList().AddLine(pCenter, p1, blackColor.Convert(), 3);
                    ImGui.GetWindowDrawList().AddLine(pCenter, p2, blackColor.Convert(), 3);
                }
                ImGui.SetCursorPos(pCenter);
                ImGui.Text(edge.Text);
                ImGui.End();

            }
        }
    }

    public void Draw()
    {
        //Drawing edges
        foreach (var node in Nodes)
            foreach (var next in node.Value.Nexts)
            {
                if(next.UseGotos)
                    DrawRefEdges(node.Key,next);
                else if(_edges.ContainsKey(new Tuple<string, string>(node.Key, next.To)))
                {
                    var edge = _edges[new Tuple<string, string>(node.Key, next.To)];
                    var p1 = node.Value.Position;
                    p1.X += node.Value.Size.X / 2;
                    p1.Y += node.Value.Size.Y / 2;
                    var p2 = Nodes[next.To].Position;
                    p2.X += Nodes[next.To].Size.X / 2;
                    p2.Y += Nodes[next.To].Size.Y / 2;
                    edge.Draw(Sizes, p2, p1, next.Text, next.IsArrow);
                }
            }
        //Drawing vertex
        foreach (var node in Nodes)
            node.Value.Draw();
    }
    
    public static Node Create(string str)
    {
        return str switch
        {
            "Begin_node" => new BeginNode(),
            "End_node" => new EndNode(),
            "State_node" => new StateNode(0),
            "Statement_node" => new StatementNode(0),
            "Edge_node" => new EdgeNode(new Vector2(0, 0)),
            _ => throw new Exception("not defined")
        };
    }
    public GraphIterator GetIterator()
    {
        return new GraphIterator(this);
    }
    
    //TODO добавить disconnects и ?move?
}