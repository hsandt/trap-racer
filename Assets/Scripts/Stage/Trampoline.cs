using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;

public class Trampoline : Device
{
    private void Start()
    {
        // must be done after DeviceManager.Awake/Init
        DeviceManager.Instance.RegisterDevice(this);
    }
    
    private void OnDestroy()
    {
        // when stopping the game, DeviceManager may have been destroyed first so check it
        if (DeviceManager.Instance)
        {
            DeviceManager.Instance.UnregisterDevice(this);
        }
    }
    
    public override void Setup()
    {
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        var characterRun = other.GetComponentOrFail<CharacterRun>();
        characterRun.JumpWithTrampoline();
        
        StartAnimation();
    }

    private void StartAnimation()
    {
        // TODO
    }
}
