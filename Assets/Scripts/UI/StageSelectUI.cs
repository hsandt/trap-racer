using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

using CommonsPattern;

public class StageSelectUI : SingletonManager<StageSelectUI>
{
    [Tooltip("First button to select")]
    public GameObject firstSelected;
    
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }
    
    /// Load stage by number (1 or 2)
    /// Callback for Play stage X buttons
    public void LoadStage(int stageNumber)
    {
        SceneManager.LoadScene(stageNumber);
    }
}
