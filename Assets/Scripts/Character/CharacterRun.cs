//#define DEBUG_CHARACTER_RUN

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsDebug;
using CommonsHelper;

[SelectionBase]
public class CharacterRun : MonoBehaviour
{
    public struct GroundInfo
    {
        public Collider2D groundCollider;
        public float groundDistance;
        public Vector2 tangentDir;
    }
    
    /* Static member for unique raycast allocations for CharacterRun scripts */
    private static readonly RaycastHit2D[] RaycastHits = new RaycastHit2D[2];

    /* Cached external references */
    private InGameCamera m_InGameCamera;

    /* Child references */
    [Tooltip(
        "Position X from which ground is detected. Y must be at feet level. Place it slightly behind the character to allow last second jump. Actual raycast Y range is defined by Ground Detection Start/Stop Offset parameters.")]
    public Transform groundSensorXTr;
    
    [Tooltip("Mesh animator (Stickman)")]
    public Animator meshAnimator;

    /* Sibling components */
    private Rigidbody2D m_Rigidbody2D;
    private BoxCollider2D m_Collider2D;
    private FlagBearer m_FlagBearer;

    /* Parameters */

    [SerializeField, Tooltip("Player number (1 or 2)")]
    private int playerNumber = 0;

    public int PlayerNumber => playerNumber;

    [SerializeField, Tooltip("Ground raycast contact filter")]
    private ContactFilter2D groundFilter = default;

    [SerializeField, Tooltip("Distance above feet to detect ground (must be at least Max Step Up Distance)")]
    private float groundDetectionStartMargin = 0.5f;

    [SerializeField, Tooltip("Maximum step up distance allowed each frame to escape ground.")]
    private float maxStepUpDistance = 0.4f;

    [SerializeField, Tooltip("Maximum step down distance allowed each frame to stick to ground when grounded. " +
                             "Tends to be quite high to allow fast descents including on moving platforms " +
                             "(although velocity should do most of the job)")]
    private float maxStepDownDistanceGrounded = 0.4f;

    [SerializeField, Tooltip("Maximum step down distance allowed each frame to stick to ground when airborne. " +
                             "Tends to be smaller than grounded value to avoid sudden landing.")]
    private float maxStepDownDistanceAirborne = 0.1f;

    [SerializeField, Tooltip("Distance below feet to stop detecting ground. Must be at least Max Step Down Distance.")]
    private float groundDetectionStopMargin = 0.5f;

    [SerializeField, Tooltip("Distance between feet and ground under which we consider character to be just touching ground, " +
         "so no Y adjustment is applied. This is used for both penetration tolerance (small positive distance sensed) " +
         "and hover tolerance (small negative distance sensed). This prevents oscillations between Landing and Falling " +
         "as the feet Y wouldn't exactly match ground Y due to floating precision. Should be lower than margins above.")]
    private float groundDetectionToleranceHalfRange = 0.01f;

    [SerializeField, Tooltip("Run speed X at normal pace")]
    private float baseRunSpeed = 10f;

    [SerializeField, Tooltip("Factor applied to run speed X when slowed down by an obstacle")]
    private float obstacleSlowDownRunSpeedMultiplier = 0.7f;

    [SerializeField, Tooltip("Factor applied to run speed X when holding the flag")]
    private float flagSlowDownRunSpeedMultiplier = 0.7f;

    [SerializeField, Tooltip("Factor applied to run speed X when actively trying to brake with left input")]
    private float brakeSlowDownRunSpeedMultiplier = 0.7f;

    [SerializeField, Tooltip("Jump speed (added to current velocity Y)")]
    private float jumpSpeed = 10f;

    [SerializeField, Tooltip("Minimum velocity Y after jump in case you jump from a platform moving down so fast it would cancel the jump effect")]
    private float minResultingJumpSpeed = 1f;

    [SerializeField, Tooltip("Trampoline jump speed Y")]
    private float trampolineJumpSpeed = 10f;

    [SerializeField, Tooltip("Gravity acceleration (positive downward)")]
    private float gravity = 10f;

    [SerializeField, Tooltip("Slowdown duration on obstacle hit (running only)")]
    private float obstacleSlowdownDuration = 1f;

    [SerializeField, Tooltip("Slowdown factor applied to velocity X when airborne")]
    private float obstacleSlowdownFactorAirborne = 0.5f;

    [SerializeField, Tooltip("Auto-deceleration on X after race finish")]
    private float finishDecel = 3f;

