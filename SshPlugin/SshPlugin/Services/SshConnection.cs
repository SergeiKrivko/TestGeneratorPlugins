﻿using Renci.SshNet;
using TestGenerator.Shared.SidePrograms;
using TestGenerator.Shared.Utils;
using AuthenticationMethod = SshPlugin.Models.AuthenticationMethod;

namespace SshPlugin.Services;

public class SshConnection
{
    public Guid Id { get; }

    public SettingsFile Settings { get; }

    private readonly Repository _repository;
    private readonly SshBridgeService _bridgeService = new();
    private FilesService _filesService;
    private InstallationService _installationService;
    private SshClient _sshClient = new("localhost", 22, "root", "password");
    private SftpClient _sftpClient = new("localhost", 22, "root", "password");
    private ShellStream? _stream;
    private SshVirtualSystem? _virtualSystem;

    public event Action? Changed;

    public string? Name
    {
        get => Settings.Get<string>("name");
        set
        {
            Settings.Set("name", value);
            Changed?.Invoke();
        }
    }

    public string Host
    {
        get => Settings.Get<string>("host", "localhost");
        set
        {
            Settings.Set("host", value);
            Changed?.Invoke();
        }
    }

    public int Port
    {
        get => Settings.Get("port", 22);
        set
        {
            Settings.Set("port", value);
            Changed?.Invoke();
        }
    }

    public string Username
    {
        get => Settings.Get<string>("username", "root");
        set
        {
            Settings.Set("username", value);
            Changed?.Invoke();
        }
    }

    public AuthenticationMethod AuthenticationMethod
    {
        get => Settings.Get("authenticationMethod", AuthenticationMethod.Password);
        set => Settings.Set("authenticationMethod", value);
    }

    public string Password
    {
        get => Settings.Get<string>("password", "");
        set => Settings.Set("password", value);
    }

    public string PrivateKeyFile
    {
        get => Settings.Get<string>("privateKeyFile", "");
        set => Settings.Set("privateKeyFile", value);
    }

    public string HostProgramPath
    {
        get => Settings.Get<string>("hostProgramPath", "");
        set => Settings.Set("hostProgramPath", value);
    }

    public string OperatingSystem
    {
        get => Settings.Get<string>("operatingSystem", "");
        set => Settings.Set("operatingSystem", value);
    }

    public bool AutoConnect
    {
        get => Settings.Get("autoConnect", true);
        set => Settings.Set("autoConnect", value);
    }

    public bool Autoupdate
    {
        get => Settings.Get("autoupdate", true);
        set => Settings.Set("autoupdate", value);
    }

    public SshConnection(Guid id)
    {
        Id = id;
        Settings = SettingsFile.Open(Path.Join(DataPath, $"{Id}.xml"));
        _repository = new Repository(DataPath, Id);
        var sftp = new SftpService(_sftpClient);
        _filesService = new FilesService(Id, _repository, _bridgeService, sftp);
        _installationService = new InstallationService(this, _sshClient, sftp);
    }

    public async Task<bool> Reconnect()
    {
        try
        {
            Disconnect();
            switch (AuthenticationMethod)
            {
                case AuthenticationMethod.Password:
                    _sshClient = new SshClient(Host, Port, Username, Password);
                    _sftpClient = new SftpClient(Host, Port, Username, Password);
                    break;
                case AuthenticationMethod.PrivateKey:
                    var pk = new PrivateKeyFile(PrivateKeyFile);
                    _sshClient = new SshClient(Host, Port, Username, pk);
                    _sftpClient = new SftpClient(Host, Port, Username, pk);
                    break;
                default:
                    throw new Exception();
            }

            var sftp = new SftpService(_sftpClient);
            _filesService = new FilesService(Id, _repository, _bridgeService, sftp);
            _installationService = new InstallationService(this, _sshClient, sftp);

            await Connect();
        }
        catch (Exception)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(HostProgramPath))
        {
            OperatingSystem = await _installationService.DetectOperatingSystem();
            HostProgramPath = await _installationService.DetectProgramPath();
        }

        return IsConnected;
    }

    public async Task Connect()
    {
        if (!_sshClient.IsConnected)
            await _sshClient.ConnectAsync(new CancellationToken());
        if (!_sftpClient.IsConnected)
            await _sftpClient.ConnectAsync(new CancellationToken());

        Changed?.Invoke();
        SshPlugin.Logger.Info($"Connected to {Host}:{Port}");
    }

    public bool IsConnected => _sshClient.IsConnected && _sftpClient.IsConnected;

    public void Disconnect()
    {
        if (_stream != null)
        {
            _stream.Close();
            _stream = null;
        }

        if (_sshClient.IsConnected)
            _sshClient.Disconnect();
        if (_sftpClient.IsConnected)
            _sftpClient.Disconnect();

        SideProgram.VirtualSystems.RemoveAll(s => s.Key == _virtualSystem?.Key);

        Changed?.Invoke();
        SshPlugin.Logger.Info($"Disconnected from {_sshClient.ConnectionInfo.Host}:{_sshClient.ConnectionInfo.Port}");
    }

    public async Task Init()
    {
        if (!await Reconnect())
            return;

        _stream = await _installationService.StartProgram();
        if (_stream == null)
        {
            Disconnect();
            return;
        }

        await _bridgeService.Connect(Id, _stream);

        _virtualSystem = new SshVirtualSystem
        {
            Key = $"SSH-{Id}", Name = $"SSH - {Name}", Tags = ["Default"],
            FilesService = _filesService, BridgeService = _bridgeService,
        };
        SideProgram.VirtualSystems.Add(_virtualSystem);
    }

    private string DataPath { get; } = SshPlugin.DataPath;

    public Task PushFiles(string[] files) => _filesService.PushFiles(files);
    public Task PullFiles(string[] files) => _filesService.PullFiles(files);
}