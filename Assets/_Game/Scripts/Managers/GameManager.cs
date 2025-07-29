using TMPro;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private TextMeshProUGUI _stageText;
    [SerializeField] private TextMeshProUGUI _addTileCountText;

    private const int AddMorePerStage = 6;
    
    private int _currentStage;
    private int _currentAddTiles;
    
    private void Start()
    {
        BoardController.Instance.GenerateBoard(9 * 10);
        ProceedsToNextStage();
    }

    public void ProceedsToNextStage()
    {
        _currentStage++;
        _stageText.text = $"Stage: {_currentStage}";
        _currentAddTiles = AddMorePerStage;
        _addTileCountText.text = _currentAddTiles.ToString();
        
        BoardController.Instance.LoadInitialData(GetComponent<StageGenerator>().Test());
    }

    public void AddMoreTiles()
    {
        if (_currentAddTiles <= 0) return;
        
        _currentAddTiles--;
        _addTileCountText.text = _currentAddTiles.ToString();
        
        BoardController.Instance.LoadMoreTiles();
    }
}
