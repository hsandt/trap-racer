using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayStageButton : MonoBehaviour
{
    [SerializeField, Tooltip("Stage number (1 or 2)")]
    private int stageNumber = 1;
    
    /// Callback for click action
    public void OnClick()
    {
        UISFXPlayer.Instance.PlayConfirmSFX();
        StageSelectUI.Instance.LoadStage(stageNumber);
    }
}
