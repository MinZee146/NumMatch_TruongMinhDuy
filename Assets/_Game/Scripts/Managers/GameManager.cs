using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class GemMission
{
    public GemType Type;
    public int GemsLeftCount;
    public int TargetAmount;
}

public class GameManager : Singleton<GameManager>
{
    public Transform GemCollector => _gemsCollector.transform;
    public List<GemMission> CurrentGemMissions { get; private set; }
    public int CurrentStage { get; private set; }
    public int CurrentAddTiles { get; private set; }
    public int MaxGemsPerTurn { get; private set; }

    [SerializeField] private TextMeshProUGUI _stageText;
    [SerializeField] private TextMeshProUGUI _addTileCountText;
    [SerializeField] private GameObject _losePopUp, _winPopUp;
    [SerializeField] private GameObject _gemsCollector;
    [SerializeField] private GameObject _goalPrefab;

    private const int AddMorePerStage = 6;

    private GameObject _win, _lose;
    private List<(GemGoal goal, GemType gemType)> _goals = new();
    
    private void Start()
    {
        BoardController.Instance.GenerateBoard(9 * 10);
        ProceedsToNextStage();
    }
    
    private void GenerateGemMission()
    {
        var allTypes = System.Enum.GetValues(typeof(GemType)).Cast<GemType>().ToList();
        var typeCount = Random.Range(1, 4);
        var selectedTypes = allTypes.OrderBy(_ => Random.value).Take(typeCount);

        CurrentGemMissions = selectedTypes
            .Select(type =>
            {
                var target = Random.Range(3, 6);
                return new GemMission
                {
                    Type = type,
                    TargetAmount = target,
                    GemsLeftCount = target
                };
            })
            .ToList();
        
        MaxGemsPerTurn = CurrentGemMissions.Count;

        foreach (var mission in CurrentGemMissions)
        {
            var gemGoal = Instantiate(_goalPrefab, _gemsCollector.transform).GetComponent<GemGoal>();
            gemGoal.LoadMission(mission.Type, mission.TargetAmount);
            
            _goals.Add((gemGoal, mission.Type));
        }
    }

    private void ResetMissionsUI()
    {
        foreach (Transform obj in _gemsCollector.transform)
        {
            Destroy(obj.gameObject);
        }
        
        _goals.Clear();
    }

    public void UpdateProgress(GemType gemType)
    {
        var gemMission = _goals.Find(goal => goal.gemType == gemType);
        gemMission.goal.UpdateProgress();

        CurrentGemMissions.Find(mission => mission.Type == gemType).GemsLeftCount--;
    }

    private void ProceedsToNextStage()
    {
        CurrentStage++;
        _stageText.text = $"Stage: {CurrentStage}";
        CurrentAddTiles = AddMorePerStage;
        _addTileCountText.text = CurrentAddTiles.ToString();
        
        BoardController.Instance.ClearBoard();
        ResetMissionsUI();
        GenerateGemMission();
        
        BoardController.Instance.LoadInitialData(GetComponent<StageGenerator>().GenerateStage(CurrentStage));
    }

    public void ToggleLosePopUp()
    {
        if (_lose != null)
        {
            if (_lose.activeSelf)
            {
                CurrentStage--;
                ProceedsToNextStage();
            }
                
            _lose.SetActive(!_lose.activeSelf);
            return;
        }
        
        _lose = Instantiate(_losePopUp, FindObjectOfType<Canvas>().transform);
    }

    public void ToggleWinPopup()
    {
        if (_win != null)
        {
            if (_win.activeSelf)
                ProceedsToNextStage();
                
            _win.SetActive(!_win.activeSelf);
            return;
        }
        
        _win = Instantiate(_winPopUp, FindObjectOfType<Canvas>().transform);
    }

    public void AddMoreTiles()
    {
        if (CurrentAddTiles <= 0 || DOTween.TotalPlayingTweens() > 0) return;
        
        CurrentAddTiles--;
        _addTileCountText.text = CurrentAddTiles.ToString();
        
        BoardController.Instance.LoadMoreTiles();
    }
}
