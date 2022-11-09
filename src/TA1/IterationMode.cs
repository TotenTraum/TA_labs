using System.Collections;
using System.Numerics;
using ImGuiNET;

namespace TA1;

public class IterationMode
{
    private readonly Graph _graph;
    private GraphIterator _it;
    private int _itIsCount;
    private string _startPos, _queueList1 = "";
    private string _error1;
    readonly List<List<string>> _paths;
    private readonly List<List<int>> _inputDates; 
    private readonly List<string> _modes;
    private int _curIdCombo;
    private int _prevIdCombo;
    private int _val;
    private int _maxPathColumn;

    public IterationMode(Graph graph)
    {
        _graph = graph;
        _itIsCount = 0;
        _startPos = "";
        _paths = new List<List<string>>();
        _modes = new List<string>{"первый режим","второй режим","третий режим"};
        _curIdCombo = 0;
        _val = 0;
        _it = _graph.GetIterator();
        _paths.Add(new List<string>());
        _inputDates = new();
        _error1 = string.Empty;
    }

    //Демонстрация режима 1
    private void Mode1()
    {
        ImGui.SliderInt("input", ref _val, 0, 1);
        if (ImGui.Button("Prev <|")) {
            _error1 = "";
            if(_it.CurrentNode != null)
            {
                if (_it.HasPrev())
                    try
                    {
                        _it.Prev();
                        _paths[0].Remove(_paths[0].Last());
                    }
                    catch (Exception err)
                    {
                        _error1 = err.Message;
                    }
            }
            else
                _error1 = "Не выбран начальный узел";
        }
        ImGui.SameLine();
        if (ImGui.Button("Next |>##1"))
        {
            if (_it.CurrentNode != null)
            {
                (bool res, _error1) = IterNext(ref _it, _val);
                if (res)
                    _paths[0].Add(_it.GetPrettyName() ?? string.Empty);
            }
            else
                _error1 = "Не выбран начальный узел";
        }

        if(_paths.Count == 0)
            _paths.Add(new ());
        if(_paths[0].Count != 0)
        {
            ImGui.Text("Путь: ");
            foreach (var item in _paths[0])
                ImGui.Text(item);
        }
        ImGui.Text(_error1);
    }
    
