using UpdateService.Models;

namespace UpdateService.Services;

public class ReleasesHttpService : BodyDetailHttpService
{
    public ReleasesHttpService()
    {
        Client.BaseAddress = new Uri("https://testgenerator-api.nachert.art");
    }

    public async Task<string> CreateReleaseZip(ICollection<AppFile> files)
    {
        return await Post<string>(
            $"/api/v1/releases/download?runtime={System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier}",
            files);
    }

    public async Task DownloadFile(string url, string path)
    {
        var stream = await Client.GetStreamAsync(url);
        var file = File.Create(path);
        await stream.CopyToAsync(file);
        file.Close();
    }
}