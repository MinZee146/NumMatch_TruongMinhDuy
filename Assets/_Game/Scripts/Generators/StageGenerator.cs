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
    
    //i, j: tile's indexes
    private bool CanMatch(int[] board, int i, int j)
    {
        int a = board[i], b = board[j];
        if (!(a == b || a + b == 10)) return false;
        
        var diff = Math.Abs(i - j);
        return diff is 1 or 9 or 10 or 8;
    }
}
