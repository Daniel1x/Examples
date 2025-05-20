using UnityEngine;

public class EnemyUnit : CharacterInputProvider
{
    [SerializeField] private Transform target = null;
    [SerializeField] private AnimationCurve sprintIntervals = AnimationCurve.Constant(0f, 10f, 1f);
    [SerializeField] private float sprintThreshold = 0.5f;

    private Vector2 move = default;
    private bool jump = false;
    private bool sprint = false;
    private bool sprintOverride = false;

    private float lifeTime = 0f;

    public override Vector2 Move => move;
    public override bool Jump { get => jump; set => jump = value; }
    public override bool Sprint => sprint || sprintOverride;
    public override bool AnalogMovement => false;

    private void Awake()
    {
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
            move = new Vector2(_direction.x, _direction.z);

            if (move.x != 0f || move.y != 0f)
            {
                move = move.normalized;
            }
        }

        lifeTime += Time.deltaTime;
        float _duration = sprintIntervals[sprintIntervals.length - 1].time;
        float _currentTime = lifeTime % _duration;
        float _sprintValue = sprintIntervals.Evaluate(_currentTime);
        sprint = _sprintValue > sprintThreshold;
    }

    private void LateUpdate()
    {
        if (jump)
        {
            jump = false; //Clear jump state after use
        }
    }

    [ActionButton] public void StartSprint() => sprintOverride = true;
    [ActionButton] public void StopSprint() => sprintOverride = false;
    [ActionButton] public void StartJump() => jump = true;
}
