using System.Collections;
using System.Collections.Generic;
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

    protected override void Init()
    {
        keyboardJoinActionP1.performed += OnKeyboardJoinActionPerformedP1;
        keyboardJoinActionP2.performed += OnKeyboardJoinActionPerformedP2;

        for (int playerIndex = 0; playerIndex < 2; playerIndex++)
        {
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
        ConfirmPlayerJoinedWithKeyboard(0);
    }

    private void OnKeyboardJoinActionPerformedP2(InputAction.CallbackContext ctx)
    {
        ConfirmPlayerJoinedWithKeyboard(1);
    }

    private void ConfirmPlayerJoinedWithKeyboard(int playerIndex)
    {
        playerJoinTextObjects[playerIndex].SetActive(false);
        playerJoinedWithKeyboardTextObjects[playerIndex].SetActive(true);
    }
    
    public void ConfirmPlayerJoinedWithGamepad(int playerIndex)
    {
        playerJoinTextObjects[playerIndex].SetActive(false);
        playerJoinedWithGamepadTextObjects[playerIndex].SetActive(true);
    }
}
