﻿using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MCLevelEdit.ViewModels;

public class Node : ObservableObject
{
    private Bitmap _icon;
    private string _name;
    private string _title;

    public ObservableCollection<Node>? SubNodes { get; }

    public Bitmap Icon 
    {
        get => _icon;
        set => this.SetProperty(ref _icon, value);
    }

    public string Name
    {
        get => _name;
        set => this.SetProperty(ref _name, value);
    }
    public string Title
    {
        get => _title;
        set => this.SetProperty(ref _title, value);
    }

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

