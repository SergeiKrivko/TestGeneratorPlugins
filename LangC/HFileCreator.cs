namespace LangC;

public class HFileCreator : BaseFileCreator
{
    public override string Key => "CreateHeaderFile";
    public override string Name => "Header file";

    protected override string Extension => ".h";
}