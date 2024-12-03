namespace Tests.Core;

public interface ITextComparator
{
    public string Key { get; }
    public string Name { get; }

    public bool Compare(string s1, string s2);
}

public class TextComparator : ITextComparator
{
    public string Key => "Text";
    public string Name => "Текст";

    public bool Compare(string s1, string s2)
    {
        return s1 == s2;
    }
}

public class WordsComparator : ITextComparator
{
    public string Key => "Words";
    public string Name => "Слова";

    public bool Compare(string s1, string s2)
    {
        var lst1 = s1.Trim().Split();
        var lst2 = s2.Trim().Split();
        // Console.WriteLine($"{s1.Trim()} {s2.Trim()} {lst1.Length} {lst2.Length} {lst1.Zip(lst2).All(t => t.First == t.Second)}");
        if (lst1.Length != lst2.Length)
            return false;
        return lst1.Zip(lst2).All(t => t.First == t.Second);
    }
}