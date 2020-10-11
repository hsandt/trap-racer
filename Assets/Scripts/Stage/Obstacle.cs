using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;

public class Obstacle : MonoBehaviour
{
    /* Sibling components */
    private Collider2D m_Collider2D;
    private SpriteRenderer m_SpriteRenderer;

    private void Awake()
    {
        m_Collider2D = this.GetComponentOrFail<Collider2D>();
        m_SpriteRenderer = this.GetComponentOrFail<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        var characterRun = other.collider.GetComponentOrFail<CharacterRun>();
        characterRun.CrashIntoObstacle();

        // disable collider
        m_Collider2D.enabled = false;

        // no animation yet, for prototype just hide the obstacle
        m_SpriteRenderer.enabled = false;
    }
}
