namespace LangC;

public class CFileCreator : BaseFileCreator
{
    public override string Key => "CreateCFile";
    public override string Name => "C source file";
    public override string? Icon => LangC.CIcon;

    protected override string Extension => ".c";
}