using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitButton : MonoBehaviour
{
    /// Callback for click action
    public void OnClick()
    {
#if !UNITY_WEBGL
        TitleUI.Instance.ExitGame();
#endif
    }
}
