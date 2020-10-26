using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpeningGround : MonoBehaviour
{
    /* Children components */
    
    [Tooltip("Rigidbody of the left panel")]
    public Rigidbody2D m_RigidbodyPanelLeft;
    
    [Tooltip("Rigidbody of the right panel")]
    public Rigidbody2D m_RigidbodyPanelRight;
    
    /* Parameters defined in inspector */
    
    [SerializeField, Tooltip("Width of a single panel, so it can open entirely (it will enter the surrounding ground)")]
    private float moveWidth = 2f;
    
    [SerializeField, Tooltip("Move cycle period. Take half to get duration between start of move phases (s)")]
    private float movePeriod = 4f;
    
    [SerializeField, Tooltip("Duration of the motion between open and closed position itself (s). Is part of Move Period.")]
    private float moveDuration = 0.5f;

    /* Parameters defined on start */
    
    /// Initial X position for left panel
    private float initialXLeft;
    
    /// Initial X position for right panel
    private float initialXRight;

    /* State */
    
    /// Time tracker (reduced by modulo)
    private float m_CurrentTimeModulo;

    protected void Awake()
    {
        initialXLeft = m_RigidbodyPanelLeft.position.x;
        initialXRight = m_RigidbodyPanelRight.position.x;
        
        // temporary self-managed until I add manager for this script
        Setup();
    }

    /// Managed setup (soon)
    public void Setup()
    {
        m_CurrentTimeModulo = 0f;
    }
    
    private void FixedUpdate()
    {
        m_CurrentTimeModulo = (m_CurrentTimeModulo + Time.deltaTime) % movePeriod;
        if (m_CurrentTimeModulo < moveDuration)
        {
            // Phase 1: move both panels from center (closed) to edge (open)
            float ratio = m_CurrentTimeModulo / moveDuration;
            m_RigidbodyPanelLeft.MovePosition(new Vector2(Mathf.Lerp(initialXLeft, initialXLeft - moveWidth, ratio), m_RigidbodyPanelLeft.position.y));
            m_RigidbodyPanelRight.MovePosition(new Vector2(Mathf.Lerp(initialXRight, initialXRight + moveWidth, ratio), m_RigidbodyPanelRight.position.y));
        }
        else if (m_CurrentTimeModulo < movePeriod / 2f)
        {
            // Phase 2: stay at edge (open)
            if (m_RigidbodyPanelLeft.position.x != initialXLeft - moveWidth)
            {
                m_RigidbodyPanelLeft.MovePosition(new Vector2( initialXLeft - moveWidth, m_RigidbodyPanelLeft.position.y));
            }
            if (m_RigidbodyPanelRight.position.x != initialXRight + moveWidth)
            {
                m_RigidbodyPanelRight.MovePosition(new Vector2( initialXRight + moveWidth, m_RigidbodyPanelRight.position.y));
            }
        }
        else if (m_CurrentTimeModulo < movePeriod / 2f + moveDuration)
        {
            // Phase 3: move from edge (open) to center (closed)
            float ratio = (m_CurrentTimeModulo - movePeriod / 2f) / moveDuration;
            m_RigidbodyPanelLeft.MovePosition(new Vector2(Mathf.Lerp(initialXLeft - moveWidth, initialXLeft, ratio), m_RigidbodyPanelLeft.position.y));
            m_RigidbodyPanelRight.MovePosition(new Vector2(Mathf.Lerp(initialXRight + moveWidth, initialXRight, ratio), m_RigidbodyPanelRight.position.y));
        }
        else  // m_CurrentTimeModulo < movePeriod
        {
            // Phase 4: stay at center (closed)
            if (m_RigidbodyPanelLeft.position.x != initialXLeft)
            {
                m_RigidbodyPanelLeft.MovePosition(new Vector2( initialXLeft, m_RigidbodyPanelLeft.position.y));
            }
            if (m_RigidbodyPanelRight.position.x != initialXRight)
            {
                m_RigidbodyPanelRight.MovePosition(new Vector2( initialXRight, m_RigidbodyPanelRight.position.y));
            }
        }
    }
}
