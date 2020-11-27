using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using CommonsHelper;
using CommonsPattern;

public class PauseUI : SingletonManager<PauseUI>
{
    /* Strings */

    [SerializeField, Tooltip("Skip race button string format when there are next stages left. {0} will be replaced by stage number.")]
    private string skipToNextStageTextFormat = "Skip to stage {0}";

    [SerializeField, Tooltip("Skip race button text when there are no stages left so we restart from stage 1 (index 0).")]
    private string skipToFirstStageTextString = "Restart from stage 1";


    /* Child references */

    [Tooltip("Skip race button text component")]
    public TextMeshProUGUI skipRaceText;


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

    public void ShowPauseMenu(int nextStageNumber)
    {
        gameObject.SetActive(true);

        skipRaceText.text = nextStageNumber == 1 ? skipToFirstStageTextString : string.Format(skipToNextStageTextFormat, nextStageNumber);
    }

    public void HidePauseMenu()
    {
        gameObject.SetActive(false);
    }

    // Button callbacks
    
    public void OnResumeButtonClick()
    {
        UISFXPlayer.Instance.PlayConfirmSFX();
        HidePauseMenu();
        RaceManager.Instance.ResumeRace();
    }

    public void OnRetryButtonClick()
    {
        UISFXPlayer.Instance.PlayConfirmSFX();
        HidePauseMenu();
        RaceManager.Instance.ResumeRace();
        RaceManager.Instance.RestartRace();
    }

    public void OnSkipButtonClick()
    {
        UISFXPlayer.Instance.PlayConfirmSFX();

        // Skip really acts like a Next, except we haven't finished the race
        RaceManager.Instance.StartNextRace();
    }

    public void OnExitButtonClick()
    {
        UISFXPlayer.Instance.PlayConfirmSFX();
        RaceManager.Instance.GoBackToTitle();
    }
}
