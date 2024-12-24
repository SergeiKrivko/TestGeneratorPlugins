namespace Dotnet.Creators;

public class ConsoleProjectCreator : BaseProjectCreator
{
    public override string Key => "Console";
    public override string Name => "Консольное приложение";
    protected override string TemplateName => "console";
}