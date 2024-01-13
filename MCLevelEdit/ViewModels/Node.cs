using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.ObjectModel;

namespace MCLevelEdit.ViewModels;

public class Node
{
    public ObservableCollection<Node>? SubNodes { get; }
    public Bitmap Icon { get; set; }
    public string Name { get; }
    public string Title { get; }

    public Node(string iconPath, string name, string title)
    {
        if (!string.IsNullOrWhiteSpace(iconPath))
            Icon = new Bitmap(AssetLoader.Open(new Uri(iconPath)));
        Name = name;
        Title = title;
    }

    public Node(string iconPath, string name, string title, ObservableCollection<Node> subNodes)
    {
        if (!string.IsNullOrWhiteSpace(iconPath))
            Icon = new Bitmap(AssetLoader.Open(new Uri(iconPath)));
        Name = name;
        Title = title;
        SubNodes = subNodes;
    }
}

