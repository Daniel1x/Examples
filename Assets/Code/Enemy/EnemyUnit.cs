using DL.Structs;
using UnityEngine;

public class EnemyUnit : CharacterInputProvider
{
    [SerializeField] private float targetUpdateInterval = 0.5f;
    [SerializeField] private Transform target = null;
    [SerializeField] private AnimationCurve sprintIntervals = AnimationCurve.Constant(0f, 10f, 1f);
    [SerializeField] private float sprintThreshold = 0.5f;
    [SerializeField, Min(0.1f)] private float minDistance = 1f;

    [Header("Attack")]
    [SerializeField, Min(0f)] private float attackRange = 2f;
    [SerializeField, MinMaxSlider(0f, 20f)] private MinMax throwAttackRange = new MinMax(3f, 10f);
    [SerializeField, Min(0f)] private float attackCooldown = 2f;
    [SerializeField] private ActionBehaviour.ActionType[] availableAttacks = new ActionBehaviour.ActionType[0];

    private UnitCharacterController controller = null;

    private Vector2 move = default;
    private bool jump = false;
    private bool sprint = false;
    private bool sprintOverride = false;

    private float lifeTime = 0f;
    private float lastTargetUpdate = 0f;
    private float attackCooldownTimer = 0f;

    public override Vector2 Move => move;
    public override bool Jump { get => jump; set => jump = value; }
    public override bool Sprint => sprint || sprintOverride;
    public override bool AnalogMovement => false;

    private void Awake()
    {
        controller = GetComponent<UnitCharacterController>();
        lifeTime = UnityEngine.Random.Range(0f, 1000f);
    }

    private void Update()
    {
        if (target == null)
        {
            move = Vector2.zero;
        }
        else
        {
            Vector3 _direction = transform.DirectionTo(target);
            _direction.y = 0f; // Ignore vertical direction
            float _distance = _direction.magnitude;

            move = _distance < minDistance
                ? Vector2.zero
                : new Vector2(_direction.x, _direction.z) / _distance;

            tryTriggerAttack(_distance);
        }

        lifeTime += Time.deltaTime;
        float _duration = sprintIntervals[sprintIntervals.length - 1].time;
        float _currentTime = lifeTime % _duration;
        float _sprintValue = sprintIntervals.Evaluate(_currentTime);
        sprint = _sprintValue > sprintThreshold;

        checkForTargetUpdate();
    }

    private void LateUpdate()
    {
        if (jump)
        {
            jump = false; //Clear jump state after use
        }
    }

    private void OnEnable()
    {
        updateTarget();

        PlayerController.OnPlayerCountUpdated += updateTarget;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerCountUpdated -= updateTarget;
    }

    private void checkForTargetUpdate()
    {
        if (target == null || Time.time - lastTargetUpdate > targetUpdateInterval)
        {
            lastTargetUpdate = Time.time;
            updateTarget();
        }
    }

    private void updateTarget()
    {
        PlayerController _closestPlayer = PlayerController.GetClosestPlayer(transform.position);

        target = _closestPlayer != null
            ? _closestPlayer.transform
            : null;
    }

    private void tryTriggerAttack(float _distanceToTarget)
    {
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
            return;
        }

        bool _canThrow = throwAttackRange.FitsInRange(_distanceToTarget);

        if (_distanceToTarget > attackRange && _canThrow == false)
        {
            return;
        }

        if (controller == null || controller.IsAnyActionBehaviourInProgress(out _))
        {
            return;
        }

        ActionBehaviour.ActionType _action;

        if (_canThrow)
        {
            _action = ActionBehaviour.ActionType.ThrowAttack;
        }
        else
        {
            _action = availableAttacks != null && availableAttacks.Length > 0
                ? availableAttacks.GetRandom()
                : ActionBehaviour.ActionType.BothHandsAttack;
        }

        controller.Attack(_action);
        attackCooldownTimer = attackCooldown;
    }

    [ActionButton] public void StartSprint() => sprintOverride = true;
    [ActionButton] public void StopSprint() => sprintOverride = false;
    [ActionButton] public void StartJump() => jump = true;
}
