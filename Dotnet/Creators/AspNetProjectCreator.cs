namespace Dotnet.Creators;

public class AspNetProjectCreator : BaseProjectCreator
{
    public override string Key => "AspNet";
    public override string Name => "Веб-API ASP.NET Core";
    protected override string TemplateName => "webapi";
}