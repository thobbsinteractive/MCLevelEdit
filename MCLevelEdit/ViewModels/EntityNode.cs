using Avalonia.Media.Imaging;
using ReactiveUI;
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

    public EntityNode(MapTreeViewModel parent, string name, int x, int y, Bitmap icon, string title, string subTitle, string toolTip) : base(icon, name, title, toolTip)
    {
        X = x;
        Y = y;
        Subtitle = subTitle;
        CanDelete = true;

        DeleteEntityCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var id = 0;
            if (int.TryParse(this.Name, out id))
            {
                parent.DeleteEntityNode(id);
            }
        });
    }

    public EntityNode(MapTreeViewModel parent, string name, int x, int y, Bitmap icon, string title, string subTitle, string toolTip, ObservableCollection<Node> subNodes) : base(null, name, title, toolTip, subNodes)
    {
        X = x;
        Y = y;
        Subtitle = subTitle;
        CanDelete = true;

        DeleteEntityCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var id = 0;
            if (int.TryParse(this.Name, out id))
            {
                parent.DeleteEntityNode(id);
            }
        });
    }
}

