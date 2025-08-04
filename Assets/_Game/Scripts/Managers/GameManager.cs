using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;

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
    public List<GemMission> CurrentGemMissions { get; private set; } = new();
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
        ProceedsToNextLevel();
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

    public void ProceedsToNextStage()
    {
        CurrentStage++;
        _stageText.text = $"Stage: {CurrentStage}";
        CurrentAddTiles = AddMorePerStage;
        _addTileCountText.text = CurrentAddTiles.ToString();
        
        MaxGemsPerTurn = CurrentGemMissions.Count(mission => mission.GemsLeftCount > 0);
        
        BoardController.Instance.ClearBoard();
        BoardController.Instance.LoadInitialData(GetComponent<StageGenerator>().GenerateStage(CurrentStage));
    }

    private void ProceedsToNextLevel()
    {
        BoardController.Instance.ClearBoard();
        
        CurrentStage = 0;
        
        ResetMissionsUI();
        GenerateGemMission();
        ProceedsToNextStage();
    }

    public void ToggleLosePopUp(bool active)
    {
        if (active)
        {
            if (_lose == null)
            {
                _lose = Instantiate(_losePopUp, FindObjectOfType<Canvas>().transform);
                return;
            }
            
            if (_lose.activeSelf) return;
            
            _lose.SetActive(true);
        }
        else
        {
            _lose.SetActive(false);
            ProceedsToNextLevel();
        }
    }

    public void ToggleWinPopUp(bool active)
    {
        if (active)
        {
            if (_win == null)
            {
                _win = Instantiate(_winPopUp, FindObjectOfType<Canvas>().transform);
                return;
            }
            
            if (_win.activeSelf) return;
            
            _win.SetActive(true);
        }
        else
        {
            _win.SetActive(false);
            ProceedsToNextLevel();
        }
    }

    public void AddMoreTiles()
    {
        if (CurrentAddTiles <= 0 || DOTween.TotalPlayingTweens() > 0) return;
        
        CurrentAddTiles--;
        _addTileCountText.text = CurrentAddTiles.ToString();
        
        BoardController.Instance.LoadMoreTiles();
    }
}
