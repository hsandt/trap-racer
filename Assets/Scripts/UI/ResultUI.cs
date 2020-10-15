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
    
    /* Child references */
    
    [Tooltip("Victory text component")]
    public TextMeshProUGUI victoryText;
    
    /* Sibling components */

    private Canvas m_Canvas;

    protected override void Init()
    {
        m_Canvas = this.GetComponentOrFail<Canvas>();

        // hide until needed (done on Awake to avoid re-hiding after race start on Start)
        m_Canvas.enabled = false;
    }
    
    public void ShowResult(int winnerNumber)
    {
        m_Canvas.enabled = true;
        victoryText.text = string.Format(victoryTextFormat, winnerNumber);
    }
}
