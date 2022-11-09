using System.Numerics;
using ImGuiNET;
namespace TA1;
/// <summary>
/// Класс расширения
/// </summary>
public static class Utils{
    public static uint Convert(this ImColor color)
    {
        return ImGui.ColorConvertFloat4ToU32(color.Value);
    }
    
    public static ImColor Convert(byte r,byte g, byte b, byte a)
    {
        var rgba = ((uint)r << 0) | ((uint)g << 8) | ((uint)b << 16) | ((uint)a << 24);
        var col = new ImColor();
        col.Value = ImGui.ColorConvertU32ToFloat4(rgba);
        return col;
    }
}

[Serializable]
public struct Edge
{
    public string To { get; set; }
    public bool UseGotos { get; set; }
    public string Text { get; set; }
    public bool IsArrow { get; set; }

    public Edge()
    {
        To = "";
        UseGotos = false;
        Text = "";
        IsArrow = false;
    }
}

[Serializable]
public abstract class Node
{
    public enum Types
    {
        Begin,
        State,
        Statement,
        End,
        Edge
    }
    public Types Type;
    
    public List<Edge> Nexts { get; set; }
    public bool Repos { get; set; }
    protected bool DoubleClicked;
    protected ImColor Color { get; set; }
    public static ImColor ActivatedColor{ get; set; }
    public static ImColor NotActivatedColor{ get; set; }
    
    public string Name;
    public Int64 Id;
    public string UniqueName;
    public Vector2 Position, Size;
    static Node()
    {
        //ActivatedColor = Utils.Convert(128, 64, 64, 255);
        ActivatedColor = Utils.Convert(128, 64, 64, 255);

        NotActivatedColor = Utils.Convert(128, 128, 128, 255);
    }

    protected Node()
    {
        DoubleClicked = false;
        Name = "";
        UniqueName = "";
        Id = GetHashCode();
        Repos = false;
        Color = NotActivatedColor;
        Nexts = new List<Edge>();
        Size.X = 110;
        Size.Y = 35;
    }
    
    public void SetActive(bool val)
    {
        Color = val ? ActivatedColor : NotActivatedColor;
    }
    
    public abstract void Push(Edge edge);
    public abstract string GetName();
    public abstract bool CanPush(Edge edge);
    
    public void Remove(string name)
    {
        Nexts.RemoveAll((x)=> x.To == name);
    }
    
    public abstract void Draw();
}
[Serializable]
public class BeginNode : Node
{
    public BeginNode(string name = "")
    {
        Type = Types.Begin;
        Size = new Vector2(110, 35);
        UniqueName = "Begin#"+ Id.ToString();
        if(name == "")
            Name = UniqueName;
    }
    
    public override void Draw()
    {
        if(Repos)
        {
            ImGui.SetNextWindowPos(Position);
            Repos = false;
        }
        ImGui.SetNextWindowSize(Size);
        var flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground |
            ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoDocking;
        bool opened = true;
        ImGui.Begin(UniqueName,ref opened, flags);
        Position = ImGui.GetWindowPos();
        var startPos = ImGui.GetWindowPos();
        startPos.X += 5;
        var pos = new Vector2(Position.X + 100 , Position.Y + 35);
        
        ImGui.GetWindowDrawList().AddRectFilled(startPos,pos, Color.Convert(),10.0f);
        ImGui.Text("       Начало");
        ImGui.End();
    }
    
    public override void Push(Edge edge)
    {
        if(Nexts.Count == 0)
            Nexts.Add(edge);
    }

    public override string GetName()
    {
        return "Yb";
    }

    public override bool CanPush(Edge edge) => Nexts.Count == 0;
}
[Serializable]
public class EndNode : Node
{
    public EndNode(string name = "")
    {
        Type = Types.End;
        Size = new Vector2(110, 35);
        UniqueName = "End#"+ Id.ToString();
        if(name == "")
            Name = UniqueName;
    }
    
    public override void Draw()
    {
        if(Repos)
        {
            ImGui.SetNextWindowPos(Position);
            Repos = false;
        }
        ImGui.SetNextWindowSize(Size);
        var flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground |
            ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoDocking;
        var opened = true;
        ImGui.Begin(UniqueName,ref opened, flags);
        Position = ImGui.GetWindowPos();
        var startPos = ImGui.GetWindowPos();
        startPos.X += 5;
        var pos = new Vector2(Position.X + 100 , Position.Y + 35);
        ImGui.GetWindowDrawList().AddRectFilled(startPos,pos, Color.Convert(),10.0f);
        ImGui.Text("       Конец");
        ImGui.End();
    }
    
