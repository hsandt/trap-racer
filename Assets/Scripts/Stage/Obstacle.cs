using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;

public class Obstacle : Device
{
    /* Sibling components */
    private Collider2D m_Collider2D;
    private Renderer m_Renderer;
    
    /// Is the obstacle active? (set to false when hits a runner, only reset on race restart)
    private bool m_Active = true;

    private void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        m_Collider2D = this.GetComponentOrFail<Collider2D>();
        m_Renderer = GetComponentInChildren<Renderer>();
    }
    
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
    
    /// Managed setup
    /// Not called on own Start, must be called in RaceManager.SetupRace > DeviceManager.SetupObstacles
    public override void Setup()
    {
        // We assume collider and sprite renderer are enabled on Start
        // And only reenable them if the gate has been opened and we are restarting
        // This is only to avoid enabling them twice, once here and once in Gate
        if (!m_Active)
        {
            m_Active = true;
            m_Collider2D.enabled = true;
            m_Renderer.enabled = true;
        }
    }
    
    public override void Pause()
    {
        // nothing needs to be done, assuming nothing is moving during pause so collision can be triggered
    }

    public override void Resume()
    {
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // currently we don't check for m_Active, counting on collider to be disabled to avoid this obstacle
        //   to work twice
        // this has an interesting effect: if two runners crash into the obstacle at the exact same frame
        //   (possible since they start at the same X and may run in sync for a moment), BOTH runners are slowed down
        //   as this will be called twice, but the second time it will already be inactive (but the collisions have
        //   already been solved for this frame, so too late to disable collider)
        // this is fairer as only slowing down one runner (which is undeterministic as we don't know in which order
        //  this callback is called for other colliders), so we kept it
        // an alternative is to keep the collision active for a short time, which is more likely to happen and be noticed
        //  by the player as the exact same frame
        var characterRun = other.collider.GetComponentOrFail<CharacterRun>();
        characterRun.CrashIntoObstacle();

        // for tracking and restart purpose
        m_Active = false;
        
        // disable collider
        m_Collider2D.enabled = false;

        // no animation yet, for prototype just hide the obstacle
        m_Renderer.enabled = false;
    }
}
