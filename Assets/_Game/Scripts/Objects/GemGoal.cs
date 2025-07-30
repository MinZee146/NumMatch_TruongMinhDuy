using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GemGoal : MonoBehaviour
{
    [SerializeField] private Image _gemImage;
    [SerializeField] private TextMeshProUGUI _gemCountText;

    private int _currentGem;

    public void LoadMission(GemType gemType, int gemCount)
    {
        _currentGem = gemCount;
        _gemCountText.text = gemCount.ToString();

        var address = $"{gemType}";
        Addressables.LoadAssetAsync<Sprite>(address).Completed += OnSpriteLoaded;
    }

    public void UpdateProgress()
    {
        _currentGem--;
        _gemCountText.text =  _currentGem.ToString();

        if (_currentGem <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnSpriteLoaded(AsyncOperationHandle<Sprite> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _gemImage.sprite = handle.Result;
        }
        else
        {
            Debug.LogWarning("Failed to load gem sprite.");
        }
    }
}