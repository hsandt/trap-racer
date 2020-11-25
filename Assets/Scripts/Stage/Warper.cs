using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;

public class Warper : Device
{
    /* External references */
    
    [Tooltip("Target warper")]
    public Transform targetWarperTr;
    
    
    /* Children references */

    [Tooltip("Particle system played continuously")]
    public ParticleSystem pfx;   
    
    
    /* State */
    
    /// Is the warp active? It cannot be used twice.
    private bool m_Active;
    
    
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
        SetActive(true);
    }
    
    public override void Pause()
    {
    }

    public override void Resume()
    {
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (m_Active)
        {
            SetActive(false);
            StartTriggerAnimation();
            
            var characterRun = other.GetComponentOrFail<CharacterRun>();
            characterRun.WarpTo(targetWarperTr);
        }
    }

    private void SetActive(bool value)
    {
        m_Active = value;
        
        // pfx indicating warper can be used
        // make sure to check for null as emission property will create an Emission Module
        // bound to null otherwise, causing "Do not create your own module instances,
        // get them from a ParticleSystem instance"
        Debug.AssertFormat(pfx != null, this, "PFX not set on Warper {0}", this);
        if (pfx != null)
        {
            var emissionModule = pfx.emission;
            emissionModule.enabled = value;
        }
    }

    private void StartTriggerAnimation()
    {
        // TODO
    }
}
