using System.Reflection;
using Avalonia;
using TestGenerator.Shared.Types;

namespace WebView;

public class WebViewProvider : IEditorProvider
{
    public string Key => "WebView";
    public string Name => "Web View";
    public string[] Extensions => [".pdf", ".html", ".svg"];
    public int Priority => 6;
    private readonly string[] _prefixes = ["http:/", "https:/"];

    public bool CanOpen(string path)
    {
        return Extensions.Any(path.EndsWith) || _prefixes.Any(path.StartsWith);
    }

    public OpenedFile Open(string path)
    {
        var widget = new WebViewControl.WebView();
        widget.Address = File.Exists(path) ? "file:///" + path : path;
        return new OpenedFile
        {
            Name = Path.GetFileName(path),
            Path = path,
            Widget = widget
        };
    }
}