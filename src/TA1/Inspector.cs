using ImGuiNET;
namespace TA1;

public class Inspector
{
    public Graph? Graph { get; init; }
    //string tmpPrjFile = "";
    readonly string[] _types = new string[4]{"Begin_node","End_node","State_node","Statement_node"};
    int _curIdCombo = 0;
    private string _nameNode = "", _toDelNameNode = "";
    string _from1 = "", _to1 = "" ,_text1 = "", _from2 = "", _toFirst = "", _toSecond = "",_text2 = "";
    int _isArrow1 = 0;
    int _isArrow2 = 0;
    
    public void Draw()
    {
        if (ImGui.Begin("Inspector"))
        {
            /*
            ImGui.InputText("projectFile",ref tmpPrjFile,256);

            if(ImGui.Button("Create project file"))
            {
                //TODO load serialize
                //graph.get().serialize(saves);
                //graph.get().clear();
                //adapter.save(prjFile,saves);
                //prjFile = tmpPrjFile;
                //utils.Config.appendSaveable("LastFile",prjFile);
            }
            ImGui.SameLine();
            if(ImGui.Button("Open project file"))
            {
                if(File.Exists(tmpPrjFile))
                {
                    //utils.ILoader.Value saves;
                    //graph.get().serialize(saves);
                    //graph.get().clear();
                    //adapter.save(prjFile,saves);
                    //prjFile = tmpPrjFile;
                    //auto obj = adapter.load(prjFile);
                    //graph.get().deserialize(obj);
                    //utils.Config.appendSaveable("LastFile",prjFile);
                }
            }
            ImGui.SameLine();
            if(ImGui.Button("Save project file"))
            {
                //utils.ILoader.Value saves;
                //graph.get().serialize(saves);
                //adapter.save(prjFile,saves);
            }
            */
            if (ImGui.CollapsingHeader("Создать вершину")) {
                var combo_preview_value = _types[_curIdCombo];  // Pass in the preview value visible before opening the combo (it could be anything)
                if (ImGui.BeginCombo("type of node##", combo_preview_value)) {
                    for (int n = 0; n < _types.Length; n++) {
                        var is_selected = (_curIdCombo == n);
                        if (ImGui.Selectable(_types[n], is_selected))
                            _curIdCombo = n;

                        // Set the initial focus when opening the combo (scrolling + keyboard navigation focus)
                        if (is_selected)
                            ImGui.SetItemDefaultFocus();
                    }
                    ImGui.EndCombo();
                }
                ImGui.InputText("name of node", ref _nameNode, 256);
                if (ImGui.Button("Create")) {
                    var ptr = Graph.Create(_types[_curIdCombo]);
                    //ptr.Position.X = Center.pos.x;
                    //ptr->y = Center.pos.y;
                    Graph?.Add(ptr, _nameNode);
                }
            }
            if (ImGui.CollapsingHeader("Удалить вершину")) {

                ImGui.InputText("name of node##delete",ref _toDelNameNode,256);
                if (ImGui.Button("Delete"))
                    Graph?.DeleteNode(_toDelNameNode);
            }
            if (ImGui.CollapsingHeader("Создать связь между вершинами")) {
                ImGui.InputText("from", ref _from1, 256);
                ImGui.InputText("to", ref _to1, 256);
                ImGui.InputText("text", ref _text1, 256);
                ImGui.SliderInt("isArrow", ref _isArrow1, 0, 1);
                if (ImGui.Button("Connect")) {
                    Graph?.Connect(_from1, _to1, Convert.ToBoolean(_isArrow1), _text1);
                }
            }
            if (ImGui.CollapsingHeader("Создать связь с ребром")) {
                ImGui.InputText("from##1",ref _from2, 256);
                ImGui.InputText("to.first##1", ref _toFirst, 256);
                ImGui.InputText("to.second##1", ref _toSecond, 256);
                ImGui.InputText("text##1", ref _text2, 256);
                ImGui.SliderInt("isArrow##1", ref _isArrow2, 0, 1);
                if (ImGui.Button("Connect##1")) {
                    Graph?.Connect(_from2, (_toFirst, _toSecond), Convert.ToBoolean(_isArrow2), _text2);
                }
            }
        }
        ImGui.End();
    }

}

