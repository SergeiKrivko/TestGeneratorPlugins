using UpdateService.Models;

namespace UpdateService.Services;

public class ReleasesHttpService : BodyDetailHttpService
{
    public ReleasesHttpService()
    {
        Client.BaseAddress = new Uri("https://testgenerator-api.nachert.art");
    }

    public async Task<ZipResponseModel> CreateReleaseZip(ICollection<AppFile> files)
    {
        return await Post<ZipResponseModel>(
            $"/api/v1/releases/download?runtime={System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier}",
            files);
    }

    public async Task<Version> GetLatestVersion()
    {
        return await Get<Version>(
            $"/api/v1/releases/latest?runtime={System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier}");
    }

    public async Task DownloadFile(string url, string path)
    {
        var stream = await Client.GetStreamAsync(url);
        var file = File.Create(path);
        await stream.CopyToAsync(file);
        file.Close();
    }
}