    //Демонстрация режима 2
    private void Mode2()
    {
        ImGui.InputTextMultiline("##queueLine",ref _queueList1,256, new Vector2(ImGui.GetWindowSize().X/2,128));
        if (ImGui.Button("Next |>##2")) {
            if (_it.CurrentNode != null)
            {
                _it.ReturnToStart();
                if(_paths.Count == 0)
                    _paths.Add(new());
                _paths[0].Clear();
                if (_queueList1.Length > 0)
                {
                    var list = _queueList1.Split(" ",
                            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList()
                        .Select(x => Convert.ToInt32(x)).ToList();
                    GeneratePath(list);
                }
            }
            else
                _error1 = "Не выбран начальный узел";
        }

        if(_paths.Count == 0)
            _paths.Add(new ());
        if (_paths[0].Count != 0)
        {
            ImGui.Text("Путь: ");
            foreach (var item in _paths[0])
                ImGui.Text(item);
        }
        if(_error1.Length != 0)
        {
            ImGui.Text("error: ");
            ImGui.SameLine();
            ImGui.Text(_error1);
        }
    }

    //Демонстрация режима 3
    private void Mode3()
    {
        if (ImGui.Button("Next |>##3"))
        {
            if (_it.CurrentNode != null)
            {
                _inputDates.Clear();
                _paths.Clear();
                
                var count = _graph.CountStatement();
                var n = (int)Math.Pow(2, count);
                GenerateInputDatas(count, n);

                foreach (var t in _inputDates)
                {
                    _it.ReturnToStart();
                    _paths.Add(new List<string>());
                    _paths.Last().Add(_it.GetPrettyName() ?? string.Empty);
                    GeneratePath(t);
                }

                _maxPathColumn = _paths.Max(path => path.Count);
            }
            else
                _error1 = "Не выбран начальный узел";
        }

        if (_inputDates.Count > 0)
        {
            if(ImGui.BeginTable("Table1",_inputDates[0].Count))
            {
                ImGui.TableHeadersRow();
                foreach (var row in _inputDates)
                {
                    ImGui.TableNextRow();
                    for (int i = 0; i < row.Count; i++)
                    {
                        ImGui.TableSetColumnIndex(i);
                        ImGui.Text(row[i].ToString());
                    }
                }
                ImGui.EndTable();
            }
            if(ImGui.BeginTable("Table2",_maxPathColumn))
            {
                ImGui.TableHeadersRow();
                foreach (var row in _paths)
                {
                    ImGui.TableNextRow();
                    for (int i = 0; i < row.Count; i++)
                    {
                        ImGui.TableSetColumnIndex(i);
                        ImGui.Text(row[i]);
                    }
                }
                ImGui.EndTable();
            }
        }
        if(_error1.Length != 0)
        {
            ImGui.Text("error: ");
            ImGui.SameLine();
            ImGui.Text(_error1);
        }
    }

    //Раздел с начальной позицией
    private void BeginPosition()
    {
        if (ImGui.InputText("Start Node", ref _startPos, 256, ImGuiInputTextFlags.EnterReturnsTrue)) {
            _error1 = "";
            _it.SetStartNode(_startPos);
            foreach (var item in _paths)
            {
                item.Clear();
                item.Add(_it.GetPrettyName() ?? string.Empty);
            }
        }
        if (ImGui.Button("to start")) {
            _error1 = "";
            try
            {
                _it.ReturnToStart();
                foreach (var item in _paths)
                    item.Clear();
            } catch (Exception err) {
                _error1 = err.Message;
            }
        }
    }

    //Вывод combobox с режимами итерации
    private void ComboBox()
    {
        var combo_preview_value = _modes[_curIdCombo];
        if (ImGui.BeginCombo("##Режим##", combo_preview_value)) {
            for (int n = 0; n < _modes.Count; n++) {
                bool is_selected = (_curIdCombo == n);
                if (ImGui.Selectable(_modes[n], is_selected))
                {
                    _prevIdCombo = _curIdCombo;
                    if(_prevIdCombo == _curIdCombo)
                    {
                        _error1 = "";
                        _it.ReturnToStart();
                        _paths.Clear();
                        if (_curIdCombo is 0 or 1)
                        {
                            _paths.Add(new List<string>());
                            _paths[0].Add(_it.GetPrettyName()?? String.Empty);
                        }

                    }
                    _curIdCombo = n;
                }

                if (is_selected)
                    ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }
    }

    //Отрисовка окна
    public void Draw()
    {
        if (ImGui.Begin("input")) {
            if (ImGui.Button("reload"))
                _graph.ReloadEdges();

            if (ImGui.CollapsingHeader("Проверка на зацикливание")) {
                if (ImGui.SliderInt("count edges", ref _itIsCount, 0, 1))
                    _it.IsCountingEdges = _itIsCount > 0;

                ImGui.InputInt("max count value", ref _it.MaxCountValue);
            }
            if (ImGui.CollapsingHeader("Начальная позиция")) 
                BeginPosition();

            if (ImGui.CollapsingHeader("Итерации")) {
                ComboBox();            
                switch (_curIdCombo)
                {
                    case 0:
                        Mode1();
                        break;
                    case 1:
                        Mode2();
                        break;
                    case 2:
                        Mode3();
                        break;
                }
            }
        }
    }
    
    //Генерация набора входных данных
    private void GenerateInputDatas(int count, int n)
    {
        BitArray array = new BitArray(count);
        void Increment(ref BitArray bArray)
        {
            var prev = true;
            for (var i = 0; i < count; i++)
            {
                if(prev == false)
                    break;
                if (bArray[i])
                    bArray[i] = false;
                else
                {
                    bArray[i] = true;
                    prev = false;
                }
            }
        }

        for (var i = 0; i < n; i++)
        {
            _inputDates.Add(new List<int>());
            for (int j = 0; j < array.Length; j++)
                _inputDates[_inputDates.Count() - 1].Add(Convert.ToInt32(array[j]));
            Increment(ref array);
        }
    }

    //Генерация пути для входных данных 
    private void GeneratePath(IEnumerable<int> inputData)
    {
        var queue = new Queue<int>(inputData);
        _error1 = "";
        while (_error1.Length == 0)
        {
            (var res, _error1) = IterNext(ref _it, ref queue);
            if (res && _error1.Length == 0)
                _paths.Last().Add(_it.GetPrettyName() ?? string.Empty);
            else
                break;
        }

    }

    private (bool,string) IterNext(ref GraphIterator it, ref Queue<int> val) {
        var err = "";
        if (it.HasNext())
            try {
                it.Next(val);
                return (true,err);
            }
            catch (Exception err1) {
                err = err1.Message;
            }
        else
            err = "Последний узел";
        return (false,err);
    }

    private (bool, string) IterNext(ref GraphIterator it, int val)
    {
        Queue<int> path = new Queue<int>();
        path.Enqueue(val);
        return IterNext(ref it,ref path);
    }    
}