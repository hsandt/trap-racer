﻿//#define DEBUG_RACE_MANAGER
//#define SET_FLAG_ON_START

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using CommonsHelper;
using CommonsPattern;
using UnityConstants;

public class RaceManager : SingletonManager<RaceManager>
{
    public struct FinishInfo
    {
        /// Player number (1 or 2).
        public int playerNumber;

        /// Finish time (s)
        public float time;
    }

    
    /* External references */

    [Tooltip("Characters parent")] public Transform charactersParent;

    [Tooltip("Spawn point parent")] public Transform spawnPointParent;

    /* Cached external references */
    private InGameCamera m_InGameCamera;

    private ParticleSystem m_ConfettiParticleSystem;


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

    /// Current time since race start
    private float m_RaceTime;

    /// List of runner numbers, from 1st to last
    private readonly List<CharacterRun> m_Runners = new List<CharacterRun>();

    /// Array of animation scripts that need to be restarted (typically stuff that must sync with other objects' motion)
    private FixedUpdateAnimationScript[] m_FixedUpdateAnimationScripts = null;
    
    /// List of finishing runner info, from 1st to last
    private readonly List<FinishInfo> m_FinishInfoList = new List<FinishInfo>();

    /// Winning player number, 0 if draw
    private int winnerNumber;

    
    public CharacterRun GetRunner(int index)
    {
        return m_Runners[index];
    }

    protected override void Init()
    {
        // Usually we'd have RaceManager to be DontDestroyOnLoad to preserve the stage index and increment it over races
        // but we currently use a self-contained scene system with no DontDestroyOnLoad (except PlayerInputGamepad),
        // so on new race start, a new RaceManager appears and auto-detects the stage index from the build index of the
        // current scene. Title scene is 0 and Stage scenes start at index 1, so subtract 1.
        m_CurrentStageIndex = SceneManager.GetActiveScene().buildIndex - 1;

        m_InGameCamera = Camera.main.GetComponentOrFail<InGameCamera>();
        m_ConfettiParticleSystem = GameObject.FindWithTag(Tags.Confetti).GetComponentOrFail<ParticleSystem>();

#if SET_FLAG_ON_START
        m_FlagTr = GameObject.FindWithTag(Tags.Flag).transform;
#endif
        RegisterRunners();
        RegisterScriptsToRestart();

        // If going from the title menu, some DontDestroyOnLoad PlayerInputGamepad have been preserved from the lobby,
        // and they know the player they should be bound to. Bind control to the matching runner now.
        SetupPlayerInputGamepadControls();
    }

    private void Start()
    {
        SetupRace();
    }

    private void FixedUpdate()
    {
        if (m_State == RaceState.Started)
        {
            m_RaceTime += Time.deltaTime;
        }
    }
    
    private void SetupRace()
    {
        StartCoroutine(SetupAsync());
    }

    private IEnumerator SetupAsync()
    {
        SetupInstant();
        // wait 1 frame so devices are all registered
        yield return null;
        SetupDelayed();
    }

    private void SetupInstant()
    {
        m_State = RaceState.WaitForStart;
        m_RaceTime = 0f;
        winnerNumber = 0;
        
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

        foreach (var animationScript in m_FixedUpdateAnimationScripts)
        {
            animationScript.Setup();
        }

        // setup camera after spawning runners to target their initial position (including on Restart)
        m_InGameCamera.Setup();
    }

    private void SetupDelayed()
    {
        GateManager.Instance.SetupGates();
        DeviceManager.Instance.SetupDevices();
        
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

    /// Setup control for all player input gamepads (must be done after RegisterRunners)
    private void SetupPlayerInputGamepadControls()
    {
        foreach (var playerInputGamepad in FindObjectsOfType<PlayerInputGamepad>())
        {
            playerInputGamepad.SetupControl();
        }
    }

    private void RegisterScriptsToRestart()
    {
        m_FixedUpdateAnimationScripts = FindObjectsOfType<FixedUpdateAnimationScript>();
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
        // m_RaceTime should have been set to 0f on SetupRace

        foreach (var characterRun in m_Runners)
        {
            characterRun.StartRunning();
        }
    }

    public void NotifyRunnerFinished(CharacterRun characterRun)
    {
        // Build finish info (rank is not part of it, the index in the list gives it)
        FinishInfo finishInfo;
        finishInfo.playerNumber = characterRun.PlayerNumber;
        finishInfo.time = m_RaceTime;

        if (m_FinishInfoList.Exists(info => info.playerNumber == finishInfo.playerNumber))
        {
            Debug.LogWarningFormat(characterRun, "Runner #{0} already finished, cannot finish again", finishInfo.playerNumber);
            return;
        }

        m_FinishInfoList.Add(finishInfo);

        // winner (1st finisher) gets the confetti animation
        if (m_FinishInfoList.Count == 1)
        {
            PlayConfettiAnimation();
        }

        // if this was the last runner in the race, finish the race now
        // when adding Character/RunnerManager, replace hardcoded 2 with RunnerManager.GetRunnerCount()
        if (m_FinishInfoList.Count >= m_Runners.Count)
        {
            FinishRace();
        }
    }

    private void FinishRace()
    {
        m_State = RaceState.Finished;
        ComputeRaceResult();
        StartCoroutine(PlayFinishSequenceAsync());
    }

    private void ComputeRaceResult()
    {
#if DEBUG_RACE_MANAGER
        Debug.LogFormat(this, "[RaceManager] Compute Race Result");
#endif
        // we know that finish time are sorted ascending, but it's possible both players
        // finished the same frame. In this case we don't bother interpolating the exact winner between frames,
        // we just declare draw
        if (m_FinishInfoList[0].time < m_FinishInfoList[1].time)
        {
            winnerNumber = m_FinishInfoList[0].playerNumber;
        }
        else
        {
            winnerNumber = 0;
        }
    }

    private IEnumerator PlayFinishSequenceAsync()
    {
        yield return new WaitForSeconds(1f);
        ShowResultPanel();
    }

    private void PlayConfettiAnimation()
    {
        m_ConfettiParticleSystem.Play();
    }

    private void ShowResultPanel()
    {
#if DEBUG_RACE_MANAGER
        Debug.LogFormat(this, "[RaceManager] Show Result Panel");
#endif
        // if winnerNumber == 0, it's a draw
        Debug.LogFormat("Winner: Player #{0}", winnerNumber);
        ResultUI.Instance.ShowResult(winnerNumber, m_CurrentStageIndex >= stageCount - 1);
    }

    /// Callback for ResultUI Retry Button
    public void RestartRace()
    {
        // important to completely deactivate the Result UI canvas to prevent using it even when hidden
        // (if only disabling canvas, we can still press the last selection with gamepad Confirm input to Restart again!)
        ResultUI.Instance.Deactivate();

        // preserve m_CurrentStageIndex as a different scene has a different RaceManager
        
        // preserve m_Runners as runners remain in the scene
        
        // clear other lists
        m_FinishInfoList.Clear();
        
        SetupRace();
    }
    
    /// Callback for ResultUI Next Button
    public void StartNextRace()
    {
        // if at last stage, restart from stage 1 (add 1 since Stage 1 is at index 1)
        int nextStageIndex = (m_CurrentStageIndex + 1) % stageCount;
        SceneManager.LoadScene(nextStageIndex + 1);
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
