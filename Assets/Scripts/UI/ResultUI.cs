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
    
    [SerializeField, Tooltip("Next race button text when there are next stages left.")]
    private string nextRaceTextString = "Next race";
    
    [SerializeField, Tooltip("Next race button text when there are no stages left so we restart from stage 1 (index 0).")]
    private string firstRaceTextString = "Restart from stage 1";
    
    
    /* Child references */
    
    [Tooltip("Victory text component")]
    public TextMeshProUGUI victoryText;
    
    [Tooltip("Next race button text component")]
    public TextMeshProUGUI nextRaceText;
    
    
    /* Sibling components */

    private Canvas m_Canvas;

    
    protected override void Init()
    {
        m_Canvas = this.GetComponentOrFail<Canvas>();

        // hide until needed (done on Awake to avoid re-hiding after race start on Start)
        m_Canvas.enabled = false;
    }
    
    public void ShowResult(int winnerNumber, bool wasLastRace)
    {
        m_Canvas.enabled = true;
        victoryText.text = string.Format(victoryTextFormat, winnerNumber);
        nextRaceText.text = string.Format(wasLastRace ? firstRaceTextString : nextRaceTextString, winnerNumber);
    }
    
    public void Hide()
    {
        m_Canvas.enabled = false;
    }
}
