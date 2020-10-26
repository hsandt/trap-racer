using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using CommonsHelper;
using CommonsPattern;

public class StartUI : SingletonManager<StartUI>
{
    /* Strings */

    [SerializeField, Tooltip("Count down string format. {0} will be replaced by 3, 2, 1.")]
    private string countDownTextFormat = "{0}";

    [SerializeField, Tooltip("Countdown 0 string format. Allows custom message instead of just '0'.")]
    private string countDown0TextFormat = "GO!";

    /* Child references */

    [Tooltip("Countdown text component")] public TextMeshProUGUI countDownText;

    /* Sibling components */

    private Canvas m_Canvas;

    /* Parameters */
    [SerializeField, Tooltip("Value from which countdown starts (usually 3, set to 0 for fast dev iterations)")]
    private int countDownStartValue = 3;

    [SerializeField, Tooltip("Duration of Countdown 0 message (s)")]
    private float countDown0TextDuration = 1f;

    protected override void Init()
    {
        m_Canvas = this.GetComponentOrFail<Canvas>();

        // hide until needed (done on Awake to avoid re-hiding after race start on Start)
        m_Canvas.enabled = false;
    }

    public void StartCountDown()
    {
        StartCoroutine(ShowCountDownAsync());
    }

    private IEnumerator ShowCountDownAsync()
    {
        m_Canvas.enabled = true;

        for (int i = countDownStartValue; i >= 1; --i)
        {
            countDownText.text = string.Format(countDownTextFormat, i);
            yield return new WaitForSeconds(1f);
        }

        countDownText.text = countDown0TextFormat;

        RaceManager.Instance.NotifyCountDownOver();

        // still continue the coroutine, we need to hide the last text after some time
        yield return new WaitForSeconds(countDown0TextDuration);
        m_Canvas.enabled = false;
    }
}
