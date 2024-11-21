using System.Diagnostics;
using System.Reflection;
using TestGenerator.Shared.Types;

namespace MarkdownReports;

public class ToDocxAction : IFileAction
{
    public string Key => "MarkdownReportToDocx";
    public string Name => "Конвертировать в docx";
    public string[]? Extensions => [".md"];

    private string _scriptPath =
        Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets/converter/converter") +
        (OperatingSystem.IsWindows() ? ".exe" : "");

    public async Task Run(string path)
    {
        var proc = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _scriptPath,
                Arguments =
                    $"{path} {Path.Join(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".docx")}",
                RedirectStandardError = true,
                UseShellExecute = false,
            }
        };
        proc.Start();
        await proc.WaitForExitAsync();
        if (proc.ExitCode != 0)
            throw new Exception(await proc.StandardError.ReadToEndAsync());
    }
}