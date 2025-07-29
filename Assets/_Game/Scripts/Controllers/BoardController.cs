using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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
    
    private List<Tile> _tileList = new();

    private Tile _currentSelectedTile;
    private int _currentNumberedTiles;
    private int _totalRows;
    private const int Cols = 9;

    private int _currentStage = 1;
    
    private void Start()
    {
        //Generate 10 rows to begin with
        GenerateBoard(10 * 9);
        LoadInitialData(GetComponent<StageGenerator>().Test());
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
        AudioManager.Instance.PlaySfx("pop");
        
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
        if (neededCapacity > 90)
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
        
        AudioManager.Instance.PlaySfx("choose_number");

        if (_currentSelectedTile != null)
        {
            _currentSelectedTile.UpdateSelector(false);
            
            if (_currentSelectedTile.CanMatch(tile.Index, tile.Number))
            {
                AudioManager.Instance.PlaySfx("pair_clear");
                
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
    
    private bool IsRowEmpty(int row)
    {
        for (var col = 0; col < Cols; col++)
        {
            var index = row * Cols + col;
            
            if (!_tileList[index].IsDisabled && _tileList[index].Number != 0)
                return false;
        }
        
        return true;
    }
    
    private void CheckAndCollapseEmptyRows()
    {
        var sequence = DOTween.Sequence();
        
        // clearedRows store cleared rows indexes and nonEmptyRows store the rest
        List<int> clearedRows = new();
        List<int> nonEmptyRows = new();
        
        for (var row = 0; row < _totalRows; row++)
        {
            if (IsRowEmpty(row))
            {
                clearedRows.Add(row);
            }
            else
            {
                nonEmptyRows.Add(row);
            }
        }

        if (clearedRows.Count == 0) return;
        
        clearedRows.Sort((a, b) => b.CompareTo(a));
        
        var oldTotalRows = _totalRows;

        foreach (var row in clearedRows)
        {
            CollapseRows(row);
            AudioManager.Instance.PlaySfx("row_clear");
        }
        
        //Calculate how much offset needed for each row, then animate
        for (var row = clearedRows.Min(); row < oldTotalRows; row ++)
        {
            var offset = row < _totalRows ? clearedRows.Count(cleared => cleared < nonEmptyRows[row]) : 0;
            
            var miniSequence = DOTween.Sequence();
            
            for (var col = 0; col < Cols; col++)
            {
                var tile = _tileList[row * Cols + col];

                if (row < _totalRows)
                {
                    tile.SetUpClearAnimation(offset);
                }

                if (clearedRows.Contains(row))
                {
                    miniSequence.Append(tile.ClearAnimation());
                }
            }
            
            sequence.Join(miniSequence);
        }
        
        sequence.AppendInterval(1f);
        
        for (var row = clearedRows.Min(); row < oldTotalRows; row++)
        {
            for (var col = 0; col < Cols; col++)
            {
                var tile = _tileList[row * Cols + col];
                sequence.Join(tile.SlideAnimation());
            }
        }
        
        sequence.AppendCallback(CheckForWinning);
    }

    //Move all the rows below up one row and delete the last row
    private void CollapseRows(int emptyRow)
    {
        for (var col = 0; col < Cols; col++)
        {
            var tile = _tileList[emptyRow * Cols + col];
            if (tile.IsDisabled && tile.Number != 0)
            {
                _currentNumberedTiles--;
            }
        }
        
        for (var row = emptyRow + 1; row < _totalRows; row++)
        {
            if (emptyRow == _totalRows - 1) break;
            
            for (var col = 0; col < Cols; col++)
            {
                var fromIndex = row * Cols + col;
                var toIndex = (row - 1) * Cols + col;

                var fromTile = _tileList[fromIndex];
                var toTile = _tileList[toIndex];

                toTile.LoadData(fromTile.Number, toIndex, fromTile.IsDisabled);
            }
        }

        var lastRowStart = (_totalRows - 1) * Cols;
        for (var col = 0; col < Cols; col++)
        {
            _tileList[lastRowStart + col].Clear();
        }

        _totalRows = Mathf.CeilToInt((float)_currentNumberedTiles / Cols);
    }

    private void CheckForWinning()
    {
        if (_totalRows == 0)
        {
            NextStage();
        }
    }

    private void NextStage()
    {
        LoadInitialData(GetComponent<StageGenerator>().GenerateStage(++_currentStage));
        
        _stageText.text = $"Stage: {_currentStage}";
    }

    public void DebugLog()
    {
        Debug.Log(_currentNumberedTiles);
    }
}
