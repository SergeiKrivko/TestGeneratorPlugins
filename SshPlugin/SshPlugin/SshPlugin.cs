using System.Collections.ObjectModel;
using SshPlugin.Services;
using SshPlugin.Ui;
using TestGenerator.Shared.Settings;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace SshPlugin;

public class SshPlugin : TestGenerator.Shared.Plugin
{
    public static string DataPath { get; } = AAppService.Instance.GetDataPath();
    public static SettingsSection Settings { get; } = AAppService.Instance.GetSettings();
    public static Logger Logger { get; } = AAppService.Instance.GetLogger();

    public static ObservableCollection<SshConnection> Connections { get; } = [];
    
    public SshPlugin()
    {
        MainTabs = [];
        SideTabs = [];

        BuildTypes = [];
        ProjectTypes = [];

        SettingsControls = [new SettingsNode("SSH", new SshPage())];

        Connections.CollectionChanged += (sender, args) =>
            Settings.Set("connections", Connections.Select(c => c.Id).ToArray());
    }

    public override async Task Init()
    {
        foreach (var id in Settings.Get<Guid[]>("connections", []))
        {
            var connection = new SshConnection(id);
            Connections.Add(connection);
            if (connection.IsEnabled)
                await connection.Init();
        }
    }

    public override async Task Destroy()
    {
        foreach (var connection in Connections)
        {
            connection.Disconnect();
        }
    }
}