using Newtonsoft.Json;
using TestGenerator.Shared.Types;

namespace TestPlugin.UI;

public partial class TestMainTab : MainTab
{
    public TestMainTab()
    {
        TabKey = "Newtonsoft.Json";
        TabName = "Newtonsoft.Json";
        InitializeComponent();

        Block.Text = JsonConvert.SerializeObject(new {A = 1, B = 2, C = 3, D = 4, E = 5});
    }
}