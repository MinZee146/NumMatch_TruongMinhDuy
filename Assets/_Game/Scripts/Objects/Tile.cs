using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _numberText;
    [SerializeField] private GameObject _selector;
    [SerializeField] private Color _normalTextColor, _disabledTextColor, _fadeColor;
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _gemPrefab, _linePrefab, _hintPrefab;

    public Gem Gem { get; private set; }
    public int Index { get; private set; }
    public int Number { get; set; }
    public bool IsDisabled { get; private set; }
    
    public void LoadData(int number, int index, bool isDisabled = false, bool fade = false, Gem gem = null)
    {
        Number = number;
        Index = index;
        IsDisabled = number == 0 || isDisabled;

        _numberText.text = number != 0 ? number.ToString() : "";
        _numberText.color = fade ? _fadeColor : isDisabled ? _disabledTextColor : _normalTextColor;
        
        _button.interactable = !isDisabled;
        
        _button.onClick.RemoveAllListeners();
        
        if (!isDisabled)
            _button.onClick.AddListener(() => BoardController.Instance.UpdateCurrentSelectedTile(this));

        Gem = null;
        if (gem != null)
        {
            SetGem(gem.GemType, gem);
        }
    }

    public void SetGem(GemType gemType, Gem gem = null)
    {
        if (gem != null)
        {
            Gem = gem;
            Gem.transform.parent = transform;
            Gem.transform.localPosition = Vector3.zero;
            return;
        }
        
        Gem = Instantiate(_gemPrefab, transform).GetComponent<Gem>();
        Gem.LoadData(Number, gemType);
    }

    public void ClearGem()
    {
        Gem = null;
    }

    public void UpdateSelector(bool isSelected)
    {
        var image = _selector.GetComponent<Image>();
        if (DOTween.IsTweening(_selector.transform))
        {
            image.sprite = null;
        }

        if (Gem != null)
        {
            Gem.UpdateSelector(isSelected);
        }
        
        if (isSelected)
        {
            _selector.transform.DOScale(1f, 0.2f);
        }
        else
        {
            _selector.GetComponent<Image>().DOFade(0f, 0.2f)
                .OnComplete(() =>
            {
                _selector.transform.localScale = Vector3.zero;
                image.DOFade(1f, 0f);
            });
        }
    }

    public void InvalidateAnimation()
    {
        _selector.transform.localScale = Vector3.one;
        var image = _selector.GetComponent<Image>();
        image.DOFade(0f, 0.2f)
            .OnComplete(() =>
            {
                _selector.transform.localScale = Vector3.zero;
                image.DOFade(1f, 0f);
            });
    }

    public void Disable()
    {
        if (Gem != null)
        {
            Gem.CollectAnimation(GameManager.Instance.GemCollector);
            Gem = null;
        }
        
        IsDisabled = true;
        
        _button.interactable = false;
        _numberText.color = _disabledTextColor;
    }

    public void Clear()
    {
        Number = 0;
        _numberText.text = "";
        _button.interactable = false;
        
        if (Gem != null)
            Destroy(Gem.gameObject);
        Gem = null;
    }

    public Sequence SpawnAnimation()
    {
        var seq = DOTween.Sequence();

        seq.Join(ClearAnimation(0.05f));
        seq.Join(_numberText.DOColor(_normalTextColor, 0f));
        
        return seq;
    }

    public Tween ClearAnimation(float duration = 0.1f)
    {
        return _selector.transform.DOScale(1f, duration).OnComplete(() =>
        {
            _selector.transform.DOScale(0f, duration);
        });
    }

    public void SetUpClearAnimation(int row)
    {
        _numberText.transform.localPosition = new Vector3(0f, -100f * row, 0f);
        
        if (Gem != null)
            Gem.transform.localPosition = new Vector3(0f, -100f * row, 0f);
    }
    
    public Tween SlideAnimation()
    {
        var sequence = DOTween.Sequence();

        sequence.Join(_numberText.transform.DOLocalMove(Vector3.zero, 0.25f));

        if (Gem != null)
        {
            sequence.Join(Gem.transform.DOLocalMove(Vector3.zero, 0.25f));
        }

        return sequence;
    }

    private Tween Jiggle()
    {
        return _numberText.transform.DOShakePosition(0.5f, 9f);
    }

    public void HintAnimation()
    {
        var hint = Instantiate(_hintPrefab, transform);
        
        var sequence = DOTween.Sequence();

        sequence.Append(hint.transform.DOScale(1f, 0.1f)) 
            .Append(hint.transform.DORotate(new Vector3(0, 0, 360f), 0.5f, RotateMode.FastBeyond360))
            .AppendInterval(0.5f)
            .Append(hint.transform.DOScale(0f, 0.1f))
            .OnComplete(() => {
                Destroy(hint.gameObject);
            });
    }

    
    //Assuming that both tiles are still active
    public bool CanMatch(int targetTileIndex, int targetTileNumber, bool animated = true)
    {
        if (!(Number == targetTileNumber || Number + targetTileNumber == 10)) return false;

        const int cols = 9;

        int row1 = Index / cols, col1 = Index % cols;
        int row2 = targetTileIndex / cols, col2 = targetTileIndex % cols;

        int step;

        // Horizontal
        if (row1 == row2)
            step = col2 > col1 ? 1 : -1;
        // Vertical
        else if (col1 == col2)
            step = row2 > row1 ? cols : -cols;
        // Diagonal ↘ / ↖
        else if (row2 - row1 == col2 - col1)
            step = row2 > row1 ? cols + 1 : -(cols + 1);
        // Diagonal ↙ / ↗
        else if (row2 - row1 == -(col2 - col1))
            step = row2 > row1 ? cols - 1 : -(cols - 1);
        else
            step = 0;

        if (step != 0)
        {
            var sequence = DOTween.Sequence();
            var canMatch = true;

            var current = Index + step;
            while (current != targetTileIndex)
            {
                if (!BoardController.Instance.TileList[current].IsDisabled)
                {
                    if (!animated) return false;

                    sequence.Join(BoardController.Instance.TileList[current].Jiggle());
                    canMatch = false;
                }

                current += step;
            }

            if (!canMatch || !animated) return canMatch;
            var line = Instantiate(_linePrefab, transform).GetComponent<UILine>();
            line.CreateLine(BoardController.Instance.TileList[targetTileIndex].transform.position, transform.position);
            line.FadeAndDestroy();

            return true;
        }

        //Special case: return true if nothing blocks in array
        var start = Mathf.Min(Index, targetTileIndex) + 1;
        var end = Mathf.Max(Index, targetTileIndex);

        var sequence2 = DOTween.Sequence();
        var canMatch2 = true;

        for (var i = start; i < end; i++)
        {
            if (!BoardController.Instance.TileList[i].IsDisabled)
            {
                if (!animated) return false;

                sequence2.Join(BoardController.Instance.TileList[i].Jiggle());
                canMatch2 = false;
            }
        }

        if (!canMatch2 || !animated) return canMatch2;
        
        int upperIndex, lowerIndex;
        int upperRow, lowerRow;

        if (row1 < row2)
        {
            upperIndex = Index;
            upperRow = row1;
            lowerIndex = targetTileIndex;
            lowerRow = row2;
        }
        else
        {
            upperIndex = targetTileIndex;
            upperRow = row2;
            lowerIndex = Index;
            lowerRow = row1;
        }

        var endOfUpperRow = BoardController.Instance.TileList[upperRow * cols + (cols - 1)].transform.position;
        var startOfLowerRow = BoardController.Instance.TileList[lowerRow * cols].transform.position;

        var line1 = Instantiate(_linePrefab, transform).GetComponent<UILine>();
        line1.CreateLine(BoardController.Instance.TileList[upperIndex].transform.position, endOfUpperRow);
        line1.FadeAndDestroy();

        var line2 = Instantiate(_linePrefab, transform).GetComponent<UILine>();
        line2.CreateLine(startOfLowerRow, BoardController.Instance.TileList[lowerIndex].transform.position);
        line2.FadeAndDestroy();

        return true;
    }
}
