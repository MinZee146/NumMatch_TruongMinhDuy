using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class StageGenerator : MonoBehaviour
{
    private const int Rows = 3;
    private const int Cols = 9;
    private const int TotalTiles = Rows * Cols;
    
    private Random _rng = new();

    public List<int> GenerateStage(int stage)
    {
        var requiredPairs = stage switch
        {
            1 => 3,
            2 => 2,
            _ => 1
        };
        
        //Retry until having enough required pairs 
        while (true)
        {
            //Numbers list
            var numbers = new List<int>();
            for (var i = 1; i <= 9; i++)
            {
                numbers.Add(i);
                numbers.Add(i);
                numbers.Add(i);
            }

            Shuffle(numbers);

            //Add to board and check for matches
            var board = new int[TotalTiles];
            for (var i = 0; i < TotalTiles; i++)
            {
                board[i] = numbers[i];
            }

            var actualPairs = CountMatchablePairs(board);
            if (actualPairs == requiredPairs)
            {
                return new List<int>(board);
            }
        }
    }

    private void Shuffle(List<int> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = _rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private int CountMatchablePairs(int[] board)
    {
        var matched = new bool[board.Length];
        var count = 0;

        for (var i = 0; i < board.Length; i++)
        {
            //if this tile already matched with sth else, continue
            if (matched[i]) continue;

            for (var j = i + 1; j < board.Length; j++)
            {
                if (matched[j]) continue;

                if (CanMatch(board, i, j))
                {
                    matched[i] = matched[j] = true;
                    count++;
                    break;
                }
            }
        }

        return count;
    }

    private bool CanMatch(int[] board, int i, int j)
    {
        int a = board[i], b = board[j];
        if (!(a == b || a + b == 10)) return false;
        
        //calculate each tile's coordinate
        int r1 = i / Cols, c1 = i % Cols;
        int r2 = j / Cols, c2 = j % Cols;
        
        //calculate distance between 2 tiles
        int dr = r2 - r1, dc = c2 - c1;
        
        if (dr == 0 || dc == 0 || Math.Abs(dr) == Math.Abs(dc))
        {
            int step;
            
            //calculate step in 8 directions
            if (dr == 0) step = dc > 0 ? 1 : -1;
            else if (dc == 0) step = dr > 0 ? Cols : -Cols;
            else if (dr == dc) step = dr > 0 ? Cols + 1 : -(Cols + 1);
            else step = dr > 0 ? Cols - 1 : -(Cols - 1);

            var current = i + step;
            while (current != j)
            {
                if (board[current] != 0) return false;
                current += step;
            }

            return true;
        }

        return false;
    }
}
