using System.Collections.ObjectModel;

namespace MCLevelEdit.ViewModels;

public class CoordNode : Node
{
    public int X;
    public int Y;

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

