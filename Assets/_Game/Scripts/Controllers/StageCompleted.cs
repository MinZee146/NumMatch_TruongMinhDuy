using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageCompleted : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private Button _nextLevel;

    private void Start()
    {
        _nextLevel.onClick.AddListener(() => GameManager.Instance.ToggleWinPopup());
        _levelText.text = $"LEVEL {GameManager.Instance.CurrentStage} COMPLETED";
    }
}
