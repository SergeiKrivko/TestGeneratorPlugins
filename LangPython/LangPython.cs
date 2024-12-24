using LangPython.Builders;
using LangPython.Creators;
using TestGenerator.Shared.Settings;
using TestGenerator.Shared.SidePrograms;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace LangPython;

public class LangPython : TestGenerator.Shared.Plugin
{
    internal const string PythonIcon =
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

    internal const string FastApiIcon = "M56.813 127.586c-1.903-.227-3.899-.52-4.434-.652a48 48 0 0 0-2.375-.5a36 36 0 0 1-2.703-.633c-4.145-1.188-4.442-1.285-7.567-2.563c-2.875-1.172-8.172-3.91-9.984-5.156c-.496-.344-.96-.621-1.031-.621s-1.23-.816-2.578-1.813c-8.57-6.343-15.004-14.043-19.653-23.527c-.8-1.629-1.453-3.074-1.453-3.21c0-.134-.144-.505-.32-.817c-.363-.649-.88-2.047-1.297-3.492a20 20 0 0 0-.625-1.813c-.195-.46-.352-1.02-.352-1.246c0-.227-.195-.965-.433-1.645c-.238-.675-.43-1.472-.43-1.77c0-.296-.187-1.32-.418-2.276C.598 73.492 0 67.379 0 63.953c0-3.422.598-9.535 1.16-11.894c.23-.957.418-2 .418-2.32c0-.321.145-.95.32-1.4c.18-.448.41-1.253.516-1.788c.11-.535.36-1.457.563-2.055l.59-1.726c.433-1.293.835-2.387 1.027-2.813c.11-.238.539-1.21.957-2.16c.676-1.535 2.125-4.43 2.972-5.945c.309-.555.426-.739 2.098-3.352c2.649-4.148 7.176-9.309 11.39-12.988c1.485-1.297 6.446-5.063 6.669-5.063c.062 0 .53-.281 1.043-.625c1.347-.902 2.668-1.668 4.39-2.531a53 53 0 0 0 1.836-.953c.285-.164.82-.41 3.567-1.64c.605-.27 1.257-.516 3.136-1.173c.414-.144 1.246-.449 1.84-.672c.598-.222 1.301-.406 1.563-.406c.258 0 .937-.18 1.508-.402c.57-.223 1.605-.477 2.304-.563c.696-.082 1.621-.277 2.055-.43c.43-.148 1.61-.34 2.621-.425a73 73 0 0 0 3.941-.465c2.688-.394 8.532-.394 11.192 0a75 75 0 0 0 3.781.445c.953.079 2.168.278 2.703.442c.535.16 1.461.36 2.055.433c.594.079 1.594.325 2.222.551c.63.23 1.344.414 1.59.414s.754.137 1.125.309c.375.168 1.168.449 1.766.625c.594.18 1.613.535 2.27.797c.652.261 1.527.605 1.945.761c.77.29 6.46 3.137 7.234 3.622c6.281 3.917 9.512 6.476 13.856 10.964c5.238 5.414 8.715 10.57 12.254 18.16c.25.536.632 1.329.851 1.758c.215.434.395.942.395 1.13c0 .19.18.76.402 1.269c.602 1.383 1.117 2.957 1.36 4.16c.12.59.343 1.32.495 1.621c.153.3.332 1.063.403 1.688c.07.624.277 1.648.453 2.269c1.02 3.531 1.527 13.934.91 18.535c-.183 1.367-.39 3.02-.46 3.672c-.118 1.117-.708 4.004-1.212 5.945l-.52 2.055c-.98 3.957-3.402 9.594-6.359 14.809c-1.172 2.07-5.101 7.668-5.843 8.324c-.067.058-.399.45-.735.863c-.336.418-1.414 1.586-2.39 2.594c-4.301 4.441-7.77 7.187-13.86 10.969c-.722.449-6.847 3.441-7.992 3.906c-.594.238-1.586.64-2.203.89c-.613.247-1.297.454-1.512.458c-.215.003-.781.195-1.258.425c-.476.23-1.082.422-1.351.426c-.266.004-1.043.192-1.727.418c-.683.23-1.633.477-2.11.55c-.476.075-1.495.278-2.269.45c-.773.172-3.11.508-5.187.746a59 59 0 0 1-13.945-.031m4.703-12.5c.3-.234.609-.7.691-1.027c.18-.723 29.234-58.97 29.781-59.7c.461-.617.504-1.605.082-1.953c-.222-.187-3.004-.246-10.43-.234c-5.57.012-10.253.016-10.406.012c-.226-.008-.273-3.73-.25-19.672c.016-10.817-.035-19.766-.113-19.89c-.078-.126-.383-.227-.68-.227c-.418 0-.613.18-.87.808c-.485 1.168-1.825 3.82-8.348 16.485a3555 3555 0 0 0-4.055 7.89c-1.156 2.262-2.98 5.813-4.047 7.89a8751 8751 0 0 0-8.598 16.759c-4.933 9.636-5.53 10.785-5.742 11.039c-.41.496-.633 1.64-.402 2.07c.21.394.629.41 11.043.394c5.953-.007 10.863.024 10.914.07c.137.141.086 37.31-.055 38.196c-.093.582-.031.89.235 1.156c.46.461.586.457 1.25-.066m0 0";

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
            },
            new BuildType
            {
                Key = "FastApi",
                Name = "FastAPI",
                Icon = FastApiIcon,
                Builder = (id, project, settings) => new FastApiBuilder(id, project, settings),
                SettingsFields = () =>
                [
                    new StringField { Key = "entrypoint", FieldName = "Точка входа" },
                    new DefaultField([
                        new ProgramField { Program = Python, FieldName = "Интерпретатор Python", Key = "interpreter" }
                    ]) { Key = "defaultInterpreter", FieldName = "Интерпретатор по умолчанию", Inversion = true },
                ]
            }
        ];
        ProjectTypes = [new ProjectType("Python", "Python", PythonIcon, [
            new ProjectType.ProjectTypeDetector(7, path => File.Exists(Path.Join("main.py"))),
            new ProjectType.ProjectTypeDetector(7, path => File.Exists(Path.Join(path, "src/main.py"))),
            new ProjectType.ProjectTypeDetector(5, path => File.Exists(Path.Join(path, "requirements.txt"))),
            new ProjectType.ProjectTypeDetector(5, path => Directory.Exists(Path.Join(path, "venv"))),
            new ProjectType.ProjectTypeDetector(5, path => Directory.Exists(Path.Join(path, ".venv"))),
            new ProjectType.ProjectTypeDetector(5, path => FileWithExtensionExists(path, ".py", 3)),
        ], [new PythonProjectCreator(), new FastApiCreator()])];

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

        FileCreators = [new PythonFileCreator()];
    }

    private static bool FileWithExtensionExists(string path, string extension, int recurseLevel = 1)
    {
        if (Directory.EnumerateFiles(path).Any(f => f.EndsWith(extension)))
            return true;
        if (recurseLevel > 1)
            return Directory.EnumerateDirectories(path)
                .Any(d => FileWithExtensionExists(d, extension, recurseLevel - 1));
        return false;
    }
}