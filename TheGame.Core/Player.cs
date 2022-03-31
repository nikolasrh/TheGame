namespace TheGame.Core;

public class Player
{
    public Player(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; }
    public string Name { get; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
}
