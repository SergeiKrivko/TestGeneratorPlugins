using System.Text;
using Renci.SshNet;
using StdioBridge.Client.Core;

namespace SshPlugin.Services;

internal class StreamBrideStream : IBrideClientStream
{
    private readonly ShellStream _stream;

    public event Action<string>? OnNewLine;
    public async Task WriteLineAsync(string line)
    {
        // await _stream.WriteAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(line)));
        await Task.Run(() => _stream.WriteLine(line));
        await _stream.FlushAsync();
    }

    public StreamBrideStream(ShellStream stream)
    {
        _stream = stream;
        ReadOutputLoop();
    }
    
    private async void ReadOutputLoop()
    {
        while (_stream.CanRead)
        {
            var line = await Task.Run(() => _stream.ReadLine());
            if (line != null)
            {
                OnNewLine?.Invoke(line);
            }
        }
    }

}