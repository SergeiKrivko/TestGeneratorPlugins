using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using TestGenerator.Shared.Types;

namespace Tests.Core;

public class TestsGroup
{
    public required Guid Id { get; init; }

    private string _name = "";

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            Changed?.Invoke();
        }
    }

    public event Action? Changed;

    [JsonIgnore] public ObservableCollection<TestsGroup> Groups { get; init; } = [];
    [JsonIgnore] public ObservableCollection<Test> Tests { get; init; } = [];

    [JsonPropertyName("Groups")]
    public required TestsGroup[] JsonGroups
    {
        get => Groups.ToArray();
        init
        {
            Groups = new ObservableCollection<TestsGroup>(value);
            Groups.CollectionChanged += OnCollectionChanged;
            ReloadChildren();
        }
    }

    [JsonPropertyName("Tests")]
    public required Guid[] JsonTests
    {
        get => Tests.Select(t => t.Id).ToArray();
        init
        {
            Tests = new ObservableCollection<Test>(value.Select(Test.Load));
            Tests.CollectionChanged += OnCollectionChanged;
            ReloadChildren();
        }
    }

    [JsonIgnore] public ObservableCollection<object> Children { get; init; } = [];

    public static TestsGroup New() => new TestsGroup
        { Id = Guid.NewGuid(), Name = "Новая группа", JsonGroups = [], JsonTests = [] };

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var item in e.NewItems ?? new List<object>())
                {
                    Children.Add(item);
                }

                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (var item in e.OldItems ?? new List<object>())
                {
                    Children.Remove(item);
                }

                break;
            case NotifyCollectionChangedAction.Reset:

                break;
        }
        global::Tests.Tests.Service.Save();
    }

    private void ReloadChildren()
    {
        Children.Clear();
        foreach (var item in Groups)
        {
            Children.Add(item);
        }

        foreach (var item in Tests)
        {
            Children.Add(item);
        }
    }

    public int Count() => Groups.Sum(g => g.Count()) + Tests.Count;
    public int Count(Test.TestStatus status) => Groups.Sum(g => g.Count(status)) + Tests.Count(t => t.Status == status);

    public async IAsyncEnumerable<Test> Run(ABuild build, [EnumeratorCancellation] CancellationToken token)
    {
        foreach (var group in Groups)
        {
            await foreach (var i in group.Run(build, token))
                yield return i;
        }
        foreach (var test in Tests)
        {
            await test.Run(build, token);
            yield return test;
        }
    }

    [JsonIgnore]
    public Test.TestStatus Status
    {
        set
        {
            foreach (var group in Groups)
            {
                group.Status = value;
            }
            foreach (var test in Tests)
            {
                test.Status = value;
            }
            Changed?.Invoke();
        }
    }

    public bool Remove(Test test)
    {
        if (!Tests.Contains(test))
            return false;
        test.Delete();
        Tests.Remove(test);
        return true;
    }

    public bool Remove(TestsGroup group)
    {
        if (!Groups.Contains(group))
            return false;
        Groups.Remove(group);
        return true;
    }

    public bool InsertAfter(Test existing, ICollection<Test> newTests)
    {
        if (!Tests.Contains(existing))
            return false;
        var index = Tests.IndexOf(existing);
        foreach (var test in newTests)
        {
            Tests.Insert(index++, test);
        }
        return true;
    }

    public void UpdateTestsStatus()
    {
        Changed?.Invoke();
    }

    public PackedTestGroup Pack() => new PackedTestGroup
    {
        Name = Name, 
        Groups = Groups.Select(group => group.Pack()).ToArray(),
        Tests = Tests.Select(test => test.Pack()).ToArray()
    };

    public static TestsGroup Unpack(PackedTestGroup packedGroup)
    {
        var res = New();
        res.Name = packedGroup.Name;
        foreach (var group in packedGroup.Groups)
        {
            res.Groups.Add(Unpack(group));
        }
        foreach (var test in packedGroup.Tests)
        {
            res.Tests.Add(Test.Unpack(test));
        }

        return res;
    }
}