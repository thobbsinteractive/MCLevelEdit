namespace MCLevelEdit.Model.Domain;

public class Position
{
    public int X { get; set; }
    public int Y { get; set; }

    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Position Copy()
    {
        return new Position(X, Y);
    }

    public bool Equals(Position pos) => this.X == pos.X && this.Y == pos.Y;
};
