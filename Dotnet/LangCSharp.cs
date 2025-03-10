﻿using AvaluxUI.Utils;
using Dotnet.Creators;
using TestGenerator.Shared.Settings;
using TestGenerator.Shared.SidePrograms;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace Dotnet;

public class LangCSharp : TestGenerator.Shared.Plugin
{
    public static Logger Logger { get; } = Injector.Inject<IAppService>().GetLogger();
    public static ISettingsSection Settings { get; } = Injector.Inject<IAppService>().GetSettings();
    public static ISettingsSection ProjectSettings => Injector.Inject<IProjectsService>().Current.GetSettings();
    public static ISettingsSection ProjectData => Injector.Inject<IProjectsService>().Current.GetData();

    public const string CSharpIcon =
        "M12.0133 9.77166H10.0211C9.96431 9.44496 9.85955 9.15554 9.70685 8.90341C9.55415 8.64773 9.36417 8.43111 " +
        "9.1369 8.25355C8.90962 8.07599 8.65039 7.94283 8.3592 7.85405C8.07156 7.76172 7.76083 7.71555 7.42702 " +
        "7.71555C6.83398 7.71555 6.30842 7.8647 5.85032 8.163C5.39222 8.45774 5.03356 8.89098 4.77433 " +
        "9.46271C4.51509 10.0309 4.38548 10.7251 4.38548 11.5455C4.38548 12.38 4.51509 13.0831 4.77433 " +
        "13.6548C5.03711 14.223 5.39577 14.6527 5.85032 14.9439C6.30842 15.2315 6.83221 15.3754 7.4217 15.3754C7.7484" +
        " 15.3754 8.0538 15.3327 8.33789 15.2475C8.62553 15.1587 8.88299 15.0291 9.11026 14.8587C9.34109 14.6882" +
        " 9.53462 14.4787 9.69087 14.2301C9.85067 13.9815 9.96076 13.6974 10.0211 13.3778L12.0133 13.3885C11.9387 " +
        "13.907 11.7772 14.3935 11.5286 14.848C11.2836 15.3026 10.9622 15.7038 10.5645 16.0518C10.1667 16.3963 " +
        "9.70153 16.6662 9.16886 16.8615C8.63619 17.0533 8.04492 17.1491 7.39506 17.1491C6.43626 17.1491 5.58043 " +
        "16.9272 4.82759 16.4833C4.07475 16.0394 3.48171 15.3984 3.04847 14.5604C2.61523 13.7223 2.39862 12.7173 " +
        "2.39862 11.5455C2.39862 10.37 2.61701 9.36506 3.0538 8.53054C3.49059 7.69247 4.0854 7.05149 4.83825 " +
        "6.6076C5.59109 6.16371 6.44336 5.94176 7.39506 5.94176C8.00231 5.94176 8.56694 6.02699 9.08896 " +
        "6.19744C9.61097 6.3679 10.0762 6.61825 10.4846 6.94851C10.8929 7.27521 11.2285 7.67649 11.4913 " +
        "8.15234C11.7576 8.62464 11.9316 9.16442 12.0133 9.77166ZM17.8501 17L19.6398 6.09091H21.1313L19.3415 " +
        "17H17.8501ZM12.9335 14.1875L13.1839 12.696H21.5787L21.3284 14.1875H12.9335ZM14.0148 17L15.8046 " +
        "6.09091H17.2961L15.5063 17H14.0148ZM13.5674 10.3949L13.8124 8.90341H22.2073L21.9623 10.3949H13.5674Z";

    public static SideProgram Dotnet { get; } = new SideProgram
    {
        Key = "Dotnet", Validators = [new ProgramOutputValidator("--info", ".NET")],
        Locations = new Dictionary<string, ICollection<string>>
        {
            { "Default", ["dotnet"] },
            {
                "Windows",
                [@"%ProgramFiles%\dotnet.exe", @"%ProgramFiles(x86)%\dotnet.exe"]
            },
            {
                "Linux",
                ["/usr/bin"]
            },
            {
                "MacOS",
                ["/usr/bin"]
            }
        }
    };

    public static SideProgramFile? GetDotnet() => Dotnet.FromModel(ProjectSettings.Get("defaultPrograms", true)
        ? Settings.Get<ProgramFileModel>("dotnet")
        : ProjectSettings.Get<ProgramFileModel>("dotnet"));

    public LangCSharp()
    {
        BuildTypes =
        [
            new BuildType
            {
                Key = "Dotnet",
                Name = ".Net",
                Icon = CSharpIcon,
                Builder = (id, project, settings) => new DotnetBuilder(id, project, settings),
                SettingsFields = () =>
                [
                    new StringField { FieldName = "Проект", Key = "project", Value = "" },
                    new StringField { FieldName = "Конфигурация", Key = "configuration" }
                ]
            }
        ];
        ProjectTypes =
        [
            new ProjectType("CSharp", "C#", CSharpIcon, [
                new ProjectType.ProjectTypeDetector(8, p => FileWithExtensionExists(p, ".sln")),
                new ProjectType.ProjectTypeDetector(4, p => FileWithExtensionExists(p, ".csproj", 4)),
            ], [new ConsoleProjectCreator(), new ClassLibProjectCreator(), new AspNetProjectCreator()])
        ];
        SettingsControls =
        [
            new SettingsPage("Языки/.Net", Settings.Name ?? "", [
                new ProgramField { Program = Dotnet, FieldName = ".Net", Key = "dotnet" }
            ]),
            new SettingsPage("Проект/.Net", ProjectSettings.Name ?? "", [
                new DefaultField([
                    new ProgramField { Program = Dotnet, FieldName = ".Net", Key = "dotnet" }
                ]) { Key = "defaultPrograms", FieldName = "Программы по умолчанию", Inversion = true }
            ], SettingsPageType.ProjectSettings, () => Injector.Inject<IProjectsService>().Current.Type == ProjectTypes[0])
        ];

        FileCreators = [new CSharpFileCreator()];
        FileIcons[".cs"] = CSharpIcon;
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