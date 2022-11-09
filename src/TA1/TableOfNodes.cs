using ImGuiNET;
namespace TA1;

public class TableOfNodes
{
    private Graph _graph;

    public TableOfNodes(Graph graph)
    {
        _graph = graph;
    }

    public void Draw()
    {
        if( ImGui.Begin("Table"))
        {
            if(ImGui.BeginTable("Table1",3))
            {
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Type");
                ImGui.TableSetupColumn("Next");
                ImGui.TableHeadersRow();
                foreach (var node in _graph.Nodes)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(node.Key);
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text(node.Value.Type.ToString());
                    ImGui.TableSetColumnIndex(2);
                    ImGui.Text("");
                    foreach (var nexts in node.Value.Nexts)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text("\t"+node.Key);
                        ImGui.TableSetColumnIndex(1);
                        ImGui.Text(node.Value.Type.ToString());
                        ImGui.TableSetColumnIndex(2);
                        ImGui.Text((nexts.To + " | " + nexts.Text + " | " + ((nexts.IsArrow)? ("is arrow"): ("not arrow"))));
                    }
                }
                ImGui.EndTable();
            }
        }
        ImGui.End();
    }
}