    /* State vars */

    /// Current state
    private CharacterState m_State;
    
    /// Can the player control this runner?
    private bool m_CanControl;

    /// Obstacle slow down timer. When positive, character is currently slowed down by the last obstacle hurt.
    private float m_ObstacleSlowDownTimer;

    /// Is the player trying to brake? (active slowdown)
    private bool m_BrakeIntention;

    /// Current ground collider, if any
    private Collider2D currentGround;
    
    /// Current moving ground rigidbody, if any
    private Rigidbody2D currentMovingGroundBody;
    
    /// Current conveyor belt, if any (can only be component of currentGround)
    private ConveyorBelt currentConveyorBelt;
    
    /// Current tangent direction (unit vector)
    private Vector2 tangentDir;
    
    // Animator param hashes

    private static readonly int Airborne = Animator.StringToHash("Airborne");
    private static readonly int Jumping = Animator.StringToHash("Jumping");
    private static readonly int SpeedX = Animator.StringToHash("SpeedX");

    public bool IsAirborne()
    {
        return m_State == CharacterState.Jump || m_State == CharacterState.Fall;
    }

    private void Awake()
    {
        m_InGameCamera = Camera.main.GetComponentOrFail<InGameCamera>();

        m_Rigidbody2D = this.GetComponentOrFail<Rigidbody2D>();
        m_Collider2D = this.GetComponentOrFail<BoxCollider2D>();
        m_FlagBearer = this.GetComponentOrFail<FlagBearer>();
    }

    /// Managed setup
    /// Not called on own Start, must be called in RaceManager.SetupRace
    /// if no RaceManager is present (workshop scene), state var will keep default values
    public void Setup()
    {
        m_Rigidbody2D.velocity = Vector2.zero;

        m_State = CharacterState.WaitForStart;
        m_CanControl = false;
        m_ObstacleSlowDownTimer = 0f;
        m_BrakeIntention = false;
        currentGround = null;
        tangentDir = Vector2.right;
    }

    public void StartRunning()
    {
        m_State = CharacterState.Run;
        m_CanControl = true;
    }

    private void FixedUpdate()
    {
        UpdateMotion();
        UpdateAnimator();
    }
    
    private void UpdateMotion()
    {
        // don't move before race start
        if (m_State == CharacterState.WaitForStart)
        {
            return;
        }
        
        // update slowdown timer
        if (m_ObstacleSlowDownTimer > 0f)
        {
            m_ObstacleSlowDownTimer = Mathf.Max(0f, m_ObstacleSlowDownTimer - Time.deltaTime);
        }
        
        // Transitions
        
        if (m_State == CharacterState.Run)
        {
            // sense ground to check if no ground anymore
            // for now, we ignore the result groundDistance if ground is detected
            //  as we assume that all platforms are flat and we don't need to escape ground while running
            if (!SenseGround(out GroundInfo groundInfo))
            {
                // no ground detected, fall (we can wait next frame to start applying gravity)
                m_State = CharacterState.Fall;
                SetGround(null);
                tangentDir = groundInfo.tangentDir;
#if DEBUG_CHARACTER_RUN
                Debug.LogFormat(this, "[CharacterRun] #{0} No ground detected, Fall", playerNumber);
#endif
            }
            else
            {
                SetGround(groundInfo.groundCollider);
                tangentDir = groundInfo.tangentDir;
                AdjustYToGround(groundInfo.groundDistance);
            }
        }
        else
        {
            // airborne
            switch (m_State)
            {
                case CharacterState.Jump:
                {
                    // character starts falling when velocity is downward
                    if (m_Rigidbody2D.velocity.y < 0f)
                    {
                        // no need to detect ground this frame, just update state
                        // and starting next frame, we will sense ground as part of the Fall state update
                        // (animation transition from Jump to Fall uses exit time, not condition,
                        // so this will not start the Fall animation)
                        m_State = CharacterState.Fall;
                    }
                    break;
                }
                case CharacterState.Fall:
                {
                    // we assume falling character is moving downward so always sense ground in this state
                    if (SenseGround(out GroundInfo groundInfo))
                    {
                        // detected ground, leave Fall, adjust position Y to ground and reset velocity Y
                        m_State = CharacterState.Run;
                        SetGround(groundInfo.groundCollider);
                        tangentDir = groundInfo.tangentDir;
                        Land(groundInfo.groundDistance);
#if DEBUG_CHARACTER_RUN
                        Debug.LogFormat(this, "[CharacterRun] #{0} Ground detected, Land at groundDistance: {0}", playerNumber, groundInfo.groundDistance);
#endif
                    }
                    break;
                }
            }
        }

        if (m_State == CharacterState.Run)
        {
            // run following current slope (magnitude is preserved which means you lose some speed X on slopes,
            // even descending ones; we can add a slope factor on speed, not accel, to counter that)
            SetVelocityOnGround();
        }
        else  // airborne
        {
            // apply gravity (preserve speed X, so jumping from a slope is disadvantageous)
            Vector2 velocity = m_Rigidbody2D.velocity;
            m_Rigidbody2D.velocity = new Vector2(velocity.x, velocity.y - gravity * Time.deltaTime);
        }
    }

    /// Return run speed that runner should have now, if running on ground
    private float ComputeRunSpeed()
    {
        float runSpeed;

        if (m_CanControl)
        {
            // After doing all transitions, set velocity based on the resulting state
            runSpeed = GetRunSpeedMultiplier() * baseRunSpeed + GetRunSpeedOffset();

            // If runner is behind camera left edge limit, clamp speed to minimum to catch up
            // this is not enough as small offsets may accumulate over time,
            // so must clamp position itself (we still clamp speed in case we have accel based on previous speed
            //  or speed-based animations later)
            // Currently there's some jittering due to the fact that we use positions at the beginning of the frame,
            //  but we see the game through the camera at the end of the frame (after LateUpdate)
            // It may be fixed by applying clamping at the end of the frame (but InGameCamera.LateUpdate should not contain
            //  physics so we may need an external manager or something)
            float leftEdgeX = m_InGameCamera.GetLeftEdgeX();
            if (m_Rigidbody2D.position.x < leftEdgeX)
            {
                runSpeed = m_InGameCamera.MinScrollingSpeed;
                m_Rigidbody2D.MovePosition(new Vector2(leftEdgeX, m_Rigidbody2D.position.y));
            }
        }
        else
        {
            // we only lose control completely after finishing the race, and should slow down to a halt at this point
            runSpeed = Mathf.Max(0f, m_Rigidbody2D.velocity.x - finishDecel * Time.deltaTime);
        }

        return runSpeed;
    }

    /// Compute and return speed multiplier from current state
    private float GetRunSpeedMultiplier()
    {
        float speedMultiplier = 1f;

        bool isSlowedDownByObstacle = m_ObstacleSlowDownTimer > 0f;
        bool isSlowedDownByFlag = m_FlagBearer.HasFlag();
        
        if (isSlowedDownByObstacle)
        {
            speedMultiplier *= obstacleSlowDownRunSpeedMultiplier;
        }
        
        if (isSlowedDownByFlag)
        {
            speedMultiplier *= flagSlowDownRunSpeedMultiplier;
        }
        
        // only apply active slowdown (brake) when not hurt by obstacle (lose control)
        // nor flag (to avoid escaping the other player going more to the left)
        if (!isSlowedDownByObstacle && !isSlowedDownByFlag && m_BrakeIntention)
        {
            speedMultiplier *= brakeSlowDownRunSpeedMultiplier;
        }

        return speedMultiplier;
    }
    
    /// Compute and return speed offset from current state
    private float GetRunSpeedOffset()
    {
        float speedOffset = 0f;

        // Do nothing... we used to apply conveyor belt speed offset here,
        // but realized it was more logical to add it along moving ground contribution
        // (and subtract it to get local speed back) since it really represents ground moving below
        // character, just not with a rigidbody.
        // Separating it from the intentional speed will also prevent side effects like clamping at max speed
        // too early when running on a SpeedUp conveyor belt. 

        return speedOffset;
    }
    
    /// Sense ground slightly above or at feet level and return true iff ground is detected,
    /// along with out distance to it, positive upward.
    /// Out distance should be positive or zero, zero if just touching ground and positive if slightly above.
    /// If ground is too high or below feet, no ground is detected, return false with outDistance 0 (unused).
    private bool SenseGround(out GroundInfo outGroundInfo)
    {
        outGroundInfo = new GroundInfo();
        
        Vector2 origin = (Vector2) groundSensorXTr.position + groundDetectionStartMargin * Vector2.up;
        // make sure to compute sum of start and stop margin, not difference,
        // as the former is upward, the latter backward
        int hitCount = Physics2DUtil.RaycastDebug(origin, Vector2.down, groundFilter, RaycastHits,
            groundDetectionStartMargin + groundDetectionStopMargin);
        if (hitCount > 0)
        {
            // only consider first hit
            RaycastHit2D hit = RaycastHits[0];
            float hitDistance = hit.distance;
            
            // signed ground distance is negative when inside ground, positive when above ground
            outGroundInfo.groundDistance = - groundDetectionStartMargin + hitDistance;

            float maxStepDownDistance = IsAirborne() ? maxStepDownDistanceAirborne : maxStepDownDistanceGrounded;

            if (- maxStepUpDistance <= outGroundInfo.groundDistance && outGroundInfo.groundDistance <= maxStepDownDistance)
            {
                // in all cases we'll update the current ground and tangent direction
                outGroundInfo.groundCollider = hit.collider;

                Vector2 normal = hit.normal;
                outGroundInfo.tangentDir = VectorUtil.Rotate90CW(normal);
                
                if (Mathf.Abs(outGroundInfo.groundDistance) <= groundDetectionToleranceHalfRange)
                {
                    // we're close enough to ground (slightly inside or above)
                    // to consider we are grounded and don't need any offset (prevents oscillation around ground Y)
                    outGroundInfo.groundDistance = 0f;
                    return true;
                }
                else  // Abs(outGroundInfo.groundDistance) > groundDetectionToleranceHalfRange
                {
                    // we are inside the ground by a meaningful distance, so keep outGroundDistance for Y adjustment
                    return true;
                }
            }
            // else, we are either too much inside ground to step up, or too high above ground to step down
            // continue below as we consider character airborne
        }
        // else, no ground detected at all, so consider character airborne
        // continue below
        
        // in the air, we don't use tangent dir so just default to horizontal
        outGroundInfo.groundCollider = null;
        outGroundInfo.tangentDir = Vector2.right;
        outGroundInfo.groundDistance = 0f;  // just because we need something for out in case we didn't enter the block
        return false;
    }

    private void SetGround(Collider2D ground)
    {
        currentGround = ground;

        if (ground != null)
        {
            // do not fail if no special component is found, anything is possible
            currentConveyorBelt = currentGround.GetComponent<ConveyorBelt>();
            currentMovingGroundBody = currentGround.GetComponent<Rigidbody2D>();
        }
    }
    
    /// Adjust position to land on ground located at groundDistance, positive downward
    /// This method does not set the state to Run, do it separately
    private void Land(float groundDistance)
    {
        Debug.Assert(currentGround != null, "Land should be called *after* setting current ground");
        
        AdjustYToGround(groundDistance);

        // normally no need to update velocity, it will be ignored this frame, and next frame FixedUpdate will set it
        // but if you fear exploit of jumping immediately after landing on slope to preserve speed X (assuming
        // input is processed before FixedUpdate, which is easy to fix by only setting intention in OnJump and
        // processing it in FixedUpdate), then set it now as below
        SetVelocityOnGround();
    }

    private void AdjustYToGround(float groundDistance)
    {
        // we can afford comparison to 0 here because we are supposed to have checked that abs ground distance
        // was above groundDetectionToleranceHalfRange
        if (groundDistance != 0f)
        {
            // after MovePosition, physics velocity is not normally applied,
            //  so to avoid lagging by 1 frame on X on landing, we manually inject dx on landing frame
            // since we know call this every frame while grounded (to fix small errors even with tangent velocity and
            // world contribution, esp. for platforms with complex motion), velocity is only used via physics when airborne
            m_Rigidbody2D.MovePosition(m_Rigidbody2D.position +
                                       new Vector2(m_Rigidbody2D.velocity.x * Time.deltaTime, -groundDistance));
        }
    }

    private void SetVelocityOnGround()
    {
        // local velocity is decided by run speed
        Vector2 localVelocity = ComputeRunSpeed() * tangentDir;

        // add any ground velocity to get world velocity and avoid falling into / rising above moving platforms
        // nor running past them at the usual speed as if nothing happened when they have velocity along X
        m_Rigidbody2D.velocity = localVelocity + ComputeLocalToWorldVelocityContribution();
    }

    private Vector2 ComputeLocalVelocity()
    {
        return m_Rigidbody2D.velocity - ComputeLocalToWorldVelocityContribution();
    }

    /// Compute moving ground velocity contribution (but we call it local to world because technically conveyor belt
    /// is not a moving object)
    private Vector2 ComputeLocalToWorldVelocityContribution()
    {
        Vector2 localToWorldVelocity = Vector2.zero;
        
        if (currentMovingGroundBody != null)
        {
            localToWorldVelocity += currentMovingGroundBody.velocity;
        }

        if (currentConveyorBelt != null)
        {
            // not a rigidbody but in real world it would be a moving ground, so consider it a contribution to world
            // velocity
            localToWorldVelocity += currentConveyorBelt.ExtraSpeed * Vector2.right;
        }

        return localToWorldVelocity;
    }

    private void UpdateAnimator()
    {
        // to simplify we don't rotate character on slope and just keep using speed X for animation speed
        // however we take moving ground / conveyor belts into account so anim matches character's own velocity
        Vector2 localVelocity = ComputeLocalVelocity();
        meshAnimator.SetFloat(SpeedX, localVelocity.x);
        meshAnimator.SetBool(Airborne, IsAirborne());
        meshAnimator.SetBool(Jumping, m_State == CharacterState.Jump);
    }
    
    // Obstacle

    /// Start obstacle slow down timer
    /// Leave obstacle destruction to Obstacle script side
    /// If you already hit an obstacle and timer is still active, just reset it to duration
    public void CrashIntoObstacle()
    {
        // set timer for slow down when running on ground
        m_ObstacleSlowDownTimer = obstacleSlowdownDuration;

        // when airborne, we still need to slow down character for penalty so apply it directly to velocity X
        if (IsAirborne())
        {
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x * obstacleSlowdownFactorAirborne, m_Rigidbody2D.velocity.y);
        }
    }
    
    // Switch
    
    public void PlayToggleSwitchAnim()
    {
        // TODO
    }
    
    // Trampoline
    
    public void JumpWithTrampoline()
    {
        m_State = CharacterState.Jump;
        Vector2 velocity = m_Rigidbody2D.velocity;
        m_Rigidbody2D.velocity = new Vector2(velocity.x, Mathf.Max(trampolineJumpSpeed, velocity.y));
    }
    
    // Warp
    
    public void WarpTo(Transform targetWarperTr)
    {
        m_Rigidbody2D.position = targetWarperTr.position;
    }
    
    public void FinishRace()
    {
        m_CanControl = false;

        PlayFinishAnim();
    }
    
    public void PlayFinishAnim()
    {
        // TODO
    }
    
    /// Input callback: Jump action
    public void OnJump()
    {
        if (m_CanControl && m_State == CharacterState.Run)
        {
            m_State = CharacterState.Jump;
            // add moving ground velocity Y to jump speed, but NOT full rigidbody velocity from last frame
            // (this means you'll jump higher from platform moving upward, but not from an ascending slope to avoid
            // crazy jumps from stairs)
            float externalVelocityYContribution = ComputeLocalToWorldVelocityContribution().y;
            // if on a platform moving fast downward, velocity y may be negative and it's unsatisfying, so clamp
            // jump speed to some minimum
            float resultingJumpSpeed = Mathf.Max(minResultingJumpSpeed, externalVelocityYContribution + jumpSpeed);
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, resultingJumpSpeed);

#if DEBUG_CHARACTER_RUN
            Debug.LogFormat(this, "[CharacterRun] #{0} Jump with jumpSpeed: {1}", playerNumber, jumpSpeed);
#endif
        }
    }
    
    /// Input callback: Move action
    public void OnMove(float value)
    {
        if (m_CanControl)
        {
            // update brake intention in every state
            // this is because OnMove is only called on value change (-1 when holding left, 0 when releasing)
            // so if the player holds left even before race start we don't want to miss that input for later
            m_BrakeIntention = value < 0f;
        }
    }
    
#if DEBUG_CHARACTER_RUN && (UNITY_EDITOR || DEVELOPMENT_BUILD)
    private static readonly GUIStyle GuiStyle = new GUIStyle();

    private void OnGUI()
    {
        GuiStyle.fontSize = 48;
        GuiStyle.normal.textColor = playerNumber == 1 ? Color.red : Color.yellow;
        GUILayout.Label($"Runner #{playerNumber} state: {m_State}", GuiStyle);
        GUILayout.Label($"Runner #{playerNumber} speed X: {m_Rigidbody2D.velocity.x}", GuiStyle);
    }
#endif
}
