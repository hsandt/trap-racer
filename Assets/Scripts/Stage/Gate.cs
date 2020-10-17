#define DEBUG_GATE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;

// SEO: after GateManager
public class Gate : MonoBehaviour
{
    /* Sibling components */

    private Collider2D m_Collider2D;
    private SpriteRenderer m_SpriteRenderer;


    /* Parameters */

    [SerializeField, Tooltip("Color of gate and switches that toggle it")]
    private GameColor color = GameColor.Purple;
    public GameColor Color => color;
    
    
    /// Is the gate closed?
    private bool m_Closed = true;

    private void Awake()
    {
        m_Collider2D = this.GetComponentOrFail<Collider2D>();
        m_SpriteRenderer = this.GetComponentOrFail<SpriteRenderer>();
    }

    private void Start()
    {
        // must be done after GateManager.Awake/Init
        GateManager.Instance.RegisterGate(this);
    }

    /// Managed setup
    /// Not called on own Start, must be called in RaceManager.SetupRace > GateManager.SetupGates
    public void Setup()
    {
        // We assume collider and sprite renderer are enabled on Start
        // And only reenable them if the gate has been opened and we are restarting
        // This is only to avoid enabling them twice, once here and once in Obstacle
        if (!m_Closed)
        {
            m_Closed = true;
            m_Collider2D.enabled = true;
            m_SpriteRenderer.enabled = true;
        }
    }

    private void OnDestroy()
    {
        // when stopping the game, GateManager may have been destroyed first so check it
        if (GateManager.Instance)
        {
            GateManager.Instance.UnregisterGate(this);
        }
    }

    public void Toggle()
    {
        m_Closed ^= true;
        m_Collider2D.enabled = m_Closed;
        m_SpriteRenderer.enabled = m_Closed;
    }
}
