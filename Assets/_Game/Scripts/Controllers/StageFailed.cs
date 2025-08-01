    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class StageFailed : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Button _tryAgain;
        [SerializeField] private GameObject _gemGoalPrefab;
        [SerializeField] private Transform _gemGoalParent;

        private void Start()
        {
            _tryAgain.onClick.AddListener(() => GameManager.Instance.ToggleLosePopUp());
        }
        
        private void OnEnable()
        {
            _levelText.text = $"LEVEL {GameManager.Instance.CurrentStage}";
            
            foreach (Transform obj in _gemGoalParent)
            {
                Destroy(obj.gameObject);
            }
            
            foreach (var mission in GameManager.Instance.CurrentGemMissions)
            {
                var gemGoal = Instantiate(_gemGoalPrefab, _gemGoalParent).GetComponent<GemGoal>();
                gemGoal.LoadMission(mission.Type, mission.TargetAmount);
            }
        }
    }
