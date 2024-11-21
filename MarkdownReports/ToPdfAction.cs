using System.Diagnostics;
using System.Reflection;
using TestGenerator.Shared.Types;

namespace MarkdownReports;

public class ToPdfAction : IFileAction
{
    public string Key => "MarkdownReportToPdf";
    public string Name => "Конвертировать в pdf";
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
                    $"{path} {Path.Join(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".pdf")}",
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