using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using CommonsHelper;
using CommonsPattern;

public class MiniMapUI : SingletonManager<MiniMapUI>
{
    /* Child references */

    [Tooltip("Indicators parent")]
    public Transform indicatorParent;

    private RectTransform[] runnerIndicators;

    protected override void Init()
    {
        // cache runner indicator references from indicator parent
        runnerIndicators = new RectTransform[2];
        for (int i = 0; i < 2; i++)
        {
            runnerIndicators[i] = indicatorParent.GetChild(i).GetComponentOrFail<RectTransform>();
        }
    }

    private void Update()
    {
        UpdateIndicatorPositions();
    }

    public void UpdateIndicatorPositions()
    {
        for (int i = 0; i < 2; i++)
        {
            // move indicator for this runner by progression ratio
            float progress = RaceManager.Instance.GetRunnerProgress(i);
            runnerIndicators[i].anchorMin = new Vector2(progress, runnerIndicators[i].anchorMin.y);
            runnerIndicators[i].anchorMax = new Vector2(progress, runnerIndicators[i].anchorMax.y);
        }
    }
}
