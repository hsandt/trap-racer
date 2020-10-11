using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;

public class Goal : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other)
    {
        var characterRun = other.collider.GetComponentOrFail<CharacterRun>();
        characterRun.PlayFinishAnim();

        RaceManager.Instance.NotifyRunnerFinished(characterRun);
    }
}
