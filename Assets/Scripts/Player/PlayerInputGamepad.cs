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

    /// Runner controlled by this player
    private CharacterRun runner;
    
    private void Start()
    {
        // PlayerInputKeyboard is always present and count as player index/number 0
        // so at this point, playerCount is already 1 + this runner index + 1 = this runner number + 1
        // This also means that the max player count on Player Input Manager should be the max runner count + 1
        int playerNumber = PlayerInputManager.instance.playerCount - 1;
        int runnerIndex = playerNumber - 1;
        Debug.AssertFormat(runnerIndex >= 0, "runnerIndex is {0} make sure PlayerInputKeyboard is active on start to count as player number 0", runnerIndex);
        Debug.LogFormat("Player #{0} joined, associating PlayerInputGamepad to runner of index {1}", playerNumber, runnerIndex);
        
        runner = RaceManager.Instance.GetRunner(runnerIndex);
    }

    /// Input callback: Jump action
    private void OnJump()
    {
        runner.OnJump();
    }

    /// Input callback: Move action
    private void OnMoveXY(InputValue value)
    {
        runner.OnMove(value.Get<Vector2>().x);
    }
}
