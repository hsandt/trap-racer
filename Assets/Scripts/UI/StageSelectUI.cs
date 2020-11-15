using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using CommonsPattern;

public class StageSelectUI : SingletonManager<StageSelectUI>
{
    // Load stage by number (1 or 2)
    public void LoadStage(int stageNumber)
    {
        SceneManager.LoadScene(stageNumber);
    }
}
