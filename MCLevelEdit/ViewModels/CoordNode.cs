using System.Collections.ObjectModel;

namespace MCLevelEdit.ViewModels;

public class CoordNode : Node
{
    public int X;
    public int Y;

    public CoordNode(int x, int y, string title) : base(title)
    {
        X = x;
        Y = y;
    }

    public CoordNode(int x, int y, string title, ObservableCollection<Node> subNodes) : base(title, subNodes)
    {
        X = x;
        Y = y;
    }
}

