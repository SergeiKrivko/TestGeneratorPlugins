﻿using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using PluginAdmin.Exceptions;
using PluginAdmin.Models;

namespace PluginAdmin.Services;

public class HttpService
{
    protected HttpClient Client = new HttpClient();
    private AuthModel? _auth;

    public AuthModel? Auth
    {
        get => _auth;
        set
        {
            _auth = value;
            if (value == null)
                Client.DefaultRequestHeaders.Authorization = null;
            else
            {
                var encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                    .GetBytes(value.Login + ":" + value.Password));
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encoded);
            }
        }
    }

    protected string? BaseUrl
    {
        get => Client.BaseAddress?.ToString();
        set => Client.BaseAddress = value == null ? null : new Uri(value);
    }

    public delegate void UnauthorizedRequestHandler();
    public static event UnauthorizedRequestHandler? Unauthorized;

    private void ProcessStatusCode(HttpResponseMessage resp)
    {
        if (resp.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException();
        if (resp.StatusCode == HttpStatusCode.Unauthorized)
        {
            Unauthorized?.Invoke();
            throw new UnauthorizedException();
        }
        if (!resp.IsSuccessStatusCode)
            throw new BadResponseCodeException($"Code {resp.StatusCode}");
    }

    protected virtual async Task<T> ProcessResponseBody<T>(HttpResponseMessage response)
    {
        var jsonString = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<T>(jsonString);
        // var body = await response.Content.ReadFromJsonAsync<T>();
        if (body == null)
            throw new UnprocessableResponseException();
        return body;
    }

    protected virtual async Task<T> ProcessResponse<T>(HttpResponseMessage response)
    {
        ProcessStatusCode(response);
        return await ProcessResponseBody<T>(response);
    }

    protected virtual async Task ProcessResponse(HttpResponseMessage response)
    {
        ProcessStatusCode(response);
        await ProcessResponseBody<int?>(response);
    }

    protected async Task<T> Post<T>(string url, object? body)
    {
        try
        {
            var response = await Client.PostAsync(url, body == null ? null : JsonContent.Create(body));
            return await ProcessResponse<T>(response);
        }
        catch (HttpRequestException e)
        {
            throw new ConnectionException(e.Message);
        }
    }

    protected async Task Post(string url, object? body)
    {
        try
        {
            var response = await Client.PostAsync(url, body == null ? null : JsonContent.Create(body));
            await ProcessResponse(response);
        }
        catch (HttpRequestException e)
        {
            throw new ConnectionException(e.Message);
        }
    }

    protected async Task<T> Get<T>(string url)
    {
        try
        {
            var response = await Client.GetAsync(url);
            return await ProcessResponse<T>(response);
        }
        catch (HttpRequestException e)
        {
            throw new ConnectionException(e.Message);
        }
    }

    protected async Task<T> Put<T>(string url, object? body)
    {
        try
        {
            var response = await Client.PutAsync(url, body == null ? null : JsonContent.Create(body));
            return await ProcessResponse<T>(response);
        }
        catch (HttpRequestException e)
        {
            throw new ConnectionException(e.Message);
        }
    }

    protected async Task Put(string url, object? body)
    {
        try
        {
            var response = await Client.PutAsync(url, body == null ? null : JsonContent.Create(body));
            await ProcessResponse(response);
        }
        catch (HttpRequestException e)
        {
            throw new ConnectionException(e.Message);
        }
    }

    protected async Task<T> Delete<T>(string url)
    {
        try
        {
            var response = await Client.DeleteAsync(url);
            return await ProcessResponse<T>(response);
        }
        catch (HttpRequestException e)
        {
            throw new ConnectionException(e.Message);
        }
    }

    protected async Task Delete(string url)
    {
        try
        {
            var response = await Client.DeleteAsync(url);
            await ProcessResponse(response);
        }
        catch (HttpRequestException e)
        {
            throw new ConnectionException(e.Message);
        }
    }
}