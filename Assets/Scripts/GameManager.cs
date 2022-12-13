using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Timeline;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public TextAsset csvFile;
    private HashSet<Player> _players;

    private Match[] _matches;

    private const int FirstMatchColumn = 4;

    private void Start()
    {
        _players = new HashSet<Player>();
        string[,] grid = SplitCsvGrid(csvFile.text);

        DebugOutputGrid(grid); 
        FillPlayerStats(_players, _matches);

        HashSet<Player> mostPlayed = GetMostPlayedPlayers(_players);
        Player player = mostPlayed.First();
        print("Most Played Player: " + player.Name + " with "+ player.MatchCount + " matches");

        HashSet<Player> sortedByWinCount = SortByWinCount(_players);
        player = sortedByWinCount.First();
        print("Top Winner: " + player.Name + " with "+ player.WinCount + " wins");

        HashSet<Player> sortedByLooseCount = SortByLooseCount(_players);
        player = sortedByLooseCount.First();
        print("Top Looser: " + player.Name + " with "+ player.LooseCount + " looses");
        
        HashSet<Player> sortedByDrawCount = SortByDrawCount(_players);
        player = sortedByDrawCount.First();
        print("Top Drawer: " + player.Name + " with "+ player.DrawCount + " draws");
    }

    public List<List<Player>> combination(List<Player> players, bool triple)
    {
        List<List<Player>> comb = new List<List<Player>>();
        
        for (int i = 0; i < players.Count; i++) {
            for (int j = 1; j < players.Count; j++) {
                if (j == i) {
                    continue;
                }
                if (!triple) {
                    List<Player> l = new List<Player>();
                    l.Add(players[i]);
                    l.Add(players[j]);
                    comb.Add(l);
                }
                for (int k = 2; triple && k < players.Count; k++) {
                    if (k == j || j == i || i == k) {
                        continue;
                    }
                    if (triple) {
                        List<Player> m = new List<Player>();
                        m.Add(players[i]);
                        m.Add(players[j]);
                        m.Add(players[k]);
                        comb.Add(m);
                    }
                }
            }
        }
        return comb;
    }

    private HashSet<Player> GetMostPlayedPlayers(HashSet<Player> players)
    {
        HashSet<Player> mostPlayedPlayers = players.OrderByDescending(player => player.MatchCount).ToHashSet();
        return mostPlayedPlayers;
    }
    
    private HashSet<Player> SortByWinCount(HashSet<Player> players)
    {
        HashSet<Player> sortedByWinCount = players.OrderByDescending(player => player.WinCount).ToHashSet();
        return sortedByWinCount;
    }
    
    private HashSet<Player> SortByLooseCount(HashSet<Player> players)
    {
        HashSet<Player> sortedByLooseCount = players.OrderByDescending(player => player.LooseCount).ToHashSet();
        return sortedByLooseCount;
    }
    
    private HashSet<Player> SortByDrawCount(HashSet<Player> players)
    {
        HashSet<Player> sortedByDrawCount = players.OrderByDescending(player => player.DrawCount).ToHashSet();
        return sortedByDrawCount;
    }

    private void FillPlayerStats(HashSet<Player> players, Match[] matches)
    {
        foreach (var player in players)
        {
            foreach (var match in matches)
            {
                if (match.IsWinner(player)) player.WinCount++;
                if (match.IsLooser(player)) player.LooseCount++;
                if (match.NeitherWinnerNorLooser(player)) player.DrawCount++;
                if (match.IsPlayed(player)) player.MatchCount++;
            }
        }
    }

    #region CSVReader
    
    // outputs the content of a 2D array, useful for checking the importer
    public void DebugOutputGrid(string[,] grid)
    {
        _matches = null;
        for (int y = 1; y < grid.GetUpperBound(1) - 1; y++)
        {
            Player newPlayer = new Player(grid[0, y]);
            _players.Add(newPlayer);
            for (int x = FirstMatchColumn; x < grid.GetUpperBound(0); x++) 
            {
                if(_matches == null)
                {
                    _matches = new Match[grid.GetUpperBound(0) - FirstMatchColumn];
                }

                Match match = _matches[x - FirstMatchColumn];
                if (match == null)
                {
                    match = new Match(grid[x, 0]);
                    _matches[x - FirstMatchColumn] = match;
                }
                
                string score = grid[x, y];
                
                if (score.Equals("1"))
                {
                    match.Team1.Add(newPlayer);
                    match.Winner = 1;
                }
                else if (score.Equals("0"))
                {
                    match.Team2.Add(newPlayer);
                    match.Winner = 1;
                }
                else if (score.Equals("0.501"))
                {
                    match.Team1.Add(newPlayer);
                    match.Winner = 3;
                }
                else if (score.Equals("0.502"))
                {
                    match.Team2.Add(newPlayer);
                    match.Winner = 3;
                }
                else if (score.Equals("0.5"))
                {
                    throw new IOException("separate teams for equality");
                }
            }
        }
    }
 
    // splits a CSV file into a 2D string array
    public string[,] SplitCsvGrid(string csvText)
    {
        string[] lines = csvText.Split("\n"[0]); 
 
        // finds the max width of row
        int width = 0; 
        for (int i = 0; i < lines.Length; i++)
        {
            string[] row = SplitCsvLine( lines[i] ); 
            width = Mathf.Max(width, row.Length); 
        }
 
        // creates new 2D string grid to output to
        string[,] outputGrid = new string[width + 1, lines.Length + 1]; 
        for (int y = 0; y < lines.Length; y++)
        {
            string[] row = SplitCsvLine( lines[y] ); 
            for (int x = 0; x < row.Length; x++) 
            {
                outputGrid[x,y] = row[x]; 
 
                // This line was to replace "" with " in my output. 
                // Include or edit it as you wish.
                outputGrid[x,y] = outputGrid[x,y].Replace("\"\"", "\"");
            }
        }
 
        return outputGrid; 
    }
 
    // splits a CSV row 
    public string[] SplitCsvLine(string line)
    {
        return (from System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(line,
                @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)",
                System.Text.RegularExpressions.RegexOptions.ExplicitCapture)
            select m.Groups[1].Value).ToArray();
    }
    #endregion
}
