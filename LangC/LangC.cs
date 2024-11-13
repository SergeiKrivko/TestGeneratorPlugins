using TestGenerator.Shared.Settings;
using TestGenerator.Shared.Types;

namespace LangC;

public class LangC : TestGenerator.Shared.Plugin
{
    public LangC()
    {
        Name = "NewPlugin";

        MainTabs = [];
        SideTabs = [];

        BuildTypes = [new BuildType
        {
            Key = "CExe",
            Name = "C Executable",
            Icon = "M1.5 6.99999C1.5 5.99999 2.5 5.49999 2.5 5.49999L11 0.499993C11 0.499993 12 1.49012e-08 13 " +
                   "0.499993C14 0.999986 21.5 5.49999 21.5 5.49999C21.5 5.49999 22.5 5.99999 22.5 6.99999V17C22.5 " +
                   "18 21.5 18.5 21.5 18.5L13 23.5C13 23.5 12 24 11 23.5C10 23 2.5 18.5 2.5 18.5C2.5 18.5 1.5 18 " +
                   "1.5 17V6.99999ZM20.5 6.99999L12 2L3.5 7V17L12 22L20.5 17V6.99999ZM18 14.5C18 14.5 18 19 12 " +
                   "19C6 19 6 12 6 12C6 12 6 5 12 5C18 5 18 10 18 10H15.5C15.5 10 15.5 7 12 7C8.5 7 8.5 12 8.5 " +
                   "12C8.5 12 8.5 17 12 17C15.5 17 15.5 14.5 15.5 14.5H18Z",
            Builder = (id, project, settings) => new CBuilder(id, project, settings),
            SettingsFields = () =>
            [
                new StringField { FieldName = "Ключи компилятора", Key = "compilerKeys", Value = "" },
                new FilesField(AAppService.Instance.CurrentProject.Path, [".c", ".h"]) { Key = "files" }
            ]
        }];
        ProjectTypes = [];

        FileCreators = [new CFileCreator(), new HFileCreator()];
    }
}