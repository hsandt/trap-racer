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
    
    [SerializeField, Tooltip("Move cycle offset (s)")]
    private float moveCycleOffset = 0f;
    
    [SerializeField, Tooltip("Duration of the motion between bottom and top position itself (s). Is part of Move Period.")]
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
        
        m_CurrentTimeModulo = moveCycleOffset % movePeriod;
        
        // if moveCycleOffset is negative, we need to move it to positive range
        if (m_CurrentTimeModulo < 0f)
        {
            m_CurrentTimeModulo += movePeriod;
        }
    }

    private void FixedUpdate()
    {
        // advance time (assuming we did positive modulo properly in Setup, it should remain in the correct range)
        m_CurrentTimeModulo = (m_CurrentTimeModulo + Time.deltaTime) % movePeriod;
        
        if (m_CurrentTimeModulo < moveDuration)
        {
            // Phase 1: move from bottom to top
            float ratio = m_CurrentTimeModulo / moveDuration;
            MoveLerpY(initialY, initialY + moveHeight, ratio);
        }
        else if (m_CurrentTimeModulo < movePeriod / 2f)
        {
            // Phase 2: stay at top
            MoveY(initialY + moveHeight);
        }
        else if (m_CurrentTimeModulo < movePeriod / 2f + moveDuration)
        {
            // Phase 3: move from top to bottom
            float ratio = (m_CurrentTimeModulo - movePeriod / 2f) / moveDuration;
            MoveLerpY(initialY + moveHeight, initialY, ratio);
        }
        else  // m_CurrentTimeModulo < movePeriod
        {
            // Phase 4: stay at bottom
            MoveY(initialY);
        }
    }
    
    public override void Pause()
    {
        enabled = false;
        m_Rigidbody2D.simulated = false;
    }

    public override void Resume()
    {
        enabled = true;
        m_Rigidbody2D.simulated = true;
    }
    
    private void MoveY(float y)
    {
        if (m_Rigidbody2D.position.y != y)
        {
            m_Rigidbody2D.MovePosition(new Vector2(m_Rigidbody2D.position.x, y));
        }
    }
    
    private void MoveLerpY(float a, float b, float ratio)
    {
        m_Rigidbody2D.MovePosition(new Vector2(m_Rigidbody2D.position.x, Mathf.Lerp(a, b, ratio)));
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        GizmosUtil.DrawLocalLine(Vector3.zero, moveHeight * Vector3.up, transform, Color.green);
    }
#endif
}
