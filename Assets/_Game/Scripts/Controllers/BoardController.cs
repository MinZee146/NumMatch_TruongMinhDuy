using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private Transform _tilesContainer;
    
    private List<Tile> _tileList = new();
    private int _currentNumberedTiles;
    
    private void Start()
    {
        //Generate 12 rows to begin with
        GenerateBoard(12 * 9);
        LoadData(9 * 3);
    }

    private void GenerateBoard(int rows)
    {
        for (var i = 0; i < rows; i++)
        {
            var tile = Instantiate(_tilePrefab, _tilesContainer);
            _tileList.Add(tile.GetComponent<Tile>());
        }
    }

    //Load data into the next *tilesCount tiles
    private void LoadData(int tilesCount)
    {
        for (var i = _currentNumberedTiles; i < tilesCount + _currentNumberedTiles; i++)
        {
            _tileList[i].LoadData(4);
        }
        
        _currentNumberedTiles += tilesCount;
    }
}
