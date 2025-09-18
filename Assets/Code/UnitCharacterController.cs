using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnitCharacterController : UnitCharacterController<CharacterInputProvider>
{
    // This class is just a non-generic version of UnitCharacterController<T> for convenience.
}

[RequireComponent(typeof(CharacterController), typeof(UnitStats))]
public abstract class UnitCharacterController<T> : UnitAnimationEventReceiver, IRequiresCharacterInputProvider<T> where T : CharacterInputProvider
{
    [System.Serializable]
    public class CharacterAudioSettings
    {
        private static Transform parent = null;

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

            PlayClipAtPoint(_clip, _transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }

        public static void PlayClipAtPoint(AudioClip _clip, Vector3 _position, float _volume = 1f)
        {
            if (parent == null)
            {
                parent = new GameObject("One shot audio parent").transform;
                parent.ResetTransformValues();
            }

            GameObject _gameObject = new GameObject("One shot audio");

            _gameObject.transform.SetParent(parent);
            _gameObject.transform.position = _position;

            AudioSource _audioSource = _gameObject.AddComponent<AudioSource>();

            _audioSource.clip = _clip;
            _audioSource.spatialBlend = 1f;
            _audioSource.volume = _volume;
            _audioSource.Play();

            UnityEngine.Object.Destroy(_gameObject, _clip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
        }
    }

    public event UnityAction<UnitCharacterController<T>> OnJumpPerformed = null;

    [Header("Player")]
    [SerializeField] protected float moveSpeed = 2.0f;
    [SerializeField] protected float sprintSpeed = 5.335f;
    [SerializeField, Range(0.0f, 0.3f)] protected float rotationSmoothTime = 0.12f;
    [SerializeField] protected float speedChangeRate = 10.0f;

    [Header("Audio")]
    [SerializeField] protected UnitAnimationEventReceiver customEventReceiver = null;
    [SerializeField] protected CharacterAudioSettings characterAudioSettings = new CharacterAudioSettings();

    [Header("Movement")]
    [SerializeField] protected float jumpHeight = 1.2f;
    [SerializeField] protected float gravity = -15.0f;
    [SerializeField] protected float jumpTimeout = 0.50f;
    [SerializeField] protected float fallTimeout = 0.15f;

    [Header("Attack")]
    [SerializeField] protected float attackRange = 0.25f;
    [SerializeField] protected LayerMask attackLayerMask = default;
    [SerializeField, Min(0f)] protected float handDamage = 1f;

    [Header("Player Grounded")]
    [SerializeField] protected bool grounded = true;
    [SerializeField] protected float groundedOffset = -0.14f;
    [SerializeField] protected float groundedRadius = 0.28f;
    [SerializeField] protected LayerMask groundLayers = default;

    [Header("Debug")]
    [SerializeField, ReadOnlyProperty] public bool CollidedSides = false;
    [SerializeField, ReadOnlyProperty] public bool CollidedAbove = false;
    [SerializeField, ReadOnlyProperty] public bool CollidedBelow = false;

    protected readonly Dictionary<string, Transform> characterSocketsForAttacks = new();

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
    protected ActionStateHandler actionStateHandler = null;
    protected CharacterController controller = null;
    protected UnitEquipmentManager equipmentManager = null;
    protected UnitStats characterStats = null;

    protected bool sprintBlocked = false;
    protected virtual bool isSprinting
    {
        get
        {
            if (InputProvider.Sprint == false)
            {
                return false; // No inputs
            }

            if (characterStats == null)
            {
                return true; // No need to check stats
            }

            if (sprintBlocked)
            {
                if (characterStats.IsStaminaAboveThreshold)
                {
                    sprintBlocked = false;
                }
                else
                {
                    return false; // Wait for stamina to regenerate
                }
            }

            float _requiredStamina = characterStats.StaminaRunCost * Time.deltaTime;

            if (characterStats.CanUseStamina(_requiredStamina))
            {
                return true;
            }
            else
            {
                sprintBlocked = true;
                return false; // Wait for stamina to regenerate
            }
        }
    }

    public ActionType TriggeredAction { get; private set; }
    public T InputProvider { get; set; }

    protected virtual void Awake()
    {
        if (InputProvider == null)
        {
            InputProvider = GetComponent<T>();
        }

        controller = GetComponent<CharacterController>();
        equipmentManager = GetComponent<UnitEquipmentManager>();
        characterStats = GetComponent<UnitStats>();

        if (customEventReceiver != null)
        {
            customEventReceiver.RegisterCustomReceiver(this);
        }
    }

    protected virtual void OnDestroy()
    {
        if (customEventReceiver != null)
        {
            customEventReceiver.UnregisterCustomReceiver(this);
        }

        if (actionStateHandler != null)
        {
            actionStateHandler.Dispose();
            actionStateHandler = null;
        }

        characterSocketsForAttacks.Clear();
    }

    protected virtual void Start()
    {
        hasAnimator = TryGetComponent(out animator);

        if (hasAnimator == false)
        {
            animator = gameObject.GetComponentInChildren<Animator>(true);
            hasAnimator = animator != null;
        }

        if (animator != null)
        {
            actionStateHandler = new ActionStateHandler(animator);
        }

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

        handleEquipmentChange();
        jumpAndGravity();
        groundedCheck();
        move();
    }

