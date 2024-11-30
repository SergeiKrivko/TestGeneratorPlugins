using TestGenerator.Shared.SidePrograms;

namespace WslPlugin;

public class WslPlugin : TestGenerator.Shared.Plugin
{
    public WslPlugin()
    {
        MainTabs = [];
        SideTabs = [];

        BuildTypes = [];
        ProjectTypes = [];
        
        SideProgram.VirtualSystems.Add(new Wsl());
    }
}