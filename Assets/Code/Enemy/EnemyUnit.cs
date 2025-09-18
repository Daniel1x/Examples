using DL.Structs;
using UnityEngine;

[RequireComponent(typeof(UnitCharacterController), typeof(UnitStats))]
public class EnemyUnit : CharacterInputProvider
{
    [SerializeField] private float targetUpdateInterval = 0.5f;
    [SerializeField] private Transform target = null;
    [SerializeField, Min(0.1f)] private float minDistance = 1f;

    [Header("Attack")]
    [SerializeField, Min(0f)] private float throwManaCost = 5f;
    [SerializeField, Min(0f)] private float attackRange = 2f;
    [SerializeField, MinMaxSlider(0f, 20f)] private MinMax throwAttackRange = new MinMax(3f, 10f);
    [SerializeField, Min(0f)] private float attackCooldown = 2f;
    [SerializeField] private ActionType[] availableAttacks = new ActionType[0];

    private UnitCharacterController controller = null;
    private UnitStats unitStats = null;

    private Vector2 move = default;
    private bool jump = false;

    private float lastTargetUpdate = 0f;
    private float attackCooldownTimer = 0f;

    public override Vector2 Move => move;
    public override bool Jump { get => jump; set => jump = value; }
    public override bool Sprint => true;
    public override bool AnalogMovement => false;
    public override bool ChangeRightArmWeapon { get => false; set { } }
    public override bool ChangeLeftArmWeapon { get => false; set { } }

    private void Awake()
    {
        controller = GetComponent<UnitCharacterController>();
        unitStats = GetComponent<UnitStats>();
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

        ActionType _action;

        if (_canThrow && (unitStats == null || unitStats.CanUseMana(throwManaCost)))
        {
            _action = ActionType.ThrowAttack;
        }
        else
        {
            _action = availableAttacks != null && availableAttacks.Length > 0
                ? availableAttacks.GetRandom()
                : ActionType.BothHandsAttack;
        }

        controller.Attack(_action);
        attackCooldownTimer = attackCooldown;
    }

    [ActionButton] public void StartJump() => jump = true;
}
