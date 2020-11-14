using CommonsPattern;
using UnityEngine;

public class TitleUI : SingletonManager<TitleUI>
{
    [Tooltip("StageSelectUI root")]
    public GameObject stageSelectUI;

    private void Start()
    {
        stageSelectUI.SetActive(false);
    }

    public void ShowStageSelectUI()
    {
        stageSelectUI.SetActive(true);
    }
}
