using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using CommonsHelper;

public class MovingPlatform : Device
{
    /* Children components */
    
    [Tooltip("Rigidbody of the platform body")]
    public Rigidbody2D m_RigidbodyPlatform;
    
    
    /* Sibling components */
    
    /// Polygonal path followed by the platform (not really a collider, just to avoid making a custom component)
    private PolygonCollider2D movePathPolygonal;
    
    /// Circular path followed by the platform (not really a collider, just to avoid making a custom component)
    /// (ignored if Move Path Polygonal is set)
    private CircleCollider2D movePathCircular;
    
    /// Bezier path followed by the platform (ignored if Move Path Polygonal or Circular is set)
    private BezierPath2DComponent movePathBezier;
    
    
    /* Parameters defined in inspector */
    
    [SerializeField, Tooltip("Move cycle period (s)")]
    private float movePeriod = 4f;
    
    [SerializeField, Tooltip("Move cycle offset (s)")]
    private float moveCycleOffset = 0f;
    
    
    /* State */
    
    /// Time tracker (reduced by modulo)
    private float m_CurrentTimeModulo;


    private void Awake()
    {
        movePathPolygonal = GetComponent<PolygonCollider2D>();
        movePathCircular = GetComponent<CircleCollider2D>();
        movePathBezier = GetComponent<BezierPath2DComponent>();

        int definedPathCount = 0;
        if (movePathCircular) ++definedPathCount;
        if (movePathPolygonal) ++definedPathCount;
        if (movePathBezier) ++definedPathCount;

        if (definedPathCount == 0)
        {
            Debug.LogErrorFormat(this, "No PolygonCollider2D, CircleCollider2D, BezierPath2DComponent found on {0}. ComputePositionOnPath will fall back to origin.", this);
        }
        else if (definedPathCount > 1)
        {
            Debug.LogErrorFormat(this, "Multiple path components found on {0}. They will be picked by set priority order.", this);
        }
    }
    
    private void Start()
    {
        // must be done after DeviceManager.Awake/Init
        DeviceManager.Instance.RegisterDevice(this);
    }
    
    private void OnDestroy()
    {
        // when stopping the game, DeviceManager may have been destroyed first so check it
        if (DeviceManager.Instance)
        {
            DeviceManager.Instance.UnregisterDevice(this);
        }
    }

    /// Managed setup
    public override void Setup()
    {
        m_CurrentTimeModulo = moveCycleOffset;
    }

    private void FixedUpdate()
    {
        m_CurrentTimeModulo = (m_CurrentTimeModulo + Time.deltaTime) % movePeriod;
        float normalizedT = m_CurrentTimeModulo / movePeriod;
        Vector2 positionOnPath = ComputePositionOnPath(normalizedT);
        
        // deduce velocity from target position this frame
        // do not use MovePosition which does not update velocity, preventing us from doing relative velocity
        // calculation and moving ground contribution calculation in CharacterRun
        // in counterpart we get some small value division which is not very precise, but fortunately errors are
        // compensated by moving faster if needed next frame to catch up the target position
        Vector2 velocity = (positionOnPath - m_RigidbodyPlatform.position) / Time.deltaTime;
        m_RigidbodyPlatform.velocity = velocity;
    }
    
    private Vector2 ComputePositionOnPath(float normalizedT)
    {
        if (movePathPolygonal != null)
        {
            if (movePathPolygonal.pathCount != 1)
            {
                Debug.LogErrorFormat("Expected exactly 1 polygonal path, got {0}", movePathPolygonal.pathCount);
                return (Vector2)transform.position;
            }
            
            return (Vector2)transform.position + movePathPolygonal.offset + ComputePositionOnPolygonalPath(movePathPolygonal.GetPath(0), normalizedT);
        }
        
        if (movePathCircular != null)
        {
            return (Vector2)transform.position + movePathCircular.offset + ComputePositionOnCircularPath(movePathCircular.radius, normalizedT);
        }
        
        if (movePathBezier != null)
        {
            // note that we don't support closed Bezier curves yet, so make sure to design the Bezier path
            // so the end point = start point, or be close enough to avoid platform suddenly warping when normalizedT = 0.999 -> 0
            // we also don't support relative Bezier paths, so just return position without adding transform.position
            return movePathBezier.InterpolatePathByNormalizedParameter(normalizedT);
        }
        
        return (Vector2)transform.position;
    }

    private static Vector2 ComputePositionOnPolygonalPath(IReadOnlyList<Vector2> polygonalPath, float normalizedT)
    {
        Debug.Assert(normalizedT >= 0f && normalizedT <= 1f);
            
        // handle edge cases (including cases that would normally assert)
        if (normalizedT <= 0)
        {
            return polygonalPath[0];
        }
        else if (normalizedT >= 1)
        {
            return polygonalPath[polygonalPath.Count - 1];
        }
        
        // path is cyclic for segment count = point count
        int segmentCount = polygonalPath.Count;
        
        // split equal durations between each segments
        // if segments have different lengths, move speed will not be constant, but always equal to
        // (segment length / duration). However, this is convenient to add breaks (segment where start = end)
        float pathT = normalizedT * segmentCount;
        int keyIndex = Mathf.FloorToInt(pathT);
            
        // we have handled normalizedT == 1 case, so we cannot be right at the end
        // and the keyIndex should never reach segmentCount
        Debug.Assert(keyIndex < segmentCount);

        float remainder = pathT - keyIndex;  // or pathT % 1f;
        
        // path is cyclic so apply modulo to segment end index to allow last-to-first-point edge
        return Vector2.Lerp(polygonalPath[keyIndex], polygonalPath[(keyIndex + 1) % segmentCount], remainder);
    }
    
    
    /// Return position on circle of given radius centered on (0, 0) at normalized parameter
    /// Start is at left of circle
    /// No center argument is passed, instead add the center to the result to get the final position
    private static Vector2 ComputePositionOnCircularPath(float radius, float normalizedT)
    {
        Debug.Assert(normalizedT >= 0f && normalizedT <= 1f);
        
        // handle edge cases (including cases that would normally assert)
        if (normalizedT <= 0 || normalizedT >= 1)
        {
            return Vector2.zero;
        }
        
        return radius * VectorUtil.Rotate(Vector2.left, 360 * normalizedT);
    }
}
