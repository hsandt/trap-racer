using System.Collections;
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
    
    [SerializeField, Tooltip("Smooth factor applied to Vector3.Lerp")]
    private float smoothFactor = 0.1f;

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
    }

    /// Managed setup
    public void Setup()
    {
        UpdateCameraPosition();
    }

    private void LateUpdate()
    {
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
#if UNITY_EDITOR
        // in Unity it's convenient to adjust zoom when resizing the Game view
        AdjustZoomToShowFixedWidth();
#endif
        // center position between all the characters on X, but preserve Y for stability
        float newPositionX = m_CharacterTransforms.Average(tr => tr.position.x);

        // Camera is now placed on anchor so we can add an offset on X (backward) to make sure we keep the characters
        // in sight despite using a perspective angle (tilted forward)
        // so we move the parent anchor instead of the camera itself
        Vector3 position = transform.parent.position;
        
        if (RaceManager.Instance.State == RaceState.Started)
        {
            // if during the race, runners are going too slow, apply min scrolling speed so if they continue they'll hit the left edge
            // and are forced to move at minScrollingSpeed too
            newPositionX = Mathf.Max(newPositionX, position.x + minScrollingSpeed * Time.deltaTime);
        }

        Vector3 targetPosition = new Vector3(newPositionX, position.y, position.z);
        transform.parent.position = Vector3.Lerp(transform.parent.position, targetPosition, smoothFactor);
    }

    private void AdjustZoomToShowFixedWidth()
    {
        // in 2D, Zoom means Orthographic Size, in 3D it would mean FoV
        // TODO: do this with FoV for 3D camera now, if different aspect ratios give weird results
        m_Camera.orthographicSize = fixedHalfWidth / m_Camera.aspect;
    }

    public float GetLeftEdgeX()
    {
        // Left Edge is still defined from anchor placed around center of two characters
        // This doesn't mean that characters are really hitting the screen edge since in 3D it's hard to define
        // "screen edge". Instead the camera offset from its parent anchor will give us some margin to keep
        // leftmost character on-screen.
        return transform.parent.position.x - fixedHalfWidth + leftEdgeMargin;
    }
}
