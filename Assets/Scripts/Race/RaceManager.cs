//#define DEBUG_RACE_MANAGER
//#define SET_FLAG_ON_START

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;
using CommonsPattern;
using UnityConstants;
using UnityEngine.SceneManagement;

public class RaceManager : SingletonManager<RaceManager>
{
    /* External references */
    
    [Tooltip("Characters parent")]
    public Transform charactersParent;
    
    [Tooltip("Spawn point parent")]
    public Transform spawnPointParent;
    
    /* Cached external references */
    private InGameCamera m_InGameCamera;
    
    
    /* Parameters */

    [SerializeField, Tooltip("Total number of stages available for racing")]
    private int stageCount = 2;
    
    
    /* State vars */
    
    /// Current state
    private RaceState m_State = RaceState.WaitForStart;
    public RaceState State => m_State;

    /// Current stage index (also index of scene)
    private int m_CurrentStageIndex;
    
#if SET_FLAG_ON_START
    /// Flag transform (could be set as external reference, but found with tag at runtime)
    private Transform m_FlagTr;
#endif
    
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
        // usually we'd have RaceManager to be DontDestroyOnLoad to preserve the stage index and increment it over races
        // but we currently use a self-contained scene system with no DontDestroyOnLoad, so on new race start,
        // a new RaceManager appears and auto-detects the stage index from the build index of the current scene
        m_CurrentStageIndex = SceneManager.GetActiveScene().buildIndex;
            
        m_InGameCamera = Camera.main.GetComponentOrFail<InGameCamera>();
#if SET_FLAG_ON_START
        m_FlagTr = GameObject.FindWithTag(Tags.Flag).transform;
#endif
        RegisterRunners();
    }

    private void Start()
    {
        SetupRace();
    }

    private void SetupRace()
    {        
        GateManager.Instance.SetupGates();
        DeviceManager.Instance.SetupDevices();
        
        foreach (var characterRun in m_Runners)
        {
            characterRun.Setup();
#if SET_FLAG_ON_START
            characterRun.GetComponentOrFail<FlagBearer>().Setup();
#endif
        }
        
#if SET_FLAG_ON_START
        int firstRunnerIndex = RandomlySortStartPositions();
        GiveFlagToRunner(firstRunnerIndex);
#else
        SpawnRunners();
#endif

        // setup camera after spawning runners to target their initial position (including on Restart)
        m_InGameCamera.Setup();
        
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

    private void SpawnRunners()
    {
        // move character with flag ahead
        m_Runners[0].transform.position = spawnPointParent.GetChild(0).position;
        
        // move character without flag behind
        m_Runners[1].transform.position = spawnPointParent.GetChild(1).position;
    }

#if SET_FLAG_ON_START
    private int RandomlySortStartPositions()
    {
        // we get a 0-based index here (number - 1), but it's convenient to get object by index so we keep it
        int randomRunnerIndex = Random.Range(0, m_Runners.Count);
        CharacterRun randomRunner = m_Runners[randomRunnerIndex];

        // move character with flag ahead
        randomRunner.transform.position = spawnPointFlag.position;
        
        // move character without flag behind
        m_Runners[1 - randomRunnerIndex].transform.position = spawnPointNoFlag.position;

        return randomRunnerIndex;
    }

    private void GiveFlagToRunner(int index)
    {
        FlagBearer flagBearer = m_Runners[index].GetComponentOrFail<FlagBearer>();
        flagBearer.BearFlag(m_FlagTr);
    }
#endif
    
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
        ResultUI.Instance.ShowResult(m_RankedRunnerNumbers[0], m_CurrentStageIndex >= stageCount - 1);
    }

    /// Callback for ResultUI Retry Button
    public void RestartRace()
    {
        ResultUI.Instance.Hide();

        SetupRace();
    }
    
    /// Callback for ResultUI Next Button
    public void StartNextRace()
    {
        // if at last stage, restart from stage 1
        int nextStageIndex = (m_CurrentStageIndex + 1) % stageCount;
        SceneManager.LoadScene(nextStageIndex);
    }
    
#if DEBUG_RACE_MANAGER && (UNITY_EDITOR || DEVELOPMENT_BUILD)
    private static readonly GUIStyle GuiStyle = new GUIStyle();

    private void OnGUI()
    {
        GuiStyle.fontSize = 48;
        GuiStyle.normal = new GUIStyleState {textColor = Color.white};
        GUILayout.Label($"RaceManager state: {m_State}", GuiStyle);
    }
#endif
}
