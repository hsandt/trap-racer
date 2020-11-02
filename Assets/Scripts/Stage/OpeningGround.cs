using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpeningGround : Device
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

    /// Managed setup (soon)
    public override void Setup()
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
            MoveLerpX(m_RigidbodyPanelLeft, initialXLeft, initialXLeft - moveWidth, ratio);
            MoveLerpX(m_RigidbodyPanelRight, initialXRight, initialXRight + moveWidth, ratio);
        }
        else if (m_CurrentTimeModulo < movePeriod / 2f)
        {
            // Phase 2: stay at edge (open)
            MoveX(m_RigidbodyPanelLeft, initialXLeft - moveWidth);
            MoveX(m_RigidbodyPanelRight, initialXRight + moveWidth);
        }
        else if (m_CurrentTimeModulo < movePeriod / 2f + moveDuration)
        {
            // Phase 3: move from edge (open) to center (closed)
            float ratio = (m_CurrentTimeModulo - movePeriod / 2f) / moveDuration;
            MoveLerpX(m_RigidbodyPanelLeft, initialXLeft - moveWidth, initialXLeft, ratio);
            MoveLerpX(m_RigidbodyPanelRight, initialXRight + moveWidth, initialXRight, ratio);
        }
        else  // m_CurrentTimeModulo < movePeriod
        {
            // Phase 4: stay at center (closed)
            MoveX(m_RigidbodyPanelLeft, initialXLeft);
            MoveX(m_RigidbodyPanelRight, initialXRight);
        }
    }

    private static void MoveX(Rigidbody2D rigidbody2d, float x)
    {
        if (rigidbody2d.position.x != x)
        {
            rigidbody2d.MovePosition(new Vector2(x, rigidbody2d.position.y));
        }
    }
    
    private static void MoveLerpX(Rigidbody2D rigidbody2d, float a, float b, float ratio)
    {
        rigidbody2d.MovePosition(new Vector2(Mathf.Lerp(a, b, ratio), rigidbody2d.position.y));
    }
}
