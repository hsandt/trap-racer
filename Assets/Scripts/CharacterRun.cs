using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsDebug;
using CommonsHelper;

public class CharacterRun : MonoBehaviour
{
    /* Static member for unique raycast allocations for CharacterRun scripts */
    private static readonly RaycastHit2D[] RaycastHits = new RaycastHit2D[2];
    
    /* Child references */
    [Tooltip("Position X from which ground is detected. Y must be at feet level. Actual raycast Y range is defined by Ground Detection Start/Stop Offset parameters.")]
    public Transform groundSensorXTr;
    
    /* Sibling components */
    private Rigidbody2D m_Rigidbody2D;
    private BoxCollider2D m_Collider2D;
    
    /* Parameters */
    
    [SerializeField, Tooltip("Ground raycast contact filter")]
    private ContactFilter2D groundFilter = new ContactFilter2D();

    [SerializeField, Tooltip("Distance above feet to detect ground (must be at least expected penetration depth)")]
    private float groundDetectionStartOffset = 0.1f;

    [SerializeField, Tooltip("Distance below feet to stop detecting ground. Must be positive to at least detect ground at feet level, but a very small margin is enough. It is only to avoid missing ground just at the tip of a raycast, as ground detected more below will be ignored anyway.")]
    private float groundDetectionStopOffset = 0.1f;

    [SerializeField, Tooltip("Run speed X")]
    private float runSpeed = 10f;
    
    [SerializeField, Tooltip("Jump speed Y")]
    private float jumpSpeed = 10f;
    
    [SerializeField, Tooltip("Gravity acceleration (positive downward)")]
    private float gravity = 10f;

    /* State vars */
    private CharacterState m_State;
    
    void Awake()
    {
        m_Rigidbody2D = this.GetComponentOrFail<Rigidbody2D>();
        m_Collider2D = this.GetComponentOrFail<BoxCollider2D>();
    }

    private void Start()
    {
        m_State = CharacterState.Run;
    }

    void FixedUpdate()
    {
        if (m_State == CharacterState.Run)
        {
            m_Rigidbody2D.velocity = runSpeed * Vector2.right;
        }
        else if (m_State == CharacterState.Jump)
        {
            // sense ground when character starts falling
            if (m_Rigidbody2D.velocity.y < 0f && SenseGround(out var groundDistance))
            {
                // detected ground, leave Jump, adjust position Y and reset velocity Y
                m_State = CharacterState.Run;
                
                // after MovePosition, physics velocity is not normally applied,
                //  so to avoid lagging by 1 frame on X on landing, we manually inject dx on landing frame
                m_Rigidbody2D.MovePosition(m_Rigidbody2D.position +
                    new Vector2(m_Rigidbody2D.velocity.x * Time.deltaTime, groundDistance));
                m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0f);
            }
            else
            {
                // apply gravity
                m_Rigidbody2D.velocity += gravity * Time.deltaTime * Vector2.down;
            }
        }
    }

    /// Sense ground slightly above or at feet level and return true iff ground is detected,
    /// along with out distance to it, positive upward.
    /// Out distance should be positive or zero, zero if just touching ground and positive if slightly above.
    /// If ground is too high or below feet, no ground is detected, return false with outDistance 0 (unused).
    private bool SenseGround(out float outGroundDistance)
    {
        Vector2 origin = (Vector2) groundSensorXTr.position - groundDetectionStartOffset * Vector2.up;
        int hitCount = Physics2DUtil.RaycastDebug(origin, Vector2.down, groundFilter, RaycastHits,
            groundDetectionStopOffset - groundDetectionStartOffset);
        if (hitCount > 0)
        {
            // only consider first hit
            float hitDistance = RaycastHits[0].distance;
            
            // only consider ground actually at feet level or above
            if (hitDistance <= groundDetectionStartOffset)
            {
                outGroundDistance = groundDetectionStartOffset - hitDistance;
                return true;
            }
        }

        outGroundDistance = 0f;
        return false;
    }

    private void OnJump()
    {
        if (m_State == CharacterState.Run)
        {
            m_State = CharacterState.Jump;
            
            m_Rigidbody2D.velocity += jumpSpeed * Vector2.up;
        }
    }
}
