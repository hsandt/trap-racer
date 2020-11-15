#define DEBUG_LOBBY_UI

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommonsHelper;
using UnityEngine;

using CommonsPattern;
using UnityEngine.InputSystem;

public class LobbyUI : SingletonManager<LobbyUI>
{
    [SerializeField, Tooltip("Action needed to virtually join with keyboard for P1 " +
                             "(no player is added, we count on PlayerInputKeyboard for keyboard input handling). " +
                             "Keyboard equivalent of Join Action in PlayerManager, which is for Gamepad only.")]
    private InputAction keyboardJoinActionP1 = new InputAction();
    
    [SerializeField, Tooltip("Same as above, but for P2 (array of InputActions has a bug that clones entries so we " +
                             "don't use one).")]
    private InputAction keyboardJoinActionP2 = new InputAction();
    
    [Tooltip("Player join text game object array, by player index")]
    public GameObject[] playerJoinTextObjects;
    
    [Tooltip("Player joined with keyboard text game object array, by player index")]
    public GameObject[] playerJoinedWithKeyboardTextObjects;
    
    [Tooltip("Player joined with gamepad text game object array, by player index")]
    public GameObject[] playerJoinedWithGamepadTextObjects;
    
    /* State */

    /// Array of flags indicating if player has joined, by player index
    private readonly bool[] hasPlayerJoinedArray = {false, false};

    protected override void Init()
    {
        keyboardJoinActionP1.performed += OnKeyboardJoinActionPerformedP1;
        keyboardJoinActionP2.performed += OnKeyboardJoinActionPerformedP2;

        for (int playerIndex = 0; playerIndex < 2; playerIndex++)
        {
            playerJoinTextObjects[playerIndex].SetActive(true);
            playerJoinedWithKeyboardTextObjects[playerIndex].SetActive(false);
            playerJoinedWithGamepadTextObjects[playerIndex].SetActive(false);
        }
    }

    private void OnEnable()
    {
        keyboardJoinActionP1.Enable();
        keyboardJoinActionP2.Enable();
    }

    private void OnDisable()
    {
        keyboardJoinActionP1.Disable();
        keyboardJoinActionP2.Disable();
    }

    private void OnKeyboardJoinActionPerformedP1(InputAction.CallbackContext ctx)
    {
        if (!hasPlayerJoinedArray[0])
        {
            ConfirmPlayerJoinedWithKeyboardHalf(0);
        }
    }

    private void OnKeyboardJoinActionPerformedP2(InputAction.CallbackContext ctx)
    {
        if (!hasPlayerJoinedArray[1])
        {
            ConfirmPlayerJoinedWithKeyboardHalf(1);
        }
    }

    private void ConfirmPlayerJoinedWithKeyboardHalf(int playerIndex)
    {
#if DEBUG_LOBBY_UI
        Debug.LogFormat("Player with index {0} joined.", playerIndex);
#endif
        
        playerJoinTextObjects[playerIndex].SetActive(false);
        playerJoinedWithKeyboardTextObjects[playerIndex].SetActive(true);

        hasPlayerJoinedArray[playerIndex] = true;
        CheckAllPlayersJoined();
    }
    
    /// Register player joining with gamepad to first available slot if possible,
    /// assigning player index to playerInputGamepad (-1 if none available)
    public void ConfirmPlayerJoinedWithGamepad(PlayerInputGamepad playerInputGamepad)
    {
        // determine index of player this gamepad will be associated to,
        // by checking for the first available slot
        int newPlayerIndex;
        
        if (!hasPlayerJoinedArray[0])
        {
            newPlayerIndex = 0;
        }
        else if (!hasPlayerJoinedArray[1])
        {
            newPlayerIndex = 1;
        }
        else
        {
            // both players have joined with keyboard already, ignore PlayerInputGamepad (don't bind it)
#if DEBUG_LOBBY_UI
            Debug.LogFormat(playerInputGamepad, "Both players already joined, ignoring new PlayerInputGamepad instance {0}.", playerInputGamepad);
#endif
            return;
        }
        
#if DEBUG_LOBBY_UI
        Debug.LogFormat(playerInputGamepad, "PlayerInputGamepad instance {0} joins, bound to player index {1}.", playerInputGamepad, newPlayerIndex);
#endif

        playerInputGamepad.PlayerIndex = newPlayerIndex;

        playerJoinTextObjects[newPlayerIndex].SetActive(false);
        playerJoinedWithGamepadTextObjects[newPlayerIndex].SetActive(true);
        
        hasPlayerJoinedArray[newPlayerIndex] = true;
        CheckAllPlayersJoined();
    }

    private void CheckAllPlayersJoined()
    {
        if (hasPlayerJoinedArray.All(value => value))
        {
            TitleUI.Instance.ShowStageSelectUI();
        }
    }
}
