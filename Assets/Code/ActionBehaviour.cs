using UnityEngine;
using UnityEngine.Events;

public class ActionBehaviour : StateMachineBehaviour
{
    public event UnityAction<ActionBehaviour> OnActionStateUpdate = null;

    [SerializeField] private bool canMoveDuringThisAnimation = true;
    [SerializeField] private AnimationCurve weightCurve = CreateDefaultWeightCurve();

    public bool IsInProgress { get; protected set; } = false;
    public bool CanMove => canMoveDuringThisAnimation;

    public override void OnStateEnter(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
    {
        IsInProgress = true;
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
    }

    public override void OnStateExit(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
    {
        _animator.SetLayerWeight(_layerIndex, 0f);
        IsInProgress = false;
        OnActionStateUpdate?.Invoke(this);
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
