using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var characterRun = other.GetComponentOrFail<CharacterRun>();
        characterRun.FinishRace();

        RaceManager.Instance.NotifyRunnerFinished(characterRun);
    }
}
