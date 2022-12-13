using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match
{
    public string Date;
    public HashSet<Player> Team1 = new HashSet<Player>();
    public HashSet<Player> Team2 = new HashSet<Player>();
    public int Winner = 0; // 0 -> Draw; 1 -> Team1; 2 -> Team2

    public Match(string date)
    {
        Date = date;
    }

    public bool IsPlayed(Player player)
    {
        return Team1.Contains(player) || Team2.Contains(player);
    }
    
    public bool IsWinner(Player player)
    {
        return (Winner == 1 && Team1.Contains(player)) || (Winner == 2 && Team2.Contains(player));
    }
    
    public bool IsLooser(Player player)
    {
        return (Winner == 2 && Team1.Contains(player)) || (Winner == 1 && Team2.Contains(player));
    }
    
    public bool NeitherWinnerNorLooser(Player player)
    {
        return (Winner == 3 && Team1.Contains(player)) || (Winner == 3 && Team2.Contains(player));
    }
}
