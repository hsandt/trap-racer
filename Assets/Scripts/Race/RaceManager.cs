#define DEBUG_RACE_MANAGER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsPattern;

public class RaceManager : SingletonManager<RaceManager>
{
    /* State vars */
    
    /// List of finishing runner numbers, from 1st to last
    private List<int> rankedRunnerNumbers = new List<int>();

    private void Start()
    {
        // hide result UI for now
        ResultUI.Instance.gameObject.SetActive(false);
    }
    
    public void NotifyRunnerFinished(CharacterRun characterRun)
    {
        rankedRunnerNumbers.Add(characterRun.PlayerNumber);
        
        // if this was the last runner in the race, finish the race now
        // when adding Character/RunnerManager, replace hardcoded 2 with RunnerManager.GetRunnerCount()
        if (rankedRunnerNumbers.Count >= 2)
        {
            FinishRace();
        }
    }

    private void FinishRace()
    {
        ComputeRaceResult();
        ShowResultPanel();
    }

    private void ComputeRaceResult()
    {
#if DEBUG_RACE_MANAGER
        Debug.LogFormat(this, "[RaceManager] Compute Race Result");
#endif
        // nothing to do for now, rankedRunnerNumbers says it all
    }
    
    private void ShowResultPanel()
    {
#if DEBUG_RACE_MANAGER
        Debug.LogFormat(this, "[RaceManager] Show Result Panel");
#endif
        Debug.LogFormat("Winner: Player #{0}", rankedRunnerNumbers[0]);
        ResultUI.Instance.gameObject.SetActive(true);
        ResultUI.Instance.ShowResult(rankedRunnerNumbers[0]);
    }
}
