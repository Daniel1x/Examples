using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class UnitCharacterController : UnitAnimationEventReceiver
{
    [System.Serializable]
    public class CharacterAudioSettings
    {
        public AudioClip LandingAudioClip = null;
        public AudioClip[] FootstepAudioClips = new AudioClip[] { };
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        public void OnFootstep(AnimationEvent _animationEvent, Transform _transform, CharacterController _controller)
        {
            if (FootstepAudioClips.GetRandom() is AudioClip _clip)
            {
                playClip(_animationEvent, _transform, _controller, LandingAudioClip);
            }
        }

        public void OnLand(AnimationEvent _animationEvent, Transform _transform, CharacterController _controller)
        {
            playClip(_animationEvent, _transform, _controller, LandingAudioClip);
        }

        private void playClip(AnimationEvent _animationEvent, Transform _transform, CharacterController _controller, AudioClip _clip)
        {
            if (_animationEvent.animatorClipInfo.weight <= 0.5f
                || _transform == null
                || _controller == null
                || _clip == null)
            {
                return;
            }

            AudioSource.PlayClipAtPoint(_clip, _transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }
    }

    public event UnityAction<UnitCharacterController> OnJumpPerformed = null;

    [Header("Player")]
    [SerializeField] protected float moveSpeed = 2.0f;
    [SerializeField] protected float sprintSpeed = 5.335f;
    [SerializeField, Range(0.0f, 0.3f)] protected float rotationSmoothTime = 0.12f;
    [SerializeField] protected float speedChangeRate = 10.0f;

    [Header("Audio")]
    [SerializeField] protected CharacterAudioSettings characterAudioSettings = new CharacterAudioSettings();

    [Header("Movement")]
    [SerializeField] protected float jumpHeight = 1.2f;
    [SerializeField] protected float gravity = -15.0f;
    [SerializeField] protected float jumpTimeout = 0.50f;
    [SerializeField] protected float fallTimeout = 0.15f;

    [Header("Player Grounded")]
    [SerializeField] protected bool grounded = true;
    [SerializeField] protected float groundedOffset = -0.14f;
    [SerializeField] protected float groundedRadius = 0.28f;
    [SerializeField] protected LayerMask groundLayers = default;

    [Header("Inputs")]
    [ReadOnlyProperty] public CharacterInputProvider InputProvider = null;

    protected float speed = 0f;
    protected float animationBlend = 0f;
    protected float targetRotation = 0f;
    protected float rotationVelocity = 0f;
    protected float verticalVelocity = 0f;
    protected float terminalVelocity = 53f;

    protected float jumpTimeoutDelta = 0f;
    protected float fallTimeoutDelta = 0f;

    protected bool hasAnimator = false;
    protected int animIDSpeed = 0;
    protected int animIDGrounded = 0;
    protected int animIDJump = 0;
    protected int animIDFreeFall = 0;
    protected int animIDMotionSpeed = 0;

    protected Animator animator = null;
    protected CharacterController controller = null;

    protected virtual void Awake()
    {
        if (InputProvider == null)
        {
            InputProvider = GetComponent<CharacterInputProvider>();
        }
    }

    protected virtual void Start()
    {
        hasAnimator = TryGetComponent(out animator);

        if (hasAnimator == false)
        {
            animator = gameObject.GetComponentInChildren<Animator>(true);
            hasAnimator = animator != null;
        }

        controller = GetComponent<CharacterController>();

        animIDSpeed = Animator.StringToHash("Speed");
        animIDGrounded = Animator.StringToHash("Grounded");
        animIDJump = Animator.StringToHash("Jump");
        animIDFreeFall = Animator.StringToHash("FreeFall");
        animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

        jumpTimeoutDelta = jumpTimeout;
        fallTimeoutDelta = fallTimeout;
    }

    protected virtual void Update()
    {
        if (InputProvider == null)
        {
            return;
        }

        jumpAndGravity();
        groundedCheck();
        move();
    }

    private void groundedCheck()
    {
        Vector3 _spherePosition = transform.position;
        _spherePosition.y -= groundedOffset;

        grounded = Physics.CheckSphere(_spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);

        if (hasAnimator)
        {
            animator.SetBool(animIDGrounded, grounded);
        }
    }

    protected void move()
    {
        float _targetSpeed = InputProvider.Sprint ? sprintSpeed : moveSpeed;
        float _dt = Time.deltaTime;

        if (InputProvider.Move == Vector2.zero)
        {
            _targetSpeed = 0f;
        }

        float _currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;
        float _speedOffset = 0.1f;
        float _inputMagnitude = InputProvider.AnalogMovement ? InputProvider.Move.magnitude : 1f;

        if (_currentHorizontalSpeed < _targetSpeed - _speedOffset
            || _currentHorizontalSpeed > _targetSpeed + _speedOffset)
        {
            speed = Mathf.Lerp(_currentHorizontalSpeed, _targetSpeed * _inputMagnitude, _dt * speedChangeRate);
            speed = Mathf.Round(speed * 1000f) / 1000f;
        }
        else
        {
            speed = _targetSpeed;
        }

        animationBlend = Mathf.Lerp(animationBlend, _targetSpeed, _dt * speedChangeRate);

        if (animationBlend < 0.01f)
        {
            animationBlend = 0f;
        }

        if (InputProvider.Move != Vector2.zero)
        {
            targetRotation = getTargetRotation(InputProvider.Move);

            float _rotationY = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);

            transform.rotation = Quaternion.Euler(0.0f, _rotationY, 0.0f);
        }

        Vector3 _targetDirection = (Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward).normalized;
        _targetDirection = adjustTargetMovementDirection(_targetDirection);

        controller.Move((_targetDirection * (speed * _dt)) + new Vector3(0f, verticalVelocity * _dt, 0f));

        if (hasAnimator)
        {
            animator.SetFloat(animIDSpeed, animationBlend);
            animator.SetFloat(animIDMotionSpeed, _inputMagnitude);
        }
    }

    protected virtual float getTargetRotation(Vector2 _inputs)
    {
        float _rotation = Mathf.Atan2(_inputs.x, _inputs.y) * Mathf.Rad2Deg;

        return _rotation;
    }

    protected virtual Vector3 adjustTargetMovementDirection(Vector3 _targetDirection)
    {
        return _targetDirection;
    }

    protected void jumpAndGravity()
    {
        if (grounded)
        {
            fallTimeoutDelta = fallTimeout;

            if (hasAnimator)
            {
                animator.SetBool(animIDJump, false);
                animator.SetBool(animIDFreeFall, false);
            }

            if (verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (InputProvider.Jump && jumpTimeoutDelta <= 0f)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                if (hasAnimator)
                {
                    animator.SetBool(animIDJump, true);
                }

                OnJumpPerformed?.Invoke(this);
            }

            if (jumpTimeoutDelta >= 0f)
            {
                jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            jumpTimeoutDelta = jumpTimeout;

            if (fallTimeoutDelta >= 0.0f)
            {
                fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                if (hasAnimator)
                {
                    animator.SetBool(animIDFreeFall, true);
                }
            }

            InputProvider.Jump = false;
        }

        if (verticalVelocity < terminalVelocity)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    public override void OnFootstep(AnimationEvent _animationEvent) => characterAudioSettings.OnFootstep(_animationEvent, transform, controller);
    public override void OnLand(AnimationEvent _animationEvent) => characterAudioSettings.OnLand(_animationEvent, transform, controller);
}
