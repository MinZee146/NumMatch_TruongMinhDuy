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
        _button.interactable = false;
        _numberText.color = _disabledTextColor;
    }
    
    //Assume that both tiles are still active
    public bool CanMatch(int targetTileIndex, int targetTileNumber)
    {
        if (!(Number == targetTileNumber || Number + targetTileNumber == 10)) return false;

        var diff = Math.Abs(Index - targetTileIndex);

        return diff switch
        {
            1 or 9 or 10 or 8 => true,
            _ => false
        };
    }
}
