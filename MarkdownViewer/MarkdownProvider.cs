﻿using System.Reflection;
using Avalonia;
using Markdown.Avalonia;
using TestGenerator.Shared.Types;

namespace WebViewProvider;

public class MarkdownProvider : IEditorProvider
{
    public string Key => "MarkdownViewer";
    public string Name => "Просмотр Markdown";
    public string[]? Extensions => [".md"];
    public int Priority => 6;

    public OpenedFile Open(string path)
    {
        var widget = new MarkdownScrollViewer();
        widget.Markdown = File.ReadAllText(path);
        widget.Margin = new Thickness(15);
        return new OpenedFile
        {
            Name = Path.GetFileName(path),
            Path = path,
            Widget = widget
        };
    }
}