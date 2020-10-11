using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using CommonsPattern;

public class ResultUI : SingletonManager<ResultUI>
{
    /* Strings */
    private static readonly string victoryTextFormat = "Player {0} wins!";
    
    /* Child references */
    [Tooltip("Victory text component")]
    public TextMeshProUGUI victoryText;
    
    void Start()
    {
        
    }

    public void ShowResult(int winnerNumber)
    {
        victoryText.text = string.Format(victoryTextFormat, winnerNumber);
    }
}
