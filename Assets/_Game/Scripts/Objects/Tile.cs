using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _numberText;
    [SerializeField] private GameObject _selector;
    [SerializeField] private Color _normalTextColor, _disabledTextColor;
    [SerializeField] private Button _button;

    public int Index { get; private set; }
    public int Number { get; private set; }
    public bool IsDisabled { get; private set; }
    
    public void LoadData(int number, int index, bool isDisabled = false)
    {
        Number = number;
        Index = index;
        IsDisabled = number == 0 || isDisabled;

        _numberText.text = number != 0 ? number.ToString() : "";
        _numberText.color = isDisabled ? _disabledTextColor : _normalTextColor;
        
        _button.interactable = !isDisabled;
        
        _button.onClick.RemoveAllListeners();
        if (!isDisabled)
            _button.onClick.AddListener(() => BoardController.Instance.UpdateCurrentSelectedTile(this));
    }

    public void UpdateSelector(bool isSelected)
    {
        if (isSelected)
        {
            _selector.transform.DOScale(1f, 0.2f);
        }
        else
        {
            var image = _selector.GetComponent<Image>();
            image.DOFade(0f, 0.2f)
                .OnComplete(() =>
            {
                _selector.transform.localScale = Vector3.zero;
                image.DOFade(1f, 0f);
            });
        }
    }

    public void Disable()
    {
        IsDisabled = true;
        
        _button.interactable = false;
        _numberText.color = _disabledTextColor;
    }

    public void Clear()
    {
        Number = 0;
        _numberText.text = "";
        _button.interactable = false;
    }

    public Tween ClearAnimation()
    {
        return _selector.transform.DOScale(1f, 0.1f).OnComplete(() =>
        {
            _selector.transform.DOScale(0f, 0.1f);
        });
    }

    public void SetUpClearAnimation(int row)
    {
        _numberText.transform.localPosition = new Vector3(0f, -100f * row, 0f);
    }
    
    public Tween SlideAnimation()
    {
        return _numberText.transform.DOLocalMove(Vector3.zero, 0.25f);
    }
    
    //Assuming that both tiles are still active
    public bool CanMatch(int targetTileIndex, int targetTileNumber)
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
            var current = Index + step;
            while (current != targetTileIndex)
            {
                if (!BoardController.Instance.TileList[current].IsDisabled)
                    return false;

                current += step;
            }

            return true;
        }
        
        //Special case: return true if nothing blocks in array
        var start = Mathf.Min(Index, targetTileIndex) + 1;
        var end = Mathf.Max(Index, targetTileIndex);

        for (var i = start; i < end; i++)
        {
            if (!BoardController.Instance.TileList[i].IsDisabled)
                return false;
        }

        return true;
    }
}