    public override void Push(Edge edge)
    {
        if(Nexts.Count == 0)
            Nexts.Add(edge);
    }

    public override string GetName()
    {
        return "Ye";
    }

    public override bool CanPush(Edge edge) => Nexts.Count == 0;
}
[Serializable]
public class StateNode : Node
{
    public int Number;
    
    public StateNode(int num,string name = "")
    {
        Number = num;
        Type = Types.State;
        Size = new Vector2(100, 50);
        UniqueName = "State##"+ Id.ToString();
        if(name == "")
            Name = UniqueName;
    }
    
    public override void Draw()
    {
        if(Repos)
        {
            ImGui.SetNextWindowPos(Position);
            Repos = false;
        }
        ImGui.SetNextWindowSize(Size);
        ImGui.PushStyleColor(ImGuiCol.WindowBg,Color.Convert());
        var flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoDocking;
        var opened = true;
        ImGui.Begin(UniqueName, ref opened, flags );
        Position = ImGui.GetWindowPos();
        ImGui.Text($"      Y{Number}");
        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && ImGui.IsWindowFocused())
            DoubleClicked = true;
        ImGui.End();
        ImGui.PopStyleColor(1);
        if(DoubleClicked) {
            if (ImGui.Begin("Edit " + UniqueName, ref opened, ImGuiWindowFlags.NoSavedSettings)) {
                ImGui.InputInt("number", ref Number);
            }
            ImGui.End();
        }
    }
    
    public override void Push(Edge edge)
    {
        if(Nexts.Count == 0)
            Nexts.Add(edge);
    }

    public override string GetName()
    {
        return "Y" + Number;
    }

    public override bool CanPush(Edge edge) => Nexts.Count == 0;
}
[Serializable]
public class StatementNode : Node
{
    private int _number;
    
    public StatementNode(int num,string name = "")
    {
        _number = num;
        Type = Types.Statement;
        Size = new Vector2(100, 50);
        UniqueName = "Statement##"+ Id.ToString();
        if(name == "")
            Name = UniqueName;
    }
    
    public override void Draw()
    {
        if(DoubleClicked) {
            if (ImGui.Begin("Edit " + UniqueName, ref DoubleClicked))
                ImGui.InputInt("number", ref _number);
            ImGui.End();
        }

        if(Repos)
        {
            ImGui.SetNextWindowPos(Position);
            Repos = false;
        }
        ImGui.SetNextWindowSize(Size);
        var flags = ImGuiWindowFlags.NoTitleBar| ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDocking |
            ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoResize;
        var opened = true;
        ImGui.Begin(UniqueName, ref opened, flags);
        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && ImGui.IsWindowFocused())
            DoubleClicked = true;
        Position = ImGui.GetWindowPos();
        var startPos = ImGui.GetWindowPos();
        var vec = new Vector2[4];
        vec[0].X = startPos.X + 0;
        vec[0].Y = startPos.Y + 25;
        vec[1].X = startPos.X + 50;
        vec[1].Y = startPos.Y + 50;
        vec[2].X = startPos.X + 100;
        vec[2].Y = startPos.Y + 25;
        vec[3].X = startPos.X + 50;
        vec[3].Y = startPos.Y + 0;
        ImGui.GetWindowDrawList().AddConvexPolyFilled( ref vec[0], vec.Length,Color.Convert());
        ImGui.Text($"       X{_number}");
        ImGui.End();
    }
    
    public override void Push(Edge edge)
    {
        if(Nexts.Count == 2)
            return;
        
        switch (edge.Text)
        {
            case "1":
                if (Nexts.Count == 0)
                {
                    Nexts.Add(edge);
                    return;
                }
                if(Nexts[0].Text == "0")
                    Nexts.Add(edge);
                break;
            case "0":
                if (Nexts.Count == 0)
                {
                    Nexts.Add(edge);
                    return;
                }
                if(Nexts[0].Text == "1")
                    Nexts.Add(edge);
                break;
        }
    }

    public override string GetName()
    {
        return "X" + _number;
    }

    public override bool CanPush(Edge edge)
    {
        if(Nexts.Count == 2)
            return false;

        switch (edge.Text)
        {
            case "1":
                if(Nexts.Count == 0)
                    return true;
                return Nexts[0].Text == "0";
            case "0":
                if(Nexts.Count == 0)
                    return true;
                return Nexts[0].Text == "1";
            default:
                return false;
        }
    }
}
[Serializable]
public class EdgeNode : Node
{
    private readonly float _rad;
    
