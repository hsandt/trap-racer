using CommonsHelper;
using CommonsPattern;
using UnityEngine;

public class TitleUI : SingletonManager<TitleUI>
{
    /* Child references */
    
    [Tooltip("StageSelectUI root script")]
    public StageSelectUI stageSelectUI;

    private void Start()
    {
        stageSelectUI.Deactivate();
    }

    public void ShowStageSelectUI()
    {
        stageSelectUI.Show();
    }
   
#if !UNITY_WEBGL
    public void ExitGame()
    {
        Application.Quit();
    }
#endif
}
