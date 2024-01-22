using System.Collections.ObjectModel;

namespace MCLevelEdit.ViewModels;

public class CoordNode : Node
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

    public CoordNode(int x, int y, string name, string title) : base(null, name, title)
    {
        X = x;
        Y = y;
    }

    public CoordNode(int x, int y, string name, string title, ObservableCollection<Node> subNodes) : base(null, name, title, subNodes)
    {
        X = x;
        Y = y;
    }
}

