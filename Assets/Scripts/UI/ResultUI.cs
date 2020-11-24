using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    [Tooltip("Victory text component")] public TextMeshProUGUI victoryText;

    [Tooltip("Next race button text component")]
    public TextMeshProUGUI nextRaceText;


    /* Sibling components */

    protected override void Init()
    {
        // deactivate until needed (done on Awake to avoid re-hiding after race start on Start)
        // do not only disable canvas, we must prevent any interactions
        Deactivate();
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void ShowResult(int winnerNumber, bool wasLastRace)
    {
        gameObject.SetActive(true);

        victoryText.text = string.Format(winnerNumber > 0 ? victoryTextFormat : drawTextFormat, winnerNumber);
        nextRaceText.text = string.Format(wasLastRace ? firstRaceTextString : nextRaceTextString, winnerNumber);
    }

    // Button callbacks
    
    public void OnRetryButtonClick()
    {
        RaceManager.Instance.RestartRace();
    }

    public void OnNextButtonClick()
    {
        RaceManager.Instance.StartNextRace();
    }

    public void OnExitButtonClick()
    {
        RaceManager.Instance.GoBackToTitle();
    }
}
