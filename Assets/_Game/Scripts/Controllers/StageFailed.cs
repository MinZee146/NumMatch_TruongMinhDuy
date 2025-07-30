using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageFailed : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private Button _tryAgain;
    
    void Start()
    {
        _levelText.text = $"LEVEL {GameManager.Instance.CurrentStage}";
        
    }
}
