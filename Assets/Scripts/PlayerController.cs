using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Move (same top speed as the ball version: every enemy speed in the README depends on it)")]
    [SerializeField] float maxSpeed = 7f;
    [SerializeField] float groundAcceleration = 40f;
    [SerializeField] float groundDeceleration = 45f;
    [SerializeField] float moveForce = 10f;
    [SerializeField] float brake = 25f;
    [SerializeField, Range(0f, 1f)] float airControl = 0.6f;

    [Header("Jump (coyote time + input buffer)")]
    [SerializeField] float jumpSpeed = 6f;
    [SerializeField] float coyoteTime = 0.12f;
    [SerializeField] float jumpBuffer = 0.12f;
    [SerializeField] float fallGravityMultiplier = 1f;

    [Header("Ground probe (measured from the feet)")]
    [SerializeField] float groundProbeShrink = 0.05f;
    [SerializeField] float groundProbeLift = 0.1f;
    [SerializeField] float groundProbeTolerance = 0.12f;
    [SerializeField, Range(10f, 80f)] float maxWalkableSlopeDegrees = 50f;

    [Header("Ice")]
    [SerializeField] float iceDamping = 0.1f;
    [SerializeField] float iceControl = 0.5f;
    [SerializeField] float iceMaxSpeed = 7f;

    [Header("Look (optional)")]
    [SerializeField] Transform modelRoot;
    [SerializeField] float turnSpeedDegrees = 720f;

    Rigidbody rb;
    CapsuleCollider capsule;
    PlayerJuice juice;
    Vector3 input;
    bool controlsEnabled = true;

    float speedMultiplier = 1f;
    Coroutine boost;
    float boostEndTime;

    int iceContacts;
    float normalLinearDamping;
    float normalAngularDamping;
    float lastConveyorTouchTime = -10f;

    bool grounded;
    bool wasGrounded;
    Vector3 groundNormal = Vector3.up;
    float minimumGroundNormalY;
    float lastGroundedTime = -10f;
    float lastJumpPressTime = -10f;
    float lowestVerticalSpeed;

    float noControlUntil;

    public bool IsSpeedBoosted => boost != null;
    public float SpeedMultiplier => speedMultiplier;
    public float SpeedBoostSecondsRemaining => boost == null ? 0f : Mathf.Max(0f, boostEndTime - Time.time);
    public bool IsGrounded => grounded;
    public Vector3 FeetPosition
    {
        get
        {
            Bounds bounds = capsule.bounds;
            return new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        }
    }
    public float FlatSpeed
    {
        get
        {
            Vector3 v = rb != null ? rb.linearVelocity : Vector3.zero;
            return new Vector2(v.x, v.z).magnitude;
        }
    }

    float CapsuleWorldRadius
    {
        get
        {
            Vector3 scale = transform.lossyScale;
            return capsule.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        juice = GetComponent<PlayerJuice>();

        rb.sleepThreshold = 0f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearDamping = 0f;
        normalLinearDamping = rb.linearDamping;
        normalAngularDamping = rb.angularDamping;

        PhysicsMaterial slippery = new PhysicsMaterial("PlayerNoFriction")
        {
            dynamicFriction = 0f,
            staticFriction = 0f,
            bounciness = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Minimum
        };
        capsule.sharedMaterial = slippery;

        if (capsule.direction != 1)
            Debug.LogWarning("PlayerController: the CapsuleCollider Direction must be Y-Axis.", this);
        minimumGroundNormalY = Mathf.Cos(maxWalkableSlopeDegrees * Mathf.Deg2Rad);
    }

    void OnValidate() => minimumGroundNormalY = Mathf.Cos(maxWalkableSlopeDegrees * Mathf.Deg2Rad);

    void OnDisable()
    {
        speedMultiplier = 1f;
        boost = null;
        if (rb != null) rb.useGravity = true;
    }

    void Update()
    {
        if (!controlsEnabled)
        {
            input = Vector3.zero;
            return;
        }
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        input = Vector3.ClampMagnitude(new Vector3(h, 0f, v), 1f);
        if (Input.GetKeyDown(KeyCode.Space)) lastJumpPressTime = Time.time;

        TurnModelTowardInput();
    }

    void TurnModelTowardInput()
    {
        if (modelRoot == null || input.sqrMagnitude < 0.04f) return;
        Quaternion wanted = Quaternion.LookRotation(input.normalized, Vector3.up);
        modelRoot.rotation = Quaternion.RotateTowards(modelRoot.rotation, wanted, turnSpeedDegrees * Time.deltaTime);
    }

    void FixedUpdate()
    {
        UpdateGrounded();
        bool jumpedThisStep = TryJump();

        bool locked = Time.time < noControlUntil;
        bool onIce = iceContacts > 0;
        bool onConveyor = Time.time - lastConveyorTouchTime < 0.1f;
        bool hasInput = input.sqrMagnitude > 0.0001f;
        bool plainGround = grounded && !jumpedThisStep && !onIce && !onConveyor;

        if (!locked)
        {
            if (plainGround) SteerOnPlainGround(hasInput);
            else SteerWithForce(onIce);
            LimitFlatSpeed(onIce);
        }

        FollowGroundSlope(jumpedThisStep);

        bool holdStill = plainGround && !locked && !hasInput && FlatSpeed < 0.1f;
        if (holdStill)
        {
            rb.linearVelocity = Vector3.zero;
            rb.useGravity = false;
        }
        else
        {
            rb.useGravity = true;
            if (!grounded && fallGravityMultiplier > 1f && rb.linearVelocity.y < 0f)
                rb.AddForce(Physics.gravity * (fallGravityMultiplier - 1f), ForceMode.Acceleration);
        }
    }

    void SteerOnPlainGround(bool hasInput)
    {
        Vector3 v = rb.linearVelocity;
        Vector3 flatVelocity = new Vector3(v.x, 0f, v.z);
        Vector3 desiredVelocity = input * (maxSpeed * speedMultiplier);
        float acceleration = (hasInput ? groundAcceleration : groundDeceleration) * speedMultiplier;
        Vector3 newFlat = Vector3.MoveTowards(flatVelocity, desiredVelocity, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector3(newFlat.x, v.y, newFlat.z);
    }

    void SteerWithForce(bool onIce)
    {
        float control = grounded ? 1f : airControl;
        if (onIce) control *= iceControl;
        rb.AddForce(input * (moveForce * speedMultiplier * control), ForceMode.Acceleration);
    }

    void UpdateGrounded()
    {
        wasGrounded = grounded;
        grounded = false;
        groundNormal = Vector3.up;

        if (rb.linearVelocity.y < 1f)
        {
            Bounds bounds = capsule.bounds;
            float probeRadius = Mathf.Max(0.05f, CapsuleWorldRadius - groundProbeShrink);
            Vector3 probeOrigin = new Vector3(bounds.center.x, bounds.min.y + probeRadius + groundProbeLift, bounds.center.z);
            float probeDistance = groundProbeLift + groundProbeTolerance;
            if (Physics.SphereCast(probeOrigin, probeRadius, Vector3.down, out RaycastHit hit, probeDistance,
                                   ~0, QueryTriggerInteraction.Ignore) && hit.normal.y >= minimumGroundNormalY)
            {
                grounded = true;
                groundNormal = hit.normal;
            }
        }
        if (grounded) lastGroundedTime = Time.time;

        if (!grounded) lowestVerticalSpeed = Mathf.Min(lowestVerticalSpeed, rb.linearVelocity.y);
        if (grounded && !wasGrounded)
        {
            float impactSpeed = -lowestVerticalSpeed;
            if (impactSpeed > 4f)
            {
                JuiceFx.Burst(FeetPosition + Vector3.up * 0.05f, new Color(0.8f, 0.8f, 0.85f), 10, 2.5f, 0.14f, 0.35f);
                if (CameraShake.Instance != null && impactSpeed > 8f) CameraShake.Instance.Shake(0.1f, 0.12f);
            }
            if (juice != null) juice.PlayerLanded(impactSpeed);
            lowestVerticalSpeed = 0f;
        }
    }

    bool TryJump()
    {
        if (!controlsEnabled) return false;
        bool pressedRecently = Time.time - lastJumpPressTime <= jumpBuffer;
        bool groundedRecently = Time.time - lastGroundedTime <= coyoteTime;
        if (!pressedRecently || !groundedRecently) return false;

        lastJumpPressTime = -10f;
        lastGroundedTime = -10f;
        Vector3 v = rb.linearVelocity;
        rb.linearVelocity = new Vector3(v.x, jumpSpeed, v.z);
        JuiceFx.Burst(FeetPosition + Vector3.up * 0.05f, Color.white, 8, 2.5f, 0.12f, 0.35f);
        if (juice != null) juice.PlayerJumped();
        return true;
    }

    void FollowGroundSlope(bool jumpedThisStep)
    {
        if (!grounded || jumpedThisStep) return;
        Vector3 v = rb.linearVelocity;
        if (v.y > 0.1f) return;
        if (Vector3.Dot(v, groundNormal) <= 0f) return;
        float flatSpeed = new Vector2(v.x, v.z).magnitude;
        if (flatSpeed < 0.01f) return;
        Vector3 alongSlope = Vector3.ProjectOnPlane(v, groundNormal);
        float alongSlopeFlatSpeed = new Vector2(alongSlope.x, alongSlope.z).magnitude;
        if (alongSlopeFlatSpeed < 0.01f) return;
        rb.linearVelocity = alongSlope * (flatSpeed / alongSlopeFlatSpeed);
    }

    void LimitFlatSpeed(bool onIce)
    {
        float cap = onIce ? iceMaxSpeed : maxSpeed * speedMultiplier;
        Vector3 v = rb.linearVelocity;
        Vector3 flat = new Vector3(v.x, 0f, v.z);
        float speed = flat.magnitude;
        if (speed <= cap) return;
        float newSpeed = Mathf.MoveTowards(speed, cap, brake * Time.fixedDeltaTime);
        flat = flat / speed * newSpeed;
        rb.linearVelocity = new Vector3(flat.x, v.y, flat.z);
    }

    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out ConveyorBelt _)) lastConveyorTouchTime = Time.time;
    }

    public void SetControlsEnabled(bool controlsAllowed)
    {
        controlsEnabled = controlsAllowed;
        if (!controlsAllowed) input = Vector3.zero;
    }

    public void Knockback(Vector3 push, float seconds)
    {
        Vector3 v = rb.linearVelocity;
        rb.linearVelocity = new Vector3(0f, v.y, 0f);
        rb.AddForce(push, ForceMode.VelocityChange);
        noControlUntil = Time.time + seconds;
    }

    public void BoostSpeed(float multiplier, float seconds)
    {
        if (multiplier <= 0f || seconds <= 0f) return;
        if (boost != null) StopCoroutine(boost);
        boost = StartCoroutine(Boost(multiplier, seconds));
    }

    IEnumerator Boost(float multiplier, float seconds)
    {
        speedMultiplier = multiplier;
        boostEndTime = Time.time + seconds;
        if (juice != null) juice.SpeedBoostStarted();
        yield return new WaitForSeconds(seconds);
        speedMultiplier = 1f;
        boost = null;
        if (juice != null) juice.SpeedBoostEnded();
    }

    public void CancelBoost()
    {
        if (boost == null) return;
        StopCoroutine(boost);
        boost = null;
        speedMultiplier = 1f;
        if (juice != null) juice.SpeedBoostEnded();
    }

    public void EnterIce() { iceContacts++; ApplyIce(); }
    public void ExitIce() { iceContacts = Mathf.Max(0, iceContacts - 1); ApplyIce(); }

    void ApplyIce()
    {
        bool onIce = iceContacts > 0;
        rb.linearDamping = onIce ? iceDamping : normalLinearDamping;
        rb.angularDamping = onIce ? iceDamping : normalAngularDamping;
    }

}
