using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsPattern;

public class LobbyUI : SingletonManager<LobbyUI>
{
    [Tooltip("Player join text game object array, by player index")]
    public GameObject[] playerJoinTextObjects;
    
    [Tooltip("Player joined with keyboard text game object array, by player index")]
    public GameObject[] playerJoinedWithKeyboardTextObjects;
    
    [Tooltip("Player joined with gamepad text game object array, by player index")]
    public GameObject[] playerJoinedWithGamepadTextObjects;

    protected override void Init()
    {
        for (int playerIndex = 0; playerIndex < 2; playerIndex++)
        {
            playerJoinedWithKeyboardTextObjects[playerIndex].SetActive(false);
            playerJoinedWithGamepadTextObjects[playerIndex].SetActive(false);
        }
    }
    
    public void ConfirmPlayerJoined(int playerIndex)
    {
        playerJoinTextObjects[playerIndex].SetActive(false);
        playerJoinedWithGamepadTextObjects[playerIndex].SetActive(true);
    }
}
