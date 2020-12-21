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
    public ParticleSystem activePfx;   
    
    [Tooltip("Particle system played when warping")]
    public ParticleSystem warpPfx;   
    
    
    /* State */
    
    /// Is the warp active? It cannot be used twice.
    private bool m_Active;
    
    /// Flag to remember we should deactivate this warper at the end of the turn
    /// It allows us to keep it active until all collisions have been processed
    /// in case the 2 characters touch it at the same time
    private bool m_ShouldDeactivateOnNextFixedUpdate;
    
    
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
        m_ShouldDeactivateOnNextFixedUpdate = false;
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
            m_ShouldDeactivateOnNextFixedUpdate = true;
            
            StartTriggerAnimation();
            
            var characterRun = other.GetComponentOrFail<CharacterRun>();
            characterRun.WarpTo(targetWarperTr);
        }
    }

    private void FixedUpdate()
    {
        if (m_ShouldDeactivateOnNextFixedUpdate)
        {
            m_ShouldDeactivateOnNextFixedUpdate = false;
            SetActive(false);
        }
    }

    private void SetActive(bool value)
    {
        m_Active = value;
        
        // pfx indicating warper can be used
        // make sure to check for null as emission property will create an Emission Module
        // bound to null otherwise, causing "Do not create your own module instances,
        // get them from a ParticleSystem instance"
        Debug.AssertFormat(activePfx != null, this, "PFX not set on Warper {0}", this);
        if (activePfx != null)
        {
            var emissionModule = activePfx.emission;
            emissionModule.enabled = value;
        }
    }
    
    private void StartTriggerAnimation()
    {
        warpPfx.Play();
    }
}
