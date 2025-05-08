using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class ThirdPersonController : MonoBehaviour
{
    public event UnityAction<ThirdPersonController> OnJumpPerformed = null;

    private const float LOOK_THRESHOLD = 0.01f;

    [Header("Player")]
    public float MoveSpeed = 2.0f;
    public float SprintSpeed = 5.335f;
    [Range(0.0f, 0.3f)] public float RotationSmoothTime = 0.12f;
    public float SpeedChangeRate = 10.0f;

    [Header("Audio")]
    public AudioClip LandingAudioClip = null;
    public AudioClip[] FootstepAudioClips = new AudioClip[] { };
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Header("Movement")]
    public float JumpHeight = 1.2f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")] public float Gravity = -15.0f;
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")] public float JumpTimeout = 0.50f;
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")] public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")] public bool Grounded = true;
    [Tooltip("Useful for rough ground")] public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")] public float GroundedRadius = 0.28f;
    [Tooltip("What layers the character uses as ground")] public LayerMask GroundLayers = default;

    private float speed = 0f;
    private float animationBlend = 0f;
    private float targetRotation = 0f;
    private float rotationVelocity = 0f;
    private float verticalVelocity = 0f;
    private float terminalVelocity = 53f;

    private float jumpTimeoutDelta = 0f;
    private float fallTimeoutDelta = 0f;

    private bool hasAnimator = false;
    private int animIDSpeed = 0;
    private int animIDGrounded = 0;
    private int animIDJump = 0;
    private int animIDFreeFall = 0;
    private int animIDMotionSpeed = 0;

    private Animator animator = null;
    private CharacterController controller = null;
    private BasicInputs input = null;
    private GameObject playerCamera = null;
    private CameraTargetProvider cameraTargetProvider = null;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = gameObject.GetComponentInChildren<Camera>()?.gameObject;
        }

        if (cameraTargetProvider == null)
        {
            cameraTargetProvider = gameObject.GetComponent<CameraTargetProvider>();
        }
    }

    private void Start()
    {
        hasAnimator = TryGetComponent(out animator);
        controller = GetComponent<CharacterController>();
        input = GetComponent<BasicInputs>();

        animIDSpeed = Animator.StringToHash("Speed");
        animIDGrounded = Animator.StringToHash("Grounded");
        animIDJump = Animator.StringToHash("Jump");
        animIDFreeFall = Animator.StringToHash("FreeFall");
        animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

        jumpTimeoutDelta = JumpTimeout;
        fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
        jumpAndGravity();
        groundedCheck();
        move();
    }

    private void groundedCheck()
    {
        Vector3 _spherePosition = transform.position;
        _spherePosition.y -= GroundedOffset;

        Grounded = Physics.CheckSphere(_spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

        if (hasAnimator)
        {
            animator.SetBool(animIDGrounded, Grounded);
        }
    }

    private void move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float _targetSpeed = input.Sprint ? SprintSpeed : MoveSpeed;
        float _dt = Time.deltaTime;

        if (input.Move == Vector2.zero)
        {
            _targetSpeed = 0f;
        }

        // a reference to the players current horizontal velocity
        float _currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;
        float _speedOffset = 0.1f;
        float _inputMagnitude = input.AnalogMovement ? input.Move.magnitude : 1f;

        // accelerate or decelerate to target speed
        if (_currentHorizontalSpeed < _targetSpeed - _speedOffset || _currentHorizontalSpeed > _targetSpeed + _speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            speed = Mathf.Lerp(_currentHorizontalSpeed, _targetSpeed * _inputMagnitude, _dt * SpeedChangeRate);

            // round speed to 3 decimal places
            speed = Mathf.Round(speed * 1000f) / 1000f;
        }
        else
        {
            speed = _targetSpeed;
        }

        animationBlend = Mathf.Lerp(animationBlend, _targetSpeed, _dt * SpeedChangeRate);

        if (animationBlend < 0.01f)
        {
            animationBlend = 0f;
        }

        // normalise input direction
        Vector3 _inputDirection = new Vector3(input.Move.x, 0.0f, input.Move.y).normalized;

        GameObject _camObject = cameraTargetProvider != null && cameraTargetProvider.VirtualCamera != null
            ? cameraTargetProvider.VirtualCamera.gameObject
            : playerCamera;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (input.Move != Vector2.zero && _camObject != null)
        {
            targetRotation = Mathf.Atan2(_inputDirection.x, _inputDirection.z) * Mathf.Rad2Deg + _camObject.transform.eulerAngles.y;

            float _rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, _rotation, 0.0f);
        }

        Vector3 _targetDirection = (Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward).normalized;

        if (cameraTargetProvider != null && cameraTargetProvider.CameraManager != null)
        {
            _targetDirection = cameraTargetProvider.CameraManager.AdjustMovementVector(transform, _targetDirection, out _);
        }

        // move the player
        controller.Move((_targetDirection * (speed * _dt)) + new Vector3(0f, verticalVelocity * _dt, 0f));

        // update animator if using character
        if (hasAnimator)
        {
            animator.SetFloat(animIDSpeed, animationBlend);
            animator.SetFloat(animIDMotionSpeed, _inputMagnitude);
        }
    }

    private void jumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            fallTimeoutDelta = FallTimeout;

            // update animator if using character
            if (hasAnimator)
            {
                animator.SetBool(animIDJump, false);
                animator.SetBool(animIDFreeFall, false);
            }

            // stop our velocity dropping infinitely when grounded
            if (verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            // Jump
            if (input.Jump && jumpTimeoutDelta <= 0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                // update animator if using character
                if (hasAnimator)
                {
                    animator.SetBool(animIDJump, true);
                }

                OnJumpPerformed?.Invoke(this);
            }

            // jump timeout
            if (jumpTimeoutDelta >= 0f)
            {
                jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (fallTimeoutDelta >= 0.0f)
            {
                fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                if (hasAnimator)
                {
                    animator.SetBool(animIDFreeFall, true);
                }
            }

            // if we are not grounded, do not jump
            input.Jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (verticalVelocity < terminalVelocity)
        {
            verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private float clampAngle(float _angle, float _min, float _max)
    {
        return Mathf.Clamp(_angle.Clamp360(), _min, _max);
    }

    protected void OnFootstep(AnimationEvent _animationEvent)
    {
        if (_animationEvent.animatorClipInfo.weight <= 0.5f)
        {
            return;
        }

        if (FootstepAudioClips.GetRandom() is AudioClip _clip)
        {
            AudioSource.PlayClipAtPoint(_clip, transform.TransformPoint(controller.center), FootstepAudioVolume);
        }
    }

    protected void OnLand(AnimationEvent _animationEvent)
    {
        if (_animationEvent.animatorClipInfo.weight <= 0.5f)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(controller.center), FootstepAudioVolume);
    }
}
