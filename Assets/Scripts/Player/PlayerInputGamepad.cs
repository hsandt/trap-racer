using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// This script is attached to actual Player prefab instances who joined the game via Player Input Manager.
/// Unlike PlayerInputKeyboard, there is one per player since gamepads are individual,
/// and there is no sharing and input splitting, only input redirection to character components.
public class PlayerInputGamepad : MonoBehaviour
{
    /* Parameters initialized on start */

    /// Index of player associated to this gamepad control
    /// Used to retrieve runner to control
    /// Since this is initialized in the titlemenu where there is no PlayerInputKeyboard, 1st player has index 0
    private int m_PlayerIndex;
    public int PlayerIndex
    {
        get => m_PlayerIndex;
        set => m_PlayerIndex = value;
    }
    
    /* References set on Race Setup */
    
    /// Runner controlled by this player
    private CharacterRun runner;

    /// Index of player associated to this gamepad control
    /// Used to retrieve runner to control
    /// Since this is initialized in the titlemenu where there is no PlayerInputKeyboard, 1st player has index 0

    private void Start()
    {
        if (LobbyUI.Instance)
        {
            // will assign m_PlayerIndex to first available player index
            LobbyUI.Instance.ConfirmPlayerJoinedWithGamepad(this);
        }
        else
        {
            // in the Editor only, we have a last minute PlayerManager_StageLiveJoin_EditorOnly in the stage scenes
            // to make it easier to test with gamepad when directly running game from stage (or if we forgot to bind
            // gamepad in Title)
            // this time we have the PlayerInputKeyboardCommon to consider, so subtract 2 from new player input count
            m_PlayerIndex = PlayerInputManager.instance.playerCount - 2;
            
            // Race must have already started, so don't rely on RaceManager and SetupControl yourself
            SetupControl();
        }
    }

    public void SetupControl()
    {
        runner = RaceManager.Instance.GetRunner(m_PlayerIndex);
    }
    
    // Input callbacks below will be called even in the title menu, so check that runner exists
    // so we don't crash

    /// Input callback: Jump action
    private void OnJump(InputValue value)
    {
        if (runner != null)
        {
            if (value.isPressed)
            {
                runner.OnJump();
            }
            else
            {
                runner.OnJumpReleased();
            }
        }
    }

    /// Input callback: Move action
    private void OnMoveXY(InputValue value)
    {
        if (runner != null)
        {
            runner.OnMove(value.Get<Vector2>().x);
        }
    }
}
