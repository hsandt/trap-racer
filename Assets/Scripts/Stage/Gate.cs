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
        m_Collider2D.enabled ^= true;
        m_SpriteRenderer.enabled ^= true;
    }
}
