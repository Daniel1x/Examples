using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActionStateHandler : IDisposable
{
    public event UnityAction<ActionStateHandler> OnActionStateChanged = null;

    private Animator animator = null;
    private ActionBehaviour[] allActions = null;
    private List<ActionBehaviour> ongoingActions = null;

    public bool CanMove { get; private set; } = true;
    public AttackSide CanApplyMeleeDamage { get; private set; } = AttackSide.None;
    public bool IsAnyActionInProgress => ongoingActions != null && ongoingActions.Count > 0;

    public ActionStateHandler(Animator _animator)
    {
        animator = _animator;
        allActions = animator.GetBehaviours<ActionBehaviour>();

        int _actionsCount = allActions.Length;

        ongoingActions = new List<ActionBehaviour>(_actionsCount);

        for (int i = 0; i < _actionsCount; i++)
        {
            allActions[i].OnActionStateUpdate += onActionTriggered;
        }
    }

    ~ActionStateHandler()
    {
        Dispose();
    }

    private void onActionTriggered(ActionBehaviour _behaviour)
    {
        if (_behaviour.IsInProgress)
        {
            if (!ongoingActions.Contains(_behaviour))
            {
                ongoingActions.Add(_behaviour);
            }
        }
        else
        {
            if (ongoingActions.Contains(_behaviour))
            {
                ongoingActions.Remove(_behaviour);
            }
        }

        bool _canMove = true;
        AttackSide _canApplyMeleeDamage = AttackSide.None;
        int _ongoingCount = ongoingActions.Count;

        for (int i = 0; i < _ongoingCount; i++)
        {
            if (_canMove && ongoingActions[i].CanMove == false)
            {
                _canMove = false;
            }

            _canApplyMeleeDamage |= ongoingActions[i].CanApplyMeleeDamage;
        }

        if (_canMove != CanMove || _canApplyMeleeDamage != CanApplyMeleeDamage)
        {
            CanMove = _canMove;
            CanApplyMeleeDamage = _canApplyMeleeDamage;

            OnActionStateChanged?.Invoke(this);
        }
    }

    public void Dispose()
    {
        if (allActions != null)
        {
            int _actionsCount = allActions.Length;

            for (int i = 0; i < _actionsCount; i++)
            {
                allActions[i].OnActionStateUpdate -= onActionTriggered;
            }

            allActions = null;
        }

        if (ongoingActions != null)
        {
            ongoingActions.Clear();
            ongoingActions = null;
        }

        animator = null;
        OnActionStateChanged = null;

        GC.SuppressFinalize(this);
    }
}
