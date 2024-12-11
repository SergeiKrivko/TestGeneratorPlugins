namespace WebViewProvider;

public class MarkdownViewer : TestGenerator.Shared.Plugin
{
    public MarkdownViewer()
    {
        Name = "NewPlugin";

        MainTabs = [];
        SideTabs = [];

        BuildTypes = [];
        ProjectTypes = [];
        EditorProviders = [new MarkdownProvider()];
    }
}