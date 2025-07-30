using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GemType
{
    Pink,
    Orange,
    Purple
}

public class Gem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _numberText;
    [SerializeField] private Image _gemImage;
    [SerializeField] private Sprite _purpleSprite, _orangeSprite, _pinkSprite;
    [SerializeField] private GameObject _selector;
    
    private GemType _gemType;

    private void OnEnable()
    {
        transform.DOScale(1f, 0.5f).SetEase(Ease.OutBounce);
    }

    public void LoadData(int number, GemType gemType)
    {
        _numberText.text = number.ToString();
        _gemType = gemType;

        _gemImage.sprite = _gemType switch
        {
            GemType.Pink => _pinkSprite,
            GemType.Orange => _orangeSprite,
            GemType.Purple => _purpleSprite,
        };
    }

    public void UpdateSelector(bool isSelected)
    {
        _selector.GetComponent<Image>().DOFade(isSelected ? 1f : 0f, 0.2f);
    }

    public void CollectAnimation(Transform collector)
    {
        transform.SetParent(FindObjectOfType<Canvas>().transform);
        
        var seq = DOTween.Sequence();

        seq.Append(transform.DOMove(collector.position, 1f).SetEase(Ease.InBack));
        seq.Join(transform.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360));

        seq.AppendCallback(() =>
        {
            Destroy(gameObject);
        });
    }
}
