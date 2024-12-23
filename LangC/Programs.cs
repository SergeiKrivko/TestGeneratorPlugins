using TestGenerator.Shared.SidePrograms;

namespace LangC;

internal class Programs
{
    public static SideProgram Gcc { get; } = new SideProgram
    {
        Key = "Gcc", Validators = [new ProgramOutputValidator("--version", "gcc")],
        Locations = new Dictionary<string, ICollection<string>>
        {
            { "Default", ["gcc"] },
            {
                "Windows", [@"%SystemDrive%\MinGW\bin\gcc.exe", @"%HOMEDRIVE%\MinGW\bin\gcc.exe"]
            },
            {
                "Linux", ["/usr/bin/gcc"]
            },
            {
                "MacOS", ["/usr/bin/gcc"]
            }
        }
    };

    public static SideProgramFile? GetGcc() => Programs.Gcc.FromModel(
        LangC.ProjectSettings.Get<bool>("defaultPrograms", true)
            ? LangC.Settings.Get<ProgramFileModel>("gcc")
            : LangC.ProjectSettings.Get<ProgramFileModel>("gcc"));
}