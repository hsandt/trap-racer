using System.Collections;
using System.Collections.Generic;
using CommonsHelper;
using UnityEngine;

public class CharacterRun : MonoBehaviour
{
    /* Sibling components */
    private Rigidbody2D m_Rigidbody2D;
    private BoxCollider2D m_Collider2D;
    
    /* Parameters */
    [SerializeField, Tooltip("Speed")]
    private float m_Speed = 10f;
    
    void Awake()
    {
        m_Rigidbody2D = this.GetComponentOrFail<Rigidbody2D>();
        m_Collider2D = this.GetComponentOrFail<BoxCollider2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        m_Rigidbody2D.velocity = m_Speed * Vector2.right;
    }
}
