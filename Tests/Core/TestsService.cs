using System.Collections.ObjectModel;
using System.Text.Json;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace Tests.Core;

public class TestsService
{
    public ObservableCollection<TestsGroup> Groups { get; } = [];
    private SettingsSection _projectData;

    public TestsService()
    {
        _projectData = Tests.ProjectData;
        AAppService.Instance.Subscribe<string>("projectChanged", path => Load());
    }

    public void Save()
    {
        _projectData.Set("testGroups", Groups);
    }

    private void Load()
    {
        _projectData = Tests.ProjectData;
        Groups.Clear();
        foreach (var testsGroup in _projectData.Get<TestsGroup[]>("testGroups", []))
        {
            Groups.Add(testsGroup);
        }
    }

    public async Task Run()
    {
        var build = await AAppService.Instance.Request<ABuild?>("getBuild",
            Tests.ProjectSettings.Get<Guid>("selectedBuild"));
        if (build == null)
            return;
        
        foreach (var group in Groups)
        {
            group.Status = Test.TestStatus.InProgress;
        }
        
        async Task<int> TestingBackgroundFunc(IBackgroundTask task)
        {
            var totalCount = Groups.Sum(g => g.Count);
            var i = 0;
            foreach (var group in Groups)
            {
                await foreach (var test in group.Run(build))
                {
                    i++;
                    task.Status = test.Name;
                    task.Progress = i * 100.0 / totalCount;
                }
                    
            }
            return 0;
        }

        await build.RunPreProcConsole();

        await AAppService.Instance.RunBackgroundTask("Тестирование", TestingBackgroundFunc).Wait();

        await build.RunPostProcConsole();
    }

}