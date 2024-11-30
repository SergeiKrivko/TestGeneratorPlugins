using Avalonia.Controls;
using TestGenerator.Shared.Settings;
using TestGenerator.Shared.SidePrograms;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace LangPython;

public class LangPython : TestGenerator.Shared.Plugin
{
    private const string PythonIcon =
        "M14.719 1.70599C13.847 1.56199 12.572 1.49599 11.706 1.49999C10.895 1.50099 10.085 1.56999 9.285 " +
        "1.70599C7.14 2.07999 6.75 2.86199 6.75 4.30899V5.99999H12V6.74999H5.045C3.097 6.74999 1.515 9.07799 1.5 " +
        "11.959V12C1.5 12.504 1.548 13.006 1.647 13.5C2.082 15.67 3.439 17.25 5.045 17.25H6V14.72C6 13.253 6.977 " +
        "11.91 8.578 11.622L9.041 11.564H14.719C14.957 11.564 15.195 11.534 15.425 11.47C15.9507 11.3295 16.415 " +
        "11.0188 16.7452 10.5863C17.0754 10.1538 17.253 9.6241 17.25 9.07999V4.30999C17.25 2.95399 16.093 1.93199 " +
        "14.719 1.70599ZM9.137 4.94599C9.01159 4.94638 8.88733 4.92207 8.77131 4.87444C8.65529 4.82681 8.54979 " +
        "4.7568 8.46083 4.6684C8.37188 4.58 8.3012 4.47494 8.25284 4.35922C8.20449 4.24351 8.17939 4.1194 8.179 " +
        "3.99399C8.17861 3.86858 8.20292 3.74431 8.25055 3.6283C8.29818 3.51228 8.36819 3.40678 8.45659 " +
        "3.31782C8.54499 3.22886 8.65005 3.15819 8.76577 3.10983C8.88148 3.06147 9.00559 3.03638 9.131 " +
        "3.03599C9.38428 3.03519 9.62751 3.13505 9.80717 3.31358C9.98683 3.49211 10.0882 3.73471 10.089 " +
        "3.98799C10.0898 4.24127 9.98994 4.48449 9.81141 4.66415C9.63287 4.84381 9.39028 4.94519 9.137 4.94599Z " +
        "M22.279 10.172C21.777 8.175 20.479 6.75 18.955 6.75H18.125V8.974C18.125 10.829 16.906 12.172 15.419 " +
        "12.402C15.269 12.426 15.116 12.437 14.964 12.437H9.285C9.045 12.437 8.805 12.467 8.571 12.529C7.525 " +
        "12.803 6.75 13.673 6.75 14.802V19.572C6.75 20.932 8.116 21.728 9.456 22.118C11.06 22.584 12.806 22.668 " +
        "14.726 22.118C16 21.754 17.25 21.018 17.25 19.572V18H12V17.25H18.955C20.335 17.25 21.531 16.081 22.117 " +
        "14.376C22.3743 13.6104 22.5037 12.8077 22.5 12C22.5014 11.3839 22.4272 10.77 22.279 10.172ZM14.836 " +
        "18.937C15.0893 18.9361 15.3326 19.0358 15.5123 19.2142C15.6921 19.3927 15.7936 19.6352 15.7945 " +
        "19.8885C15.7954 20.1418 15.6957 20.3851 15.5173 20.5648C15.3388 20.7446 15.0963 20.8461 14.843 " +
        "20.847C14.7176 20.8475 14.5933 20.8232 14.4773 20.7756C14.3612 20.7281 14.2557 20.6581 14.1667 " +
        "20.5698C13.9869 20.3913 13.8854 20.1488 13.8845 19.8955C13.8836 19.6422 13.9833 19.3989 14.1617 " +
        "19.2192C14.3402 19.0394 14.5827 18.9379 14.836 18.937Z";

    public static SettingsSection Settings { get; } = AAppService.Instance.GetSettings();
    public static SettingsSection ProjectSettings => AAppService.Instance.CurrentProject.GetSettings();
    public static SettingsSection ProjectData => AAppService.Instance.CurrentProject.GetData();

    private static ICollection<string> GetLocations(ICollection<string> locations, Range versions, string os)
    {
        var res = new List<string>();

        foreach (var location in locations)
        {
            for (int i = versions.Start.Value; i < versions.End.Value; i++)
            {
                if (os == "Windows")
                    res.Add($@"{location}\Python\Python3{i}\python.exe");
                else if (os == "Linux")
                    res.Add($"{location}/python3.{i}");
                else if (os == "MacOS")
                    res.Add($"{location}/3.{i}/bin/python3");
            }
        }

        return res;
    }

    public static SideProgram Python { get; } = new SideProgram
    {
        Key = "Python", Validators = [new ProgramOutputValidator("--version", "Python 3.")],
        Locations = new Dictionary<string, ICollection<string>>
        {
            { "Default", ["python", "python3"] },
            {
                "Windows",
                GetLocations(
                    [@"%LOCALAPPDATA%\Programs", @"%APPDATA%\Programs", "%ProgramFiles%", "%ProgramFiles(x86)%"],
                    new Range(0, 13), "Windows")
            },
            {
                "Linux",
                GetLocations(["/usr/bin"], new Range(0, 13), "Linux")
            },
            {
                "MacOS",
                GetLocations(["/Library/Frameworks/Python.framework/Versions"], new Range(0, 13), "MacOS")
            }
        }
    };

    public LangPython()
    {
        MainTabs = [];
        SideTabs = [];

        BuildTypes =
        [
            new BuildType
            {
                Key = "Python",
                Name = "Python",
                Icon = PythonIcon,
                Builder = (id, project, settings) => new PythonBuilder(id, project, settings),
                SettingsFields = () =>
                [
                    new PathField { Key = "mainFile", FieldName = "Файл", Extension = ".py" },
                    new DefaultField([
                        new ProgramField { Program = Python, FieldName = "Интерпретатор Python", Key = "interpreter" }
                    ]) { Key = "defaultInterpreter", FieldName = "Интерпретатор по умолчанию", Inversion = true },
                ]
            }
        ];
        ProjectTypes = [new ProjectType("Python", "Python", PythonIcon)];

        SettingsControls =
        [
            new SettingsPage("Языки/Python", Settings.Name ?? "", [
                new ProgramField { Program = Python, FieldName = "Интерпретатор Python", Key = "interpreter" }
            ]),
            new SettingsPage("Проект/Python", ProjectSettings.Name ?? "", [
                new DefaultField([
                    new ProgramField { Program = Python, FieldName = "Интерпретатор Python", Key = "interpreter" }
                ]) { Key = "defaultInterpreter", FieldName = "Интерпретатор по умолчанию", Inversion = true }
            ], SettingsPageType.ProjectSettings, () => AAppService.Instance.CurrentProject.Type == ProjectTypes[0])
        ];

        FileActions = [new FastRunAction()];

        FileIcons[".py"] = PythonIcon;
    }
}