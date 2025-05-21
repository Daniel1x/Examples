using UnityEngine;

public class AttackBehaviour : StateMachineBehaviour
{
    public bool IsInProgress { get; private set; } = false;

    public override void OnStateEnter(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
    {
        _animator.SetFloat("AttackType", Random.Range(0, 3));

        IsInProgress = true;
    }

    public override void OnStateExit(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
    {
        IsInProgress = false;
    }
}
