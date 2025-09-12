using UnityEngine;

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

    [SerializeField] private bool isMovementAlowed = true;
    [SerializeField] private AnimationCurve weightCurve = CreateDefaultCurve();

    public bool IsMovementAllowed => isMovementAlowed;
    public bool IsInProgress { get; private set; } = false;

    public override void OnStateEnter(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
    {
        IsInProgress = true;
    }

    public override void OnStateUpdate(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
    {
        float _animationProgress = _stateInfo.normalizedTime % 1f;
        float _weight = weightCurve.Evaluate(_animationProgress);

        if (_animator.GetLayerWeight(_layerIndex) != _weight)
        {
            _animator.SetLayerWeight(_layerIndex, _weight);
        }
    }

    public override void OnStateExit(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
    {
        IsInProgress = false;
        _animator.SetLayerWeight(_layerIndex, 0f);
    }

    public static AnimationCurve CreateDefaultCurve(float _riseTime = 0.1f, float _fallTime = 0.9f)
    {
        AnimationCurve _curve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(_riseTime, 1f),
            new Keyframe(_fallTime, 1f),
            new Keyframe(1f, 0f));

        return _curve;
    }
}
