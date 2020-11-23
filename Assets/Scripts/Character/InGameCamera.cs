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
    
    
    /* Parameters defined on start */

    /// Base FoV set to initial FoV prepared for 16:9 aspect, used to calculate adapted FoV
    private float baseFoV;
    

    /* State vars */
    
    /// List of character transforms
    private readonly List<Transform> m_CharacterTransforms = new List<Transform>();


    private void Awake()
    {
        m_Camera = this.GetComponentOrFail<Camera>();
        
        baseFoV = m_Camera.fieldOfView;
        
        // register runners earlier than Start as RaceManager will require camera to be ready during Setup
        // or use RaceManager.GetRunner()
        foreach (Transform characterTr in charactersParent)
        {
            m_CharacterTransforms.Add(characterTr);
        }
        Debug.Assert(m_CharacterTransforms.Count > 0, "No character transforms found");
    }

    /// Managed setup
    public void Setup()
    {
        // important to warp without clamping nor warping to get correct position, esp. on restart
        WarpCameraToTargetPosition();
    }

    private void LateUpdate()
    {
        UpdateCameraPosition();
    }

    private Vector3 ComputeTargetPosition()
    {
        float targetX = m_CharacterTransforms.Average(tr => tr.position.x);
        Vector3 position = transform.parent.position;
        return new Vector3(targetX, position.y, position.z);
    }

    private void WarpCameraToTargetPosition()
    {
        transform.parent.position = ComputeTargetPosition();
    }

    private void UpdateCameraPosition()
    {
#if UNITY_EDITOR
        // in Unity it's convenient to adjust zoom when resizing the Game view
        AdjustZoomToShowFixedWidth();
#endif
        // center position between all the characters on X, but preserve Y for stability
        Vector3 targetPosition = ComputeTargetPosition();

        // Camera is now placed on anchor so we can add an offset on X (backward) to make sure we keep the characters
        // in sight despite using a perspective angle (tilted forward)
        // so we move the parent anchor instead of the camera itself
        Vector3 position = transform.parent.position;
        
        if (RaceManager.Instance.State == RaceState.Started)
        {
            if (targetPosition.x < position.x - 1f)
            {
                Debug.LogWarningFormat("[InGameCamera] Target position X {0} is too much behind current position X {1}," +
                                       "it will still be clamped forward and runners may warp suddenly forward!",
                    targetPosition.x, position.x);
            }
            
            // if during the race, runners are going too slow, apply min scrolling speed so if they continue they'll hit the left edge
            // and are forced to move at minScrollingSpeed too
            targetPosition.x = Mathf.Max(targetPosition.x, position.x + minScrollingSpeed * Time.deltaTime);
        }

        transform.parent.position = Vector3.Lerp(transform.parent.position, targetPosition, smoothFactor);
    }

    private void AdjustZoomToShowFixedWidth()
    {
        // since camera is tilted to the right and downward, we cannot apply the exact tangent formula
        // to compute the FoV required to see exactly fixedHalfWidth for a camera at a given Z distance
        // from the game plane, and would a combination of trigonometric calculations
        // but to simplify, let's say the camera is at 12m of the game plane, looking orthogonally into it
        float gamePlaneHalfHeight = fixedHalfWidth / m_Camera.aspect;
        float estimatedCameraToGamePlaneDistance = 12f;
        m_Camera.fieldOfView = Mathf.Rad2Deg * 2 * Mathf.Atan2(gamePlaneHalfHeight, estimatedCameraToGamePlaneDistance);
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
