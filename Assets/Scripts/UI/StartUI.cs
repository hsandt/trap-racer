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

    /* Parameters */
    [SerializeField, Tooltip("Value from which countdown starts (usually 3, set to 0 for fast dev iterations)")]
    private int countDownStartValue = 3;

    [SerializeField, Tooltip("Duration of Countdown 0 message (s)")]
    private float countDown0TextDuration = 1f;

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

    public void StartCountDown()
    {
        // need to be active to start coroutine (and to display te
        gameObject.SetActive(true);
        StartCoroutine(ShowCountDownAsync());
    }

    private IEnumerator ShowCountDownAsync()
    {
        for (int i = countDownStartValue; i >= 1; --i)
        {
            countDownText.text = string.Format(countDownTextFormat, i);
            UISFXPlayer.Instance.PlaySFX(UISFXPlayer.SFX.CountDown);
            yield return new WaitForSeconds(1f);
        }

        countDownText.text = countDown0TextFormat;

        RaceManager.Instance.NotifyCountDownOver();

        // still continue the coroutine, we need to hide the last text after some time
        yield return new WaitForSeconds(countDown0TextDuration);
        Deactivate();
    }
}
