namespace LangC;

public class HFileCreator : BaseFileCreator
{
    public override string Key => "CreateHeaderFile";
    public override string Name => "Header file";
    public override string? Icon => LangC.HIcon;

    protected override string Extension => ".h";
}