    public EdgeNode(Vector2 pos)
    {
        Position = pos;
        _rad = 10; 
        Type = Types.Edge;
        Size = new Vector2(100, 50);
        UniqueName = "EdgeNode##"+ Id.ToString();
        Repos = true;
    }
    
    public void Draw(Vector2 sizes,Vector2 pTo,Vector2 pFrom, string text, bool isArrow)
    {
        Vector2 center = Position;
        var flags = ImGuiWindowFlags.NoTitleBar| ImGuiWindowFlags.NoDocking |
                     ImGuiWindowFlags.NoSavedSettings| ImGuiWindowFlags.NoResize;
        bool opened = true;
        ImGui.Begin(UniqueName + "lines",ref opened,
            flags | ImGuiWindowFlags.NoMouseInputs | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBackground );
        Vector2 zero = new Vector2(0, 0);
        ImGui.SetWindowPos(zero);
        ImGui.SetWindowSize(sizes);
        var black = Utils.Convert(0, 0, 0, 255);
        var white = Utils.Convert(255, 255, 255, 255);

        ImGui.GetWindowDrawList().AddLine(pFrom,center,black.Convert() ,3);
        ImGui.GetWindowDrawList().AddLine(center,pTo, black.Convert(),3);
        //ImGui.SetCursorPos(center);
        //ImGui.Text(text);
        Vector2 pMin = new Vector2(Math.Min(center.X, pTo.X), Math.Min(center.Y, pTo.Y));
        Vector2 pDiff = new Vector2( Math.Max(center.X, pTo.X) - pMin.X,  Math.Max(center.Y, pTo.Y) - pMin.Y);
        Vector2 pCenter = new Vector2(pMin.X + pDiff.X / 2, pMin.Y + pDiff.Y / 2);
        if(isArrow) {
            double angle = Math.Atan((pTo.Y - center.Y) / (pTo.X - center.X));
            double b = (pTo.X * center.Y - center.Y * pTo.X) / (center.X - pTo.X);
            double halfPi = Math.PI / 2;
            angle += (angle >= 0) ? -(halfPi) : (halfPi);

            Vector2 pHalfCenter = pCenter;
            if (center.X > pTo.X)
                pHalfCenter.X += pDiff.X / 4;
            else if (center.X < pTo.X)
                pHalfCenter.X -= pDiff.X / 4;

            if (center.Y > pTo.Y)
                pHalfCenter.Y += pDiff.Y / 5;
            else if (center.Y < pTo.Y)
                pHalfCenter.Y -= pDiff.Y / 5;
            var func = new Func<double, double, double, double>((x, k, y) => x * k + y);
            var y1 = Math.Clamp(func(10.0f, Math.Tan(angle), b), -15.0f, 15.0f);
            var y2 = Math.Clamp(func(-10.0f, Math.Tan(angle), b), -15.0f, 15.0f);
            Vector2 p1 = new Vector2(pHalfCenter.X + 10.0f, (float)(pHalfCenter.Y + y1));
            Vector2 p2 = new Vector2(pHalfCenter.X - 10.0f, (float)(pHalfCenter.Y + y2));
            ImGui.GetWindowDrawList().AddLine(pCenter, p1, black.Convert(), 3);
            ImGui.GetWindowDrawList().AddLine(pCenter, p2, black.Convert(), 3);
        }

        var textCenter = new Vector2(center.X + _rad, center.Y + _rad);
        ImGui.GetWindowDrawList().AddText(textCenter,white.Convert(),text);
        ImGui.End();
        center = new Vector2(Position.X + 3,Position.Y);

        ImGui.Begin(UniqueName,ref opened,flags | ImGuiWindowFlags.NoBackground);
        if(Repos)
        {
            var pos = Position;
            pos.X -= _rad;
            pos.Y -= _rad;
            ImGui.SetWindowPos(pos);
            Repos = false;
        }
        var winSize = new Vector2(_rad,_rad);
        winSize.X *= 2.3f;
        winSize.Y *= 2.3f;
        ImGui.SetWindowSize(winSize);
        ImGui.GetWindowDrawList().AddCircleFilled(center,_rad, black.Convert());
        var posWindow = ImGui.GetWindowPos();
        posWindow.X += _rad;
        posWindow.Y += _rad;
        Position = posWindow;
        ImGui.End();
    }
    
    public override void Push(Edge edge) {}
    public override string GetName()
    {
        return "Edge";
    }

    public override bool CanPush(Edge edge)
    {
        
        return false;
    }

    public override void Draw()
    {
        throw new Exception();
    }
}

