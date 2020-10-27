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
        // must be done after ObstacleManager.Awake/Init
        DeviceManager.Instance.RegisterDevice(this);
    }
    
    /// Managed setup
    /// Not called on own Start, must be called in RaceManager.SetupRace > ObstacleManager.SetupObstacles
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
    
    private void OnDestroy()
    {
        // when stopping the game, ObstacleManager may have been destroyed first so check it
        if (ObstacleManager.Instance)
        {
            ObstacleManager.Instance.UnregisterObstacle(this);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
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
