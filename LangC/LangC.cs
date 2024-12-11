using System.Text.RegularExpressions;
using Avalonia.Controls;
using TestGenerator.Shared.Settings;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace LangC;

public class LangC : TestGenerator.Shared.Plugin
{
    public const string CIcon =
        "M1.5 6.99999C1.5 5.99999 2.5 5.49999 2.5 5.49999L11 0.499993C11 0.499993 12 1.49012e-08 13 " +
        "0.499993C14 0.999986 21.5 5.49999 21.5 5.49999C21.5 5.49999 22.5 5.99999 22.5 6.99999V17C22.5 " +
        "18 21.5 18.5 21.5 18.5L13 23.5C13 23.5 12 24 11 23.5C10 23 2.5 18.5 2.5 18.5C2.5 18.5 1.5 18 " +
        "1.5 17V6.99999ZM20.5 6.99999L12 2L3.5 7V17L12 22L20.5 17V6.99999ZM18 14.5C18 14.5 18 19 12 " +
        "19C6 19 6 12 6 12C6 12 6 5 12 5C18 5 18 10 18 10H15.5C15.5 10 15.5 7 12 7C8.5 7 8.5 12 8.5 " +
        "12C8.5 12 8.5 17 12 17C15.5 17 15.5 14.5 15.5 14.5H18Z";

    public const string HIcon =
        "M5.18359 20V4H8.56641V10.6016H15.4336V4H18.8086V20H15.4336V13.3906H8.56641V20H5.18359Z M0 " +
        "2C-2.87427e-06 0 2 0 2 0H22C22 0 24 0 24 2V22C24 24 22 24 22 24H2C0 24 0 22 0 22V2ZM2 " +
        "1.5C2 1.5 1.5 1.5 1.5 2V22C1.5 22.5 2 22.5 2 22.5H22C22 22.5 22.5 22.5 22.5 22V2C22.5 2 " +
        "22.5 1.5 22 1.5H2Z";

    public static SettingsSection Settings { get; } = AAppService.Instance.GetSettings();
    public static SettingsSection ProjectSettings => AAppService.Instance.CurrentProject.GetSettings();
    public static SettingsSection ProjectData => AAppService.Instance.CurrentProject.GetData();

    public static Logger Logger { get; } = AAppService.Instance.GetLogger("C / C++");

    public LangC()
    {
        BuildTypes =
        [
            new BuildType
            {
                Key = "CExe",
                Name = "C Executable",
                Icon = CIcon,
                Builder = (id, project, settings) => new CBuilder(id, project, settings),
                SettingsFields = () =>
                [
                    new StringField { FieldName = "Ключи компилятора", Key = "compilerKeys", Value = "" },
                    new StringField { FieldName = "Исполняемый файл", Key = "exePath" },
                    new FilesField(AAppService.Instance.CurrentProject.Path, [".c", ".h"]) { Key = "files" }
                ]
            }
        ];
        ProjectTypes = [new ProjectType("C", "C", CIcon)];

        FileCreators = [new CFileCreator(), new HFileCreator()];

        SettingsControls =
        [
            new SettingsPage("Языки/C, C++", Settings.Name ?? "", [
                new ProgramField { Program = Programs.Gcc, FieldName = "Компилятор gcc", Key = "gcc" }
            ]),
            new SettingsPage("Проект/C, C++", ProjectSettings.Name ?? "", [
                new DefaultField([
                    new ProgramField { Program = Programs.Gcc, FieldName = "Компилятор gcc", Key = "gcc" }
                ]) { Key = "defaultPrograms", FieldName = "Программы по умолчанию", Inversion = true }
            ], SettingsPageType.ProjectSettings, () => AAppService.Instance.CurrentProject.Type == ProjectTypes[0])
        ];

        FileIcons[".c"] = CIcon;
        FileIcons[".h"] = HIcon;
        RegexFileIcons[new Regex(@"[/\\]CMakeLists\.txt$")] = "M21.2 20.4H2.8L8.7 15.4L21.2 20.4Z M21.75 20.15L13.9 17L12.5 1.89999L21.75 20.15Z M13 11.2L2.89999 19.7L12.1 1.5L13 11.2Z";
        RegexFileIcons[new Regex(@"[/\\]cmake-build-\w+$")] = "M1 6C1 3 4 3 4 3H7.5C8 3 8.5 3.375 9 3.75C9.5 4.125 10 4.5 10.5 4.5H20.5C23.5 4.5 23.5 7.5 23.5 7.5V18C23.5 21.1354 20.5 21 20.5 21H4C4 21 1 21 1 18V6ZM4 4.5C4 4.5 2.5 4.5 2.5 6V18C2.5 18 2.5 19.5 4 19.5H20.5C20.5 19.5 22 19.5 22 18V7.5C22 7.5 22 6 20.5 6H10.5C9.75 6 9.25 5.625 8.75 5.25C8.25 4.875 7.75 4.5 7 4.5H4Z M18.1517 18.5H6.5L10.2361 15.3254L18.1517 18.5Z M18.5 18.3413L13.529 16.3413L12.6425 6.75395L18.5 18.3413Z M12.9591 12.6587L6.56332 18.0556L12.3892 6.5L12.9591 12.6587Z";
    }
}