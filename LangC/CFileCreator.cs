namespace LangC;

public class CFileCreator : BaseFileCreator
{
    public override string Key => "CreateCFile";
    public override string Name => "C source file";

    protected override string Extension => ".c";
}