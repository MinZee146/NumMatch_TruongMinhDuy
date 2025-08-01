using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class StageGenerator : MonoBehaviour
{
    private Random _rng = new();
    
    //Generate 27 random numbers, then fix the extra pairs
    public List<int> GenerateStage(int stage)
    {
        var requiredPairs = stage switch
        {
            1 => 3,
            2 => 2,
            _ => 1
        };

        while (true)
        {
            List<int> numbers = new();
            for (var i = 1; i <= 9; i++)
            {
                numbers.Add(i);
                numbers.Add(i);
                numbers.Add(i);
            }

            Shuffle(numbers);
            var board = numbers.ToArray();

            var pairs = GetAllMatchablePairs_MaximalNonOverlapping(board);
            var currentPairCount = pairs.Count;

            if (currentPairCount < requiredPairs)
                continue;

            if (currentPairCount == requiredPairs)
                return board.ToList();

            var fixedOK = TryFixExtraPairs(board, pairs, currentPairCount, requiredPairs);

            if (fixedOK && GetAllMatchablePairs_MaximalNonOverlapping(board).Count == requiredPairs)
                return board.ToList();
        }
    }

    //Break extra pairs by swapping one of their numbers with another number in board
    private bool TryFixExtraPairs(int[] board, List<(int, int)> originalPairs, int currentPairCount, int targetPairCount)
    {
        var extraToBreak = currentPairCount - targetPairCount;
        var length = board.Length;
        var backup = board.ToArray();

        for (var k = 0; k < originalPairs.Count && extraToBreak > 0; k++)
        {
            var (i, j) = originalPairs[k];

            if (TrySwapToBreak(board, i, j, currentPairCount))
            {
                extraToBreak--;
                currentPairCount--;
                continue;
            }

            if (TrySwapToBreak(board, j, i, currentPairCount))
            {
                extraToBreak--;
                currentPairCount--;
                continue;
            }

            Array.Copy(backup, board, length);
            return false;
        }

        return extraToBreak == 0;
    }
    
    private bool TrySwapToBreak(int[] board, int swapIndex, int otherIndex, int originalPairCount)
    {
        var length = board.Length;

        for (var t = 0; t < length; t++)
        {
            if (t == swapIndex || t == otherIndex)
                continue;

            (board[swapIndex], board[t]) = (board[t], board[swapIndex]);

            var newPairCount = GetAllMatchablePairs_MaximalNonOverlapping(board).Count;

            if (newPairCount == originalPairCount - 1)
            {
                return true;
            }

            (board[swapIndex], board[t]) = (board[t], board[swapIndex]);
        }

        return false;
    }
    
    //Get the list of all current pairs
    private List<(int, int)> GetAllMatchablePairs_MaximalNonOverlapping(int[] board)
    {
        var used = new bool[board.Length];
        var result = new List<(int, int)>();
        var bestResult = new List<(int, int)>();

        void Backtrack()
        {
            var found = false;
            for (var i = 0; i < board.Length; i++)
            {
                if (used[i]) continue;

                for (var j = i + 1; j < board.Length; j++)
                {
                    if (used[j]) continue;

                    if (CanMatch(board, i, j))
                    {
                        found = true;
                        used[i] = used[j] = true;
                        result.Add((i, j));

                        Backtrack();

                        result.RemoveAt(result.Count - 1);
                        used[i] = used[j] = false;
                    }
                }

                if (found) break;
            }

            if (result.Count > bestResult.Count)
            {
                bestResult = new List<(int, int)>(result);
            }
        }

        Backtrack();
        return bestResult;
    }

    public List<int> Test()
    {
        return Enumerable.Repeat(4, 27).ToList();
    }

    private void Shuffle(List<int> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = _rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
    
    //i, j: tile's indexes
    private bool CanMatch(int[] board, int i, int j)
    {
        int a = board[i], b = board[j];
        if (!(a == b || a + b == 10)) return false;
        
        var diff = Math.Abs(i - j);
        if (diff is 1 or 9 or 10)
            return true;
        
        const int cols = 9;
        var row1 = i / cols;
        var row2 = j / cols;

        //Edge case
        return diff == 8 && row1 != row2;
    }
}
