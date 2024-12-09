using System.Diagnostics;
using System.Text.Json;
using Renci.SshNet;
using SshPlugin.Models;
using StdioBridge.Client;
using TestGenerator.Shared.Types;

namespace SshPlugin.Services;

public class SshBridgeService
{
    private BridgeClient? _client;

    public async Task Connect(Guid connectionId, ShellStream stream)
    {
        _client = new BridgeClient(new StreamBrideStream(stream));
        _client.OnLog += Console.WriteLine;
        
        await PostConnectionId(connectionId);
    }

    private async Task PostConnectionId(Guid id)
    {
        if (_client == null)
            throw new Exception();
        await _client.PostAsync<Guid>("api/v1/system/connectionId", id);
    }

    public async Task<string> GetHostRuntime()
    {
        if (_client == null)
            throw new Exception();
        return await _client.GetAsync<string>("api/v1/system/runtime") ?? throw new Exception();
    }

    public async Task<FilesBucketModel> PostUnpackBucket(FilesBucketModel bucket)
    {
        if (_client == null)
            throw new Exception();
        return await _client.PostAsync<FilesBucketModel>("api/v1/files/unpack", bucket) ?? throw new Exception();
    }

    public async Task<FilesBucketModel> PostPackBucket(FileModel[] fileModels)
    {
        if (_client == null)
            throw new Exception();
        return await _client.PostAsync<FilesBucketModel>("api/v1/files/pack", fileModels) ?? throw new Exception();
    }

    public async Task<FileModel[]> GetFetch(FileModel[] files)
    {
        if (_client == null)
            throw new Exception();
        return await _client.GetAsync<FileModel[]>("api/v1/files/fetch", files) ?? throw new Exception();
    }

    public async Task<FileModel[]> GetList(string path, FileOrigin origin = FileOrigin.Host)
    {
        if (_client == null)
            throw new Exception();
        return await _client.GetAsync<FileModel[]>(
            $"api/v1/files/list?path={path}&hostPath={origin == FileOrigin.Host}") ?? throw new Exception();
    }

    public async Task<string> GetTemp()
    {
        if (_client == null)
            throw new Exception();
        return await _client.GetAsync<string>("api/v1/files/temp") ?? throw new Exception();
    }

    public async Task<string> GetTemp(string extension)
    {
        if (_client == null)
            throw new Exception();
        return await _client.GetAsync<string>($"api/v1/files/temp?extension={extension}") ?? throw new Exception();
    }

    public async Task<CompletedProcess[]> RunProcesses(RunProcessArgs[] argsArray)
    {
        if (_client == null)
            throw new Exception();
        return await _client.PostAsync<CompletedProcess[]>("api/v1/system/process", argsArray) ?? throw new Exception();
    }
}