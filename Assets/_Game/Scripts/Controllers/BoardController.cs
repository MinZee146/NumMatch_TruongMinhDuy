using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BoardController : Singleton<BoardController>
{
    public IReadOnlyList<Tile> TileList => _tileList;
    
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private Transform _tilesContainer;
    [SerializeField] private ScrollRect _scrollRect;
    
    private List<Tile> _tileList = new();

    private Tile _currentSelectedTile;
    private int _currentNumberedTiles;
    private int _totalRows;
    private const int Cols = 9;
    
    public void GenerateBoard(int tiles)
    {
        for (var i = 0; i < tiles; i++)
        {
            var tile = Instantiate(_tilePrefab, _tilesContainer);
            _tileList.Add(tile.GetComponent<Tile>());
        }
    }
    
    private void ApplyGemsToTiles(List<Tile> tiles, List<int> numbers)
    {
        var gemChance = Random.Range(0.05f, 0.07f);
        var gemEvery = Mathf.CeilToInt((numbers.Count + 1) / 2f);

        HashSet<int> gemIndices = new();

        var incompleteTypes = GameManager.Instance.CurrentGemMissions
            .Where(m => m.TargetAmount > 0)
            .Select(m => m.Type)
            .ToList();

        if (incompleteTypes.Count == 0) return;

        var maxGemsPerTurn = GameManager.Instance.MaxGemsPerTurn;
        var gemsPlaced = 0;

        for (var i = 0; i < numbers.Count; i++)
        {
            if (gemsPlaced >= maxGemsPerTurn) break;

            var forceGem = (i + 1) % gemEvery == 0;
            var tryGem = forceGem || Random.value < gemChance;

            if (!tryGem) continue;

            var canMatchWithAnotherGem = false;
            foreach (var j in gemIndices)
            {
                if (tiles[j].CanMatch(i, numbers[i], false))
                {
                    canMatchWithAnotherGem = true;
                    break;
                }
            }

            if (canMatchWithAnotherGem) continue;

            var selectedType = incompleteTypes[Random.Range(0, incompleteTypes.Count)];
            tiles[i].SetGem(selectedType);
            gemIndices.Add(i);
            gemsPlaced++;
        }
    }

    public void LoadMoreTiles()
    {
        AudioManager.Instance.PlaySfx("pop");
        
        var numbersToCopy = new List<int>();
        var sequence = DOTween.Sequence();

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
            _tileList[index].LoadData(numbersToCopy[i], index, fade: true);
            sequence.Append(_tileList[index].SpawnAnimation());
        }
        
        ApplyGemsToTiles(_tileList.GetRange(_currentNumberedTiles, numbersToCopy.Count), numbersToCopy);
        
        _currentNumberedTiles += numbersToCopy.Count;
        _totalRows = Mathf.CeilToInt((float)_currentNumberedTiles / Cols);
    }


    public void LoadInitialData(List<int> board)
    {
        for (var i = 0; i < 27; i++)
        {
            _tileList[i].LoadData(board[i], i);
        }
        
        ApplyGemsToTiles(_tileList, board);
        
        _currentNumberedTiles = 27;
        _totalRows = 3;
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
        
        sequence.AppendCallback(CheckForGameOver);
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
            for (var col = 0; col < Cols; col++)
            {
                var fromIndex = row * Cols + col;
                var toIndex = (row - 1) * Cols + col;

                var fromTile = _tileList[fromIndex];
                var toTile = _tileList[toIndex];
                
                toTile.LoadData(fromTile.Number, toIndex, fromTile.IsDisabled, gem: fromTile.Gem);
                
                if (fromTile.Gem != null)
                    fromTile.ClearGem();
            }
        }

        var lastRowStart = (_totalRows - 1) * Cols;
        for (var col = 0; col < Cols; col++)
        {
            _tileList[lastRowStart + col].Clear();
        }

        _totalRows = Mathf.CeilToInt((float)_currentNumberedTiles / Cols);
    }

    public void CheckForGameOver()
    {
        var remainingGems = GameManager.Instance.CurrentGemMissions.Count(m => m.TargetAmount > 0);
        var outOfTiles = GameManager.Instance.CurrentAddTiles <= 0;

        if (remainingGems == 0)
        {
            GameManager.Instance.ToggleWinPopup();
        }
        else if ((!HasAnyValidMatch() && outOfTiles) || (_currentNumberedTiles == 0 && remainingGems > 0))
        {
            GameManager.Instance.ToggleLosePopUp();
        }
    }

    
    private bool HasAnyValidMatch()
    {
        for (var i = 0; i < _currentNumberedTiles; i++)
        {
            var tileA = _tileList[i];
        
            if (tileA.IsDisabled || tileA.Number == 0)
                continue;

            for (var j = i + 1; j < _currentNumberedTiles; j++)
            {
                var tileB = _tileList[j];
            
                if (tileB.IsDisabled || tileB.Number == 0)
                    continue;

                if (tileA.CanMatch(tileB.Index, tileB.Number, false))
                    return true;
            }
        }

        return false;
    }

    public void ClearBoard()
    {
        foreach (var tile in _tileList)
        {
            tile.Clear();
        }
        
        _currentNumberedTiles = 0;
        _currentSelectedTile = null;
        _totalRows = 0;
        _scrollRect.verticalNormalizedPosition = 1f;
        _scrollRect.enabled = false;
    }

    public void DebugLog()
    {
        Debug.Log(_currentNumberedTiles);
    }
}
