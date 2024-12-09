using SshPlugin.Api.Services;
using StdioBridge.Api;

var app = new BridgeApp();

app.Services.Add<FilesService>()
    .AddControllers();

await app.RunAsync();