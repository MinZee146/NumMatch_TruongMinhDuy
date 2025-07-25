using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _number;
    [SerializeField] private GameObject _selector;
    [SerializeField] private Color _normalTextColor, _disabledTextColor;
    [SerializeField] private Button _button;
    
    private bool _isSelected;
    
    public void LoadData(int number)
    {
        _number.text = number.ToString();

        _button.interactable = true;
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(UpdateSelector);
    }

    private void UpdateSelector()
    {
        if (!_isSelected)
        {
            _selector.transform.DOScale(1f, 0.2f);
            _isSelected = true;
        }
        else
        {
            var image = _selector.GetComponent<Image>();
            
            image.DOFade(0f, 0.2f)
                .OnComplete(() =>
            {
                _selector.transform.localScale = Vector3.zero;
                image.DOFade(1f, 0f);
                _isSelected = false;
            });
        }
    }
}
