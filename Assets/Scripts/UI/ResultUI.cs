using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

using CommonsHelper;
using CommonsPattern;

public class ResultUI : SingletonManager<ResultUI>
{
    /* Strings */

    [SerializeField, Tooltip("Victory message string format. {0} will be replaced by player number.")]
    private string victoryTextFormat = "Player {0} wins!";

    [SerializeField, Tooltip("Draw message string format.")]
    private string drawTextFormat = "Draw!";

    [SerializeField, Tooltip("Next race button text when there are next stages left.")]
    private string nextRaceTextString = "Next race";

    [SerializeField, Tooltip("Next race button text when there are no stages left so we restart from stage 1 (index 0).")]
    private string firstRaceTextString = "Restart from stage 1";


    /* Child references */
    
    [Tooltip("First button to select")]
    public GameObject firstSelected;

    [Tooltip("Victory text component")] public TextMeshProUGUI victoryText;

    [Tooltip("Next race button text component")]
    public TextMeshProUGUI nextRaceText;
    
    [Tooltip("Player time widgets parent transform")]
    public Transform playerTimesParent;
    
    /// List of player time widgets
    private readonly List<PlayerTime> m_PlayerTimes = new List<PlayerTime>();


    /* Sibling components */

    protected override void Init()
    {
        foreach (Transform playerTimeTr in playerTimesParent)
        {
            var playerTime = playerTimeTr.GetComponentOrFail<PlayerTime>();
            m_PlayerTimes.Add(playerTime);
        }
        
        // deactivate until needed (done on Awake to avoid re-hiding after race start on Start)
        // do not only disable canvas, we must prevent any interactions
        Deactivate();
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void ShowResult(int winnerNumber, List<RaceManager.FinishInfo> finishInfoList, bool wasLastRace)
    {
        gameObject.SetActive(true);

        victoryText.text = string.Format(winnerNumber > 0 ? victoryTextFormat : drawTextFormat, winnerNumber);

        // show finish time for each player
        for (int i = 0; i < finishInfoList.Count; i++)
        {
            m_PlayerTimes[i].SetTime(finishInfoList[i].time);
        }
        
        nextRaceText.text = string.Format(wasLastRace ? firstRaceTextString : nextRaceTextString, winnerNumber);
        
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    // Button callbacks
    
    public void OnRetryButtonClick()
    {
        UISFXPlayer.Instance.PlaySFX(UISFXPlayer.SFX.Confirm);
        RaceManager.Instance.RestartRace();
    }

    public void OnNextButtonClick()
    {
        UISFXPlayer.Instance.PlaySFX(UISFXPlayer.SFX.Confirm);
        RaceManager.Instance.StartNextRace();
    }

    public void OnExitButtonClick()
    {
        UISFXPlayer.Instance.PlaySFX(UISFXPlayer.SFX.Cancel);
        RaceManager.Instance.GoBackToTitle();
    }
}
