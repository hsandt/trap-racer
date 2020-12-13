using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerTime : MonoBehaviour
{
    /* Strings */

    [SerializeField, Tooltip("Skip race button string format when there are next stages left. {0} will be replaced by stage number.")]
    private string timeTextFormat = "{0:0.00}s";


    /* Child references */

    [Tooltip("Time text component")]
    public TextMeshProUGUI timeText;

    public void SetTime(float time)
    {
        timeText.text = string.Format(timeTextFormat, time);
    }
}
