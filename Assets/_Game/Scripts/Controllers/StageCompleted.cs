using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class StageCompleted : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private Button _nextLevel;
    [SerializeField] private GameObject _gemImagePrefab;
    [SerializeField] private Transform _gemImageParent;

    private void Start()
    {
        _nextLevel.onClick.AddListener(() => GameManager.Instance.ToggleWinPopup(false));
    }

    private void OnEnable()
    {
        _levelText.text = $"LEVEL {GameManager.Instance.CurrentStage} COMPLETED";

        foreach (Transform gemImage in _gemImageParent)
        {
            Destroy(gemImage.gameObject);
        }
        
        foreach (var mission in GameManager.Instance.CurrentGemMissions)
        {
            var gemGoal = Instantiate(_gemImagePrefab, _gemImageParent);
            Addressables.LoadAssetAsync<Sprite>($"{mission.Type}").Completed += (handle) =>
            {
                gemGoal.GetComponent<Image>().sprite = handle.Result;
            };
        }
    }
}
