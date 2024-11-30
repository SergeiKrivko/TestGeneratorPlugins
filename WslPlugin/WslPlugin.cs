using TestGenerator.Shared.SidePrograms;

namespace WslPlugin;

public class WslPlugin : TestGenerator.Shared.Plugin
{
    private Wsl _wsl;
    
    public WslPlugin()
    {
        MainTabs = [];
        SideTabs = [];

        BuildTypes = [];
        ProjectTypes = [];
        
        SideProgram.VirtualSystems.Add(_wsl = new Wsl());
    }

    public override async Task Destroy()
    {
        SideProgram.VirtualSystems.Remove(_wsl);
    }
}