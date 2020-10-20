using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    
    private void Start()
    {
        runner1 = RaceManager.Instance.GetRunner(0);
        runner2 = RaceManager.Instance.GetRunner(1);
    }

    /// Input callback: Jump action for P1
    private void OnJumpP1()
    {
        runner1.OnJump();
    }
    
    /// Input callback: Jump action for P2
    private void OnJumpP2()
    {
        runner2.OnJump();
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
}
