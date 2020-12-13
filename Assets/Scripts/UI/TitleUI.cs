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
        stageSelectUI.Hide();
    }

    public void ShowStageSelectUI()
    {
        stageSelectUI.Show();
    }
}
