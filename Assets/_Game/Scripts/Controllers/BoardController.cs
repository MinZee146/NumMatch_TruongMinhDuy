using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private Transform _tilesContainer;
    
    private List<GameObject> _tileList = new();
    
    private void Start()
    {
        //Generate 12 rows to begin with
        GenerateBoard(12 * 9);
    }

    private void GenerateBoard(int rows)
    {
        for (var i = 0; i < rows; i++)
        {
            var tile = Instantiate(_tilePrefab, _tilesContainer);
            _tileList.Add(tile);
        }
    }
}
