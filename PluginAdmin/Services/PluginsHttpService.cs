﻿using System.Text.Json;
using PluginAdmin.Models;

namespace PluginAdmin.Services;

public class PluginsHttpService : BodyDetailHttpService
{
    public PluginsHttpService()
    {
        BaseUrl = "https://testgenerator-api.nachert.art/";
    }

    public async Task<TokenRead[]> GetAllTokens()
    {
        return await Get<TokenRead[]>("api/v1/tokens");
    }

    public async Task<PluginRead[]> GetAllPlugins()
    {
        return await Get<PluginRead[]>("api/v1/plugins/my");
    }

    private TokenPermission[]? _tokenPermissions;

    public async Task<TokenPermission[]> GetTokenPermissions()
    {
        return _tokenPermissions ??= await Get<TokenPermission[]>("api/v1/tokens/permissions");
    }

    public async Task DeleteToken(Guid tokenId)
    {
        await Delete<string>($"api/v1/tokens/{tokenId}");
    }

    public async Task<string> CreateToken(TokenCreate token)
    {
        return await Post<string>("api/v1/tokens", token);
    }

    public async Task<Guid> CreatePlugin(PluginCreate plugin)
    {
        return await Post<Guid>("api/v1/plugins", plugin);
    }
}