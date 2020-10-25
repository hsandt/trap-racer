using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;

public class Trampoline : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var characterRun = other.GetComponentOrFail<CharacterRun>();
        characterRun.JumpWithTrampoline();
    }

    private void StartAnimation()
    {
        // TODO
    }
}