    private void handleEquipmentChange()
    {
        if (equipmentManager == null)
        {
            return;
        }

        if (InputProvider.ChangeRightArmWeapon)
        {
            equipmentManager.ChangeRightArmEquipmentToNext();
            InputProvider.ChangeRightArmWeapon = false;
        }

        if (InputProvider.ChangeLeftArmWeapon)
        {
            equipmentManager.ChangeLeftArmEquipmentToNext();
            InputProvider.ChangeLeftArmWeapon = false;
        }
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
        bool _isAnyActionInProgress = IsAnyActionBehaviourInProgress(out bool _allowMovement);
        float _targetSpeed = isSprinting ? sprintSpeed : moveSpeed;
        float _dt = Time.deltaTime;

        if (InputProvider.Move == Vector2.zero || !_allowMovement)
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

        Vector3 _targetDirection;

        if (_isAnyActionInProgress && _allowMovement == false)
        {
            _targetDirection = Vector3.zero;
        }
        else
        {
            _targetDirection = (Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward).normalized;
            _targetDirection = adjustTargetMovementDirection(_targetDirection);
        }

        CollisionFlags _collision = controller.Move((_targetDirection * (speed * _dt)) + new Vector3(0f, verticalVelocity * _dt, 0f));

        CollidedSides = (_collision & CollisionFlags.Sides) != 0;
        CollidedAbove = (_collision & CollisionFlags.Above) != 0;
        CollidedBelow = (_collision & CollisionFlags.Below) != 0;

        if (hasAnimator)
        {
            animator.SetFloat(animIDSpeed, animationBlend);
            animator.SetFloat(animIDMotionSpeed, _inputMagnitude);
        }
    }

    public bool IsAnyActionBehaviourInProgress(out bool _allowMovement)
    {
        if (actionStateHandler != null)
        {
            _allowMovement = actionStateHandler.CanMove;
            return actionStateHandler.IsAnyActionInProgress;
        }

        _allowMovement = true;
        return false;
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

                InputProvider.Jump = false;
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

    public void Attack(ActionType _type)
    {
        if (IsAnyActionBehaviourInProgress(out _) == false)
        {
            TriggeredAction = _type;

            if (hasAnimator == false || animator == null)
            {
                return;
            }

            animator.SetFloat("AttackType", (float)_type);
            animator.SetTrigger("Attack");
        }
    }

    public override void OnFootstep(AnimationEvent _animationEvent) => characterAudioSettings.OnFootstep(_animationEvent, transform, controller);
    public override void OnLand(AnimationEvent _animationEvent) => characterAudioSettings.OnLand(_animationEvent, transform, controller);
    public override void OnCustom(AnimationEventData _data)
    {
        foreach (AnimationEventData.Action _action in _data.Actions)
        {
            handleAction(_action);
        }
    }

    private void handleAction(AnimationEventData.Action _action)
    {
        switch (_action.ActionType)
        {
            case ActionType.RightHandAttack:

                if (equipmentManager != null && equipmentManager.RightArmEquipment != null)
                {
                    equipmentManager.RightArmEquipment.UseEquipment(_action);
                }

                break;

            case ActionType.LeftHandAttack:

                if (equipmentManager != null && equipmentManager.LeftArmEquipment != null)
                {
                    equipmentManager.LeftArmEquipment.UseEquipment(_action);
                }

                break;

            case ActionType.BothHandsAttack:

                if (equipmentManager != null)
                {
                    if (equipmentManager.RightArmEquipment != null)
                    {
                        equipmentManager.RightArmEquipment.UseEquipment(_action);
                    }

                    if (equipmentManager.LeftArmEquipment != null)
                    {
                        equipmentManager.LeftArmEquipment.UseEquipment(_action);
                    }
                }

                break;

            case ActionType.PunchAttack:
                break;

            case ActionType.ThrowAttack:

                Transform _throwSocket = findRequiredSocket(_action.SocketName);

                if (_throwSocket != null && ProjectilesManager.Instance != null)
                {
                    Vector3 _targetPosition = _throwSocket.position + (transform.forward.WithY(0f).normalized * 10f);

                    ProjectilesManager.Instance.SpawnGrenade(_throwSocket.position, _targetPosition);
                }

                break;

            case ActionType.AreaSpell:
                break;

            case ActionType.RightSlash:

                if (equipmentManager != null && equipmentManager.RightArmEquipment != null)
                {
                    equipmentManager.RightArmEquipment.UseEquipment(_action);
                }

                break;

            case ActionType.LeftSlash:

                if (equipmentManager != null && equipmentManager.LeftArmEquipment != null)
                {
                    equipmentManager.LeftArmEquipment.UseEquipment(_action);
                }

                break;
        }
    }

    protected Transform findRequiredSocket(string _socketName)
    {
        if (_socketName.IsNullEmptyOrWhitespace())
        {
            return null;
        }

        if (characterSocketsForAttacks.TryGetValue(_socketName, out Transform _socket) == false)
        {
            _socket = transform.FindChildTransformWithName(_socketName);

            if (_socket == null)
            {
                _socket = transform.FindChildTransformWithSimilarName(_socketName);
            }

            // Cache the result (even if null to avoid repeated searches)
            characterSocketsForAttacks.Add(_socketName, _socket);
        }

        return _socket;
    }
}
