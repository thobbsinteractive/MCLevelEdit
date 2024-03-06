using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels;

public class Node : ObservableObject
{
    private Bitmap _icon;
    private string _name;
    private string _title;
    private string _subtitle;
    private string _tooltip;
    private bool _canDelete;

    public ICommand DeleteEntityCommand { get; protected set; }

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
    public string Subtitle
    {
        get => _subtitle;
        set => this.SetProperty(ref _subtitle, value);
    }
    public string ToolTip
    {
        get => _tooltip;
        set => this.SetProperty(ref _tooltip, value);
    }
    public bool CanDelete
    {
        get => _canDelete;
        set => this.SetProperty(ref _canDelete, value);
    }

    public bool IsSubtitleSet => _subtitle?.Length > 0;

    public Node(Bitmap icon, string name, string title, string toolTip)
    {
        Icon = icon;
        Name = name;
        Title = title;
        ToolTip = toolTip;
    }

    public Node(Bitmap icon, string name, string title, string toolTip, ObservableCollection<Node> subNodes)
    {
        Icon = icon;
        Name = name;
        Title = title;
        ToolTip = toolTip;
        SubNodes = subNodes;
    }
}

