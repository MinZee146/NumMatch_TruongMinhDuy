using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardController : Singleton<BoardController>
{
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private Transform _tilesContainer;
    [SerializeField] private ScrollRect _scrollRect;
    
    private List<Tile> _tileList = new();

    private Tile _currentSeletedTile;
    private int _currentNumberedTiles;
    
    private void Start()
    {
        //Generate 10 rows to begin with
        GenerateBoard(10 * 9);
        LoadData(9 * 3);
        // LoadInitialData(GetComponent<StageGenerator>().GenerateStage(1));
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
    public void LoadData(int tilesCount)
    {
        for (var i = _currentNumberedTiles; i < tilesCount + _currentNumberedTiles; i++)
        {
            _tileList[i].LoadData(4, i);
        }
        
        _currentNumberedTiles += tilesCount;
    }

    private void LoadInitialData(List<int> board)
    {
        for (var i = 0; i < 27; i++)
        {
            _tileList[i].LoadData(board[i], i);
        }
        
        _currentNumberedTiles = 27;
    }

    public void UpdateCurrentSeletedTile(Tile tile)
    {
        if (_currentSeletedTile == tile)
        {
            _currentSeletedTile.UpdateSelector(false);
            return;
        }

        if (_currentSeletedTile != null)
        {
            _currentSeletedTile.UpdateSelector(false);
            
            if ( _currentSeletedTile.CanMatch(tile.Index, tile.Number))
            {
                _currentSeletedTile.Disable();
                tile.Disable();

                _currentSeletedTile = null;
                return;
            }
        }
        
        _currentSeletedTile = tile;
        _currentSeletedTile.UpdateSelector(true);
    }
}
