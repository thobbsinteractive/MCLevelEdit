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

    public Node(Bitmap icon, string name, string title)
    {
        Icon = icon;
        Name = name;
        Title = title;
    }

    public Node(Bitmap icon, string name, string title, ObservableCollection<Node> subNodes)
    {
        Icon = icon;
        Name = name;
        Title = title;
        SubNodes = subNodes;
    }
}

