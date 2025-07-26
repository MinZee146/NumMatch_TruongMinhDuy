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
        var board = new List<int>();
        return board;
    }
}
