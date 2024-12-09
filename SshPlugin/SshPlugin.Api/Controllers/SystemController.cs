using System.Diagnostics;
using System.Text.Json;
using SshPlugin.Api.Models;
using SshPlugin.Api.Services;
using StdioBridge.Api.Attributes;

namespace SshPlugin.Api.Controllers;

[BridgeController("api/v1/system")]
public class SystemController
{
    private readonly FilesService _filesService;

    public SystemController(FilesService filesService)
    {
        _filesService = filesService;
    }

    [BridgeGet("runtime")]
    public async Task<string> GetRuntimeHandler()
    {
        return System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier;
    }

    [BridgePost("connectionId")]
    public async Task<Guid> PostConnectionId([FromBody] Guid connectionId)
    {
        Console.WriteLine($"Set connectionId = {connectionId}");
        _filesService.ConnectionId = connectionId;
        return _filesService.ConnectionId;
    }

    private async Task<CompletedProcess> RunProcess(RunProcessArgs args)
    {
        Process? proc;
        try
        {
            proc = Process.Start(new ProcessStartInfo(_filesService.ParseCommand(args.Filename),
                _filesService.ParseCommand(args.Args))
            {
                CreateNoWindow = true,
                RedirectStandardInput = args.Stdin != null,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = args.WorkingDirectory == null
                    ? null
                    : _filesService.ParseCommand(args.WorkingDirectory),
            });
        }
        catch (Exception e)
        {
            return new CompletedProcess { ExitCode = 401, Stderr = e.Message };
        }

        if (proc == null)
        {
            return new CompletedProcess { ExitCode = 402 };
        }

        if (args.Stdin != null)
        {
            await proc.StandardInput.WriteAsync(args.Stdin);
            await proc.StandardInput.FlushAsync();
        }

        await proc.WaitForExitAsync();
        return new CompletedProcess
        {
            ExitCode = proc.ExitCode,
            Stdout = await proc.StandardOutput.ReadToEndAsync(),
            Stderr = await proc.StandardError.ReadToEndAsync(),
            // Time = proc.TotalProcessorTime,
        };
    }

    [BridgePost("process")]
    public async Task<CompletedProcess[]> PostProcessHandler([FromBody] RunProcessArgs[] argsArray)
    {
        var res = new List<CompletedProcess>();
        foreach (var args in argsArray)
        {
            res.Add(await RunProcess(args));
        }

        return res.ToArray();
    }
}