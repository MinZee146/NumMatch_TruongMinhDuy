using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardController : Singleton<BoardController>
{
    public IReadOnlyList<Tile> TileList => _tileList;
    
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private Transform _tilesContainer;
    [SerializeField] private ScrollRect _scrollRect;
    
    private List<Tile> _tileList = new();

    private Tile _currentSeletedTile;
    private int _currentNumberedTiles;
    private const int Cols = 9;
    
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
    
    //Update selected tile UI, clear rows if empty
    public void UpdateCurrentSelectedTile(Tile tile)
    {
        if (_currentSeletedTile == tile)
        {
            _currentSeletedTile.UpdateSelector(false);
            return;
        }

        if (_currentSeletedTile != null)
        {
            _currentSeletedTile.UpdateSelector(false);
            
            if (_currentSeletedTile.CanMatch(tile.Index, tile.Number))
            {
                _currentSeletedTile.Disable();
                tile.Disable();
                
                CheckAndCollapseEmptyRows();

                _currentSeletedTile = null;
                return;
            }
        }
        
        _currentSeletedTile = tile;
        _currentSeletedTile.UpdateSelector(true);
    }
    
    private void CheckAndCollapseEmptyRows()
    {
        var totalRows = _currentNumberedTiles / Cols;
        var maxChecks = totalRows;

        for (var row = 0; row < totalRows && maxChecks > 0; row++, maxChecks--)
        {
            if (!IsRowEmpty(row)) continue;
            CollapseRowsAbove(row);
            row--;
        }
    }

    private bool IsRowEmpty(int row)
    {
        for (var col = 0; col < Cols; col++)
        {
            var index = row * Cols + col;
            if (!_tileList[index].IsDisabled)
                return false;
        }
        
        return true;
    }

    private void CollapseRowsAbove(int emptyRow)
    {
        var totalRows = _currentNumberedTiles / Cols;

        for (var row = emptyRow + 1; row < totalRows; row++)
        {
            for (var col = 0; col < Cols; col++)
            {
                var fromIndex = row * Cols + col;
                var toIndex = (row - 1) * Cols + col;

                var fromTile = _tileList[fromIndex];
                var toTile = _tileList[toIndex];

                toTile.LoadData(fromTile.Number, toIndex, fromTile.IsDisabled);
            }
        }

        var lastRowStart = (totalRows - 1) * Cols;
        
        for (var col = 0; col < Cols; col++)
        {
            _tileList[lastRowStart + col].Clear();
        }
        
        _currentNumberedTiles -= Cols;
    }
}
