using DotRecast.Core;
using ImGuiNET;

namespace DotRecast.Recast.Demo.UI;

public class RcMenuView : IRcView
{
    private RcCanvas _canvas;

    public void Bind(RcCanvas canvas)
    {
        _canvas = canvas;
    }

    public void Update(double dt)
    {
        //throw new System.NotImplementedException();
    }

    public void Draw(double dt)
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("Help"))
            {
                if (ImGui.MenuItem("Repository"))
                {
                    RcProcess.OpenUrl("https://github.com/ikpil/DotRecast");
                }
                
                if (ImGui.MenuItem("Nuget"))
                {
                    RcProcess.OpenUrl("https://www.nuget.org/packages/DotRecast.Core/");
                }

                ImGui.Separator();
                if (ImGui.MenuItem("Issue Tracker"))
                {
                    RcProcess.OpenUrl("https://github.com/ikpil/DotRecast/issues");
                }

                if (ImGui.MenuItem("Release Notes"))
                {
                    RcProcess.OpenUrl("https://github.com/ikpil/DotRecast/blob/main/CHANGELOG.md");
                }

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }
    }
}