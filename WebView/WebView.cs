namespace WebView;

public class WebView : TestGenerator.Shared.Plugin
{
    public WebView()
    {
        EditorProviders = [new WebViewProvider()];
    }
}