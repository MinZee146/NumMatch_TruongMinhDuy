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
    
    public void LoadData(int number, int index)
    {
        Number = number;
        Index = index;

        _numberText.text = number.ToString();
        _numberText.color = _normalTextColor;
        
        _button.interactable = true;
        _button.onClick.AddListener(() => BoardController.Instance.UpdateCurrentSeletedTile(this));
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
        Number = 0;
        
        _button.interactable = false;
        _numberText.color = _disabledTextColor;
    }
    
    //Assuming that both tiles are still active
    public bool CanMatch(int targetTileIndex, int targetTileNumber)
    {
        if (!(Number == targetTileNumber || Number + targetTileNumber == 10)) return false;

        var diff = Math.Abs(Index - targetTileIndex);

        if (diff is 1 or 9 or 10 or 8)
            return true;
        
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
        // ↘ / ↖
        else if (row2 - row1 == col2 - col1)
            step = row2 > row1 ? cols + 1 : -(cols + 1);
        // ↙ / ↗
        else if (row2 - row1 == -(col2 - col1))
            step = row2 > row1 ? cols - 1 : -(cols - 1);
        else
            return false;

        // Check for blocking tiles between
        var current = Index + step;
        while (current != targetTileIndex)
        {
            if (BoardController.Instance.TileList[current].Number != 0)
                return false;

            current += step;
        }

        return true;
    }
}
