using TMPro;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public int CurrentStage { get; private set; }
    public int CurrentAddTiles { get; private set; }

    [SerializeField] private TextMeshProUGUI _stageText;
    [SerializeField] private TextMeshProUGUI _addTileCountText;
    [SerializeField] private GameObject _losePopUp, _winPopUp;

    private const int AddMorePerStage = 6;

    private GameObject _win, _lose;
    
    private void Start()
    {
        BoardController.Instance.GenerateBoard(9 * 10);
        ProceedsToNextStage();
    }

    public void ProceedsToNextStage()
    {
        CurrentStage++;
        _stageText.text = $"Stage: {CurrentStage}";
        CurrentAddTiles = AddMorePerStage;
        _addTileCountText.text = CurrentAddTiles.ToString();
        
        BoardController.Instance.LoadInitialData(GetComponent<StageGenerator>().Test());
    }

    public void ToggleLosePopUp()
    {
        if (_lose != null)
        {
            if (_lose.activeSelf)
            {
                CurrentAddTiles = AddMorePerStage;
                _addTileCountText.text = CurrentAddTiles.ToString();
                
                BoardController.Instance.ClearBoard();
                BoardController.Instance.LoadInitialData(GetComponent<StageGenerator>().GenerateStage(CurrentStage));
            }
                
            _lose.SetActive(!_win.activeSelf);
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
        if (CurrentAddTiles <= 0) return;
        
        CurrentAddTiles--;
        _addTileCountText.text = CurrentAddTiles.ToString();
        
        BoardController.Instance.LoadMoreTiles();
    }
}
