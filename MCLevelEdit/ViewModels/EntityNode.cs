using Avalonia.Media.Imaging;
using System.Collections.ObjectModel;

namespace MCLevelEdit.ViewModels;

public class EntityNode : Node
{
    private int _x;
    private int _y;

    public int X
    {
        get => _x;
        set => this.SetProperty(ref _x, value);
    }
    public int Y
    {
        get => _y;
        set => this.SetProperty(ref _y, value);
    }

    public EntityNode(string name, int x, int y, Bitmap icon, string title, string subTitle) : base(icon, name, title)
    {
        X = x;
        Y = y;
        Subtitle = subTitle;
    }

    public EntityNode(string name, int x, int y, Bitmap icon, string title, string subTitle, ObservableCollection<Node> subNodes) : base(null, name, title, subNodes)
    {
        X = x;
        Y = y;
        Subtitle = subTitle;
    }
}

