using Avalonia.Threading;
using TestGenerator.Shared.SidePrograms;
using TestGenerator.Shared.Types;

namespace LangPython;

public class FastRunAction : IFileAction
{
    public string Key => "RunPython";
    public string Name => "Запустить";
    public string? Icon => "";
    public string[] Extensions => [".py"];
    public int Priority => 10;

    public async Task Run(string path)
    {
        var python = LangPython.Python.FromModel(
            LangPython.ProjectSettings.Get<ProgramFileModel>("interpreter") ??
            LangPython.Settings.Get<ProgramFileModel>("interpreter"));
        if (python == null)
            throw new Exception("Интерпретатор Python не найден");
        Dispatcher.UIThread.Post(() => Start(python.VirtualSystem.ExecuteInConsole($"{python.Path} {path}")));
    }

    private async void Start(ITerminalController controller)
    {
        AAppService.Instance.ShowSideTab("Run");
        await controller.RunAsync();
    }
}