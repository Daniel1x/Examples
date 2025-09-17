using UnityEngine;
using UnityEngine.Events;

public class ActionBehaviour : StateMachineBehaviour
{
    public enum ActionType
    {
        RightHandAttack = 0,
        LeftHandAttack = 1,
        BothHandsAttack = 2,
        PunchAttack = 3,
        ThrowAttack = 4,
        AreaSpell = 5,
        RightSlash = 6,
        LeftSlash = 7,
    }

    public event UnityAction<ActionBehaviour> OnActionStateUpdate = null;

    [SerializeField] private bool canMoveDuringThisAnimation = true;
    [SerializeField] private AnimationCurve weightCurve = CreateDefaultWeightCurve();
    [SerializeField] private AttackSide canApplyMeleeDamage = AttackSide.None;
    [SerializeField] private AnimationCurve damageApplyCurve = AnimationCurve.Constant(0f, 1f, 0f);

    public bool IsInProgress { get; protected set; } = false;
    public AttackSide CanApplyMeleeDamage { get; protected set; } = AttackSide.None;
    public bool CanMove => canMoveDuringThisAnimation;

    public override void OnStateEnter(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
    {
        IsInProgress = true;
        updateCanApplyMeleeDamage(0f);
        OnActionStateUpdate?.Invoke(this);
    }

    public override void OnStateUpdate(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
    {
        float _animationProgress = Mathf.Clamp01(_stateInfo.normalizedTime);
        float _weight = weightCurve.Evaluate(_animationProgress);

        if (_animator.GetLayerWeight(_layerIndex) != _weight)
        {
            _animator.SetLayerWeight(_layerIndex, _weight);
        }

        if (updateCanApplyMeleeDamage(_animationProgress))
        {
            OnActionStateUpdate?.Invoke(this);
        }
    }

    public override void OnStateExit(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
    {
        _animator.SetLayerWeight(_layerIndex, 0f);
        IsInProgress = false;
        updateCanApplyMeleeDamage(0f);
        OnActionStateUpdate?.Invoke(this);
    }

    private bool updateCanApplyMeleeDamage(float _animationProgress)
    {
        if (canApplyMeleeDamage is AttackSide.None)
        {
            return false;
        }

        AttackSide _newState = _animationProgress > 0f && _animationProgress < 1f && damageApplyCurve.Evaluate(_animationProgress) > 0f
            ? canApplyMeleeDamage
            : AttackSide.None;

        if (CanApplyMeleeDamage != _newState)
        {
            CanApplyMeleeDamage = _newState;
            return true;
        }

        return false;
    }

    public static AnimationCurve CreateDefaultWeightCurve(float _riseTime = 0.1f, float _fallTime = 0.9f)
    {
        AnimationCurve _curve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(_riseTime, 1f),
            new Keyframe(_fallTime, 1f),
            new Keyframe(1f, 0f));

        return _curve;
    }
}
