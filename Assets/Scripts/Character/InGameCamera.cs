﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using CommonsHelper;

public class InGameCamera : MonoBehaviour
{
    /* External references */
    
    [Tooltip("_Characters parent transform")]
    public Transform charactersParent;
    
    
    /* Sibling components */
    
    private Camera m_Camera;
    
    
    /* Parameters */
    
    [SerializeField, Tooltip("Fixed half-width of the world shown the camera")]
    private float fixedHalfWidth = 9f;

    [SerializeField, Tooltip("Margin from left edge to keep runner visible inside screen")]
    private float leftEdgeMargin = 0.5f;

    [SerializeField, Tooltip("Minimum scrolling speed. Left edge blocks runners so that also affect character motion.")]
    private float minScrollingSpeed = 1f;
    public float MinScrollingSpeed => minScrollingSpeed;

    /* State vars */
    
    /// List of character transforms
    private readonly List<Transform> m_CharacterTransforms = new List<Transform>();


    private void Awake()
    {
        m_Camera = this.GetComponentOrFail<Camera>();
    }
    
    private void Start()
    {
        foreach (Transform characterTr in charactersParent)
        {
            m_CharacterTransforms.Add(characterTr);
        }
        Debug.Assert(m_CharacterTransforms.Count > 0, "No character transforms found");
        
        // in real game you can only change aspect ratio in the Options so setting correct zoom on start
        // should be enough
        AdjustZoomToShowFixedWidth();
    }

    private void LateUpdate()
    {
#if UNITY_EDITOR
        // in Unity it's convenient to adjust zoom when resizing the Game view
        AdjustZoomToShowFixedWidth();
#endif
        // center position between all the characters on X, but preserve Y for stability
        float newPositionX = m_CharacterTransforms.Average(tr => tr.position.x);

        Vector3 position = transform.position;
        
        if (RaceManager.Instance.State == RaceState.Started)
        {
            // if during the race, runners are going too slow, apply min scrolling speed so if they continue they'll hit the left edge
            // and are forced to move at minScrollingSpeed too
            newPositionX = Mathf.Max(newPositionX, position.x + minScrollingSpeed * Time.deltaTime);
        }

        transform.position = new Vector3(newPositionX, position.y, position.z);
    }

    private void AdjustZoomToShowFixedWidth()
    {
        // in 2D, Zoom means Orthographic Size, in 3D it would mean FoV
        m_Camera.orthographicSize = fixedHalfWidth / m_Camera.aspect;
    }

    public float GetLeftEdgeX()
    {
        return transform.position.x - fixedHalfWidth + leftEdgeMargin;
    }
}
