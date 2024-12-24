namespace Dotnet.Creators;

public class ClassLibProjectCreator : BaseProjectCreator
{
    public override string Key => "ClassLib";
    public override string Name => "Библиотека классов";
    protected override string TemplateName => "classlib";
    protected override bool CreateBuild => false;
}