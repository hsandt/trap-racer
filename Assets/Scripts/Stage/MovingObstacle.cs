using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;

public class MovingObstacle : Obstacle
{
    /* Sibling components */
    private Rigidbody2D m_Rigidbody2D;
    
    /* Parameters defined in inspector */
    
    [SerializeField, Tooltip("Height to which this obstacle is moving. Initial position is bottom position.")]
    private float moveHeight = 2f;
    
    [SerializeField, Tooltip("Move cycle period. Take half to get duration between start of move phases (s)")]
    private float movePeriod = 4f;
    
    [SerializeField, Tooltip("Duration of the motion between bottom and top position itself (s). Is part of Move Interval.")]
    private float moveDuration = 0.5f;

    /* Parameters defined on start */
    
    /// Initial Y position
    private float initialY;

    /* State */
    
    /// Time tracker (reduced by modulo)
    private float m_CurrentTimeModulo;

    protected override void Init()
    {
        base.Init();

        m_Rigidbody2D = this.GetComponentOrFail<Rigidbody2D>();

        initialY = m_Rigidbody2D.position.y;
    }

    public override void Setup()
    {
        base.Setup();
        
        m_CurrentTimeModulo = 0f;
    }
    
    private void FixedUpdate()
    {
        m_CurrentTimeModulo = (m_CurrentTimeModulo + Time.deltaTime) % movePeriod;
        if (m_CurrentTimeModulo < moveDuration)
        {
            // Phase 1: move from bottom to top
            float ratio = m_CurrentTimeModulo / moveDuration;
            m_Rigidbody2D.MovePosition(new Vector2(m_Rigidbody2D.position.x, Mathf.Lerp(initialY, initialY + moveHeight, ratio)));
        }
        else if (m_CurrentTimeModulo < movePeriod / 2f)
        {
            // Phase 2: stay at top
            if (m_Rigidbody2D.position.y != initialY + moveHeight)
            {
                m_Rigidbody2D.MovePosition(new Vector2(m_Rigidbody2D.position.x, initialY + moveHeight));
            }
        }
        else if (m_CurrentTimeModulo < movePeriod / 2f + moveDuration)
        {
            // Phase 3: move from top to bottom
            float ratio = (m_CurrentTimeModulo - movePeriod / 2f) / moveDuration;
            m_Rigidbody2D.MovePosition(new Vector2(m_Rigidbody2D.position.x, Mathf.Lerp(initialY + moveHeight, initialY, ratio)));
        }
        else  // m_CurrentTimeModulo < movePeriod
        {
            // Phase 4: stay at bottom
            if (m_Rigidbody2D.position.y != initialY)
            {
                m_Rigidbody2D.MovePosition(new Vector2(m_Rigidbody2D.position.x, initialY));
            }
        }
    }
}
