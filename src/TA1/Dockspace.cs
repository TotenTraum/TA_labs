using System.Numerics;
using ImGuiNET;
namespace TA1;

public class Dockspace
{
    public void Draw()
    {
        ImGuiDockNodeFlags dockspace_flags = ImGuiDockNodeFlags.None | ImGuiDockNodeFlags.PassthruCentralNode;
        const ImGuiWindowFlags flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove| ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus
                                       | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoCollapse;
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        ImGui.SetNextWindowViewport(viewport.ID);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        bool b = true;
        var Color = ImGui.GetStyle().Colors[2];
        bool p_open = true;
        ImGui.Begin("Dockspace", ref p_open, flags);
        ImGui.PopStyleVar(3);
        ImGuiIOPtr io = ImGui.GetIO();
        if ((io.ConfigFlags & ImGuiConfigFlags.DockingEnable) != ImGuiConfigFlags.None)
        {
            uint dockspace_id = ImGui.GetID("Dockspace");
            ImGui.GetStyle().Colors[2] = Vector4.Zero;
            ImGui.DockSpace(dockspace_id, Vector2.Zero, dockspace_flags);
        }
        if(ImGui.BeginMenuBar()) {
            if (ImGui.BeginMenu("menu")) {
                if(ImGui.MenuItem("Fullscreen"))
                    b = false;
                ImGui.EndMenu();
            }
            ImGui.EndMenuBar();
        }
        ImGui.End();
        ImGui.GetStyle().Colors[2] = Color;
    }
}