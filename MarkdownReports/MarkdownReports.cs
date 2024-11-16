namespace MarkdownReports;

public class MarkdownReports : TestGenerator.Shared.Plugin
{
    public MarkdownReports()
    {
        MainTabs = [];
        SideTabs = [];

        BuildTypes = [];
        ProjectTypes = [];

        FileActions = [new ToDocxAction()];
    }
}