using UnityEngine.Diagnostics;

public class Player
{
    public readonly string Name;
    public int WinCount;
    public int LooseCount;
    public int DrawCount;
    public int MatchCount;

    public Player(string name)
    {
        Name = name;
    }
}