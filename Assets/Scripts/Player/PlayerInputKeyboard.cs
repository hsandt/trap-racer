using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using CommonsHelper;

/// This script is mainly a proxy between unified keyboard input (which contains both P1 and P2 input as separate
/// actions instead of separate schemes) and separate player controls
/// The New Input System doesn't allow device sharing so for the keyboard we need to
/// manually redirect inputs for P1 and P2 to the appropriate target
public class PlayerInputKeyboard : MonoBehaviour
{
    /* Cached external references */

    /// Runner controlled by player 1
    private CharacterRun runner1;
    
    /// Runner controlled by player 2
    private CharacterRun runner2;
    
#if UNITY_EDITOR
    /* Cached external references */
    private InGameCamera m_InGameCamera;

    private void Awake()
    {
        m_InGameCamera = Camera.main.GetComponentOrFail<InGameCamera>();
    }
#endif

    private void Start()
    {
        runner1 = RaceManager.Instance.GetRunner(0);
        runner2 = RaceManager.Instance.GetRunner(1);
    }

    /// Input callback: Jump action for P1
    private void OnJumpP1(InputValue value)
    {
        if (value.isPressed)
        {
            runner1.OnJump();
        }
        else
        {
            runner1.OnJumpReleased();
        }
    }
    
    /// Input callback: Jump action for P2
    private void OnJumpP2(InputValue value)
    {
        if (value.isPressed)
        {
            runner2.OnJump();
        }
        else
        {
            runner2.OnJumpReleased();
        }
    }
    
    /// Input callback: Move action for P1
    private void OnMoveP1(InputValue value)
    {
        runner1.OnMove(value.Get<float>());
    }
    
    /// Input callback: Move action for P2
    private void OnMoveP2(InputValue value)
    {
        runner2.OnMove(value.Get<float>());
    }
    
    /// Input callback: TogglePauseMenu action
    private void OnTogglePauseMenu(InputValue value)
    {
        RaceManager.Instance.OnTogglePauseMenu();
    }
    
#if UNITY_EDITOR
    /// Cheat Input callback: Move all characters by 10m backward
    private void OnCheatBackward10m(InputValue value)
    {
        // warp all character to Runner 1 pos X - 10m to avoid deepening gap between runners
        runner1.transform.position -= 10 * Vector3.right;
        runner2.transform.position = new Vector3(runner1.transform.position.x, runner2.transform.position.y, runner2.transform.position.z);
        
        // warp immediately backward, normal update will prevent going backward
        // if you add more stuff to setup, consider making WarpCameraToTargetPosition public and calling that instead
        m_InGameCamera.Setup();
    }
    
    /// Cheat Input callback: Move all characters by 10m forward
    private void OnCheatForward10m(InputValue value)
    {
        // same as above, but forward
        runner1.transform.position += 10 * Vector3.right;
        runner2.transform.position = new Vector3(runner1.transform.position.x, runner2.transform.position.y, runner2.transform.position.z);
    }
    
    /// Cheat Input callback: Restart race immediately
    private void OnCheatRestartRace(InputValue value)
    {
        RaceManager.Instance.RestartRace();
    }
#endif
}
