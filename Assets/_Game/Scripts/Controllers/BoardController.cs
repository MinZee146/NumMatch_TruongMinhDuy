using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardController : Singleton<BoardController>
{
    public IReadOnlyList<Tile> TileList => _tileList;
    
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private Transform _tilesContainer;
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private TextMeshProUGUI _stageText;
    [SerializeField] private GridLayoutGroup _gridLayoutGroup;
    
    private List<Tile> _tileList = new();

    private Tile _currentSelectedTile;
    private int _currentNumberedTiles;
    private int _currentStage = 1;
    private int _totalRows;
    private const int Cols = 9;
    
    private void Start()
    {
        //Generate 10 rows to begin with
        GenerateBoard(10 * 9);
        LoadInitialData(GetComponent<StageGenerator>().GenerateStage(_currentStage));
    }

    private void GenerateBoard(int tiles)
    {
        for (var i = 0; i < tiles; i++)
        {
            var tile = Instantiate(_tilePrefab, _tilesContainer);
            _tileList.Add(tile.GetComponent<Tile>());
        }
    }

    public void LoadMoreTiles()
    {
        var numbersToCopy = new List<int>();

        foreach (var tile in _tileList)
        {
            if (!tile.IsDisabled && tile.Number != 0)
            {
                numbersToCopy.Add(tile.Number);
            }
        }

        // Check if more rows are needed
        var neededCapacity = _currentNumberedTiles + numbersToCopy.Count;
        
        // Enable scroll
        if (_tileList.Count < neededCapacity)
        {
            _scrollRect.enabled = true;
        }
        
        while (_tileList.Count < neededCapacity)
        {
            // Instantiate more rows
            for (var i = 0; i < Cols; i++)
            {
                var tile = Instantiate(_tilePrefab, _tilesContainer);
                _tileList.Add(tile.GetComponent<Tile>());
            }
        }
        
        for (var i = 0; i < numbersToCopy.Count; i++)
        {
            var index = _currentNumberedTiles + i;
            _tileList[index].LoadData(numbersToCopy[i], index);
        }
        
        _currentNumberedTiles += numbersToCopy.Count;
        _totalRows = Mathf.CeilToInt((float)_currentNumberedTiles / Cols);
    }


    private void LoadInitialData(List<int> board)
    {
        for (var i = 0; i < 27; i++)
        {
            _tileList[i].LoadData(board[i], i);
        }
        
        _currentNumberedTiles = 27;
        _totalRows = Mathf.CeilToInt((float)_currentNumberedTiles / Cols);
    }
    
    //Update selected tile UI, clear rows if empty
    public void UpdateCurrentSelectedTile(Tile tile)
    {
        if (_currentSelectedTile == tile)
        {
            _currentSelectedTile.UpdateSelector(false);
            _currentSelectedTile = null;
            return;
        }

        if (_currentSelectedTile != null)
        {
            _currentSelectedTile.UpdateSelector(false);
            
            if (_currentSelectedTile.CanMatch(tile.Index, tile.Number))
            {
                _currentSelectedTile.Disable();
                tile.Disable();
                
                CheckAndCollapseEmptyRows();

                _currentSelectedTile = null;
                return;
            }
        }
        
        _currentSelectedTile = tile;
        _currentSelectedTile.UpdateSelector(true);
    }
    
    private void CheckAndCollapseEmptyRows()
    {
        for (var row = 0; row < _totalRows; row++)
        {
            if (!IsRowEmpty(row)) continue;
            CollapseRows(row);
            row--;
        }
        
        CheckForWinning();
    }

    private bool IsRowEmpty(int row)
    {
        for (var col = 0; col < Cols; col++)
        {
            var index = row * Cols + col;
            var tile = _tileList[index];
            if (!tile.IsDisabled && tile.Number != 0)
                return false;
        }
        
        return true;
    }
    
    //Collapse the empty row and replace it with the following row
    private void CollapseRows(int emptyRow)
    {
        _gridLayoutGroup.enabled = false;
        
        //Spawn 9 empty rows to replace the old ones
        GenerateBoard(9);

        var startIndex = emptyRow * Cols;

        // Destroy empty rows
        for (var i = 0; i < Cols; i++)
        {
            var tile = _tileList[startIndex + i];
            if (tile.Number != 0)
                _currentNumberedTiles--;
            
            Destroy(_tileList[startIndex + i].gameObject);
        }
        _tileList.RemoveRange(startIndex, Cols);
        
        // Update index
        for (var i = startIndex; i < startIndex + Cols; i++)
        {
            var tile = _tileList[i];
            tile.UpdateIndexByRow();
        }

        // Update total rows
        _totalRows = Mathf.CeilToInt((float)_currentNumberedTiles / Cols);

        _gridLayoutGroup.enabled = true;
    }

    private void CheckForWinning()
    {
        if (_totalRows == 0)
        {
            _currentNumberedTiles = 0;
            _currentSelectedTile = null;

            _currentStage++;
            
            _stageText.text = $"Stage: {_currentStage}";
            
            LoadInitialData(GetComponent<StageGenerator>().GenerateStage(_currentStage));
        }
    }
    
    //Debug only
    public void DebugLog()
    {
        Debug.Log($"Current numbered tiles: {_currentNumberedTiles}, Current tile list size: {_tileList.Count}, Total rows: {_totalRows}, Current stage: {_currentStage}");
    }

    public void ShowCurrentIndex()
    {
        Debug.Log($"{_currentSelectedTile.Index}");
    }
}
