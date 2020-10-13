#define DEBUG_RACE_MANAGER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;
using CommonsPattern;
using UnityConstants;

public class RaceManager : SingletonManager<RaceManager>
{
    /* External references */
    
    [Tooltip("Characters parent")]
    public Transform charactersParent;
    
    [Tooltip("Spawn Point for character with no flag (behind)")]
    public Transform spawnPointNoFlag;
    
    [Tooltip("Spawn Point for character with flag (in front)")]
    public Transform spawnPointFlag;
    
    /* State vars */
    
    /// Current state
    private RaceState m_State = RaceState.WaitForStart;

    /// Flag transform (could be set as external reference, but found with tag at runtime)
    private Transform m_FlagTr;
    
    /// List of finishing runner numbers, from 1st to last
    private readonly List<CharacterRun> m_Runners = new List<CharacterRun>();

    /// List of finishing runner numbers, from 1st to last
    private readonly List<int> m_RankedRunnerNumbers = new List<int>();

    public CharacterRun GetRunner(int index)
    {
        return m_Runners[index];
    }
    
    protected override void Init()
    {
        m_FlagTr = GameObject.FindWithTag(Tags.Flag).transform;
        RegisterRunners();
    }

    private void Start()
    {
        GiveFlagToRandomRunner();
        
        StartUI.Instance.StartCountDown();
    }

    private void RegisterRunners()
    {
        foreach (Transform characterTr in charactersParent)
        {
            var characterRun = characterTr.GetComponentOrFail<CharacterRun>();
            m_Runners.Add(characterRun);
        }
    }

    private void GiveFlagToRandomRunner()
    {
        // we get a 0-based index here (number - 1), but it's convenient to get object by index so we keep it
        int randomRunnerIndex = Random.Range(0, m_Runners.Count);
        CharacterRun randomRunner = m_Runners[randomRunnerIndex];
        FlagBearer randomFlagBearer = randomRunner.GetComponentOrFail<FlagBearer>();
        randomFlagBearer.BearFlag(m_FlagTr);

        // move character with flag ahead
        randomFlagBearer.transform.position = spawnPointFlag.position;
        
        // move character without flag behind
        m_Runners[1 - randomRunnerIndex].transform.position = spawnPointNoFlag.position;
    }
    
    public void NotifyCountDownOver()
    {
        StartRace();
    }

    private void StartRace()
    {
        m_State = RaceState.Started;
        
        foreach (var characterRun in m_Runners)
        {
            characterRun.StartRunning();
        }
    }
    
    public void NotifyRunnerFinished(CharacterRun characterRun)
    {
        m_RankedRunnerNumbers.Add(characterRun.PlayerNumber);
        
        // if this was the last runner in the race, finish the race now
        // when adding Character/RunnerManager, replace hardcoded 2 with RunnerManager.GetRunnerCount()
        if (m_RankedRunnerNumbers.Count >= m_Runners.Count)
        {
            FinishRace();
        }
    }

    private void FinishRace()
    {
        m_State = RaceState.Finished;

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
        Debug.LogFormat("Winner: Player #{0}", m_RankedRunnerNumbers[0]);
        ResultUI.Instance.ShowResult(m_RankedRunnerNumbers[0]);
    }
    
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void OnGUI()
    {
        GUIStyle guiStyle = new GUIStyle {fontSize = 48, normal = new GUIStyleState {textColor = Color.white}};
        GUILayout.Label(m_State.ToString(), guiStyle);
    }
#endif
}
