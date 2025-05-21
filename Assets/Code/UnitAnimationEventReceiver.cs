using UnityEngine;
using UnityEngine.Events;

public class UnitAnimationEventReceiver : MonoBehaviour
{
    public event UnityAction<AnimationEvent> OnFootstepEvent = null;
    public event UnityAction<AnimationEvent> OnLandEvent = null;

    public virtual void OnFootstep(AnimationEvent _animationEvent) => OnFootstepEvent?.Invoke(_animationEvent);
    public virtual void OnLand(AnimationEvent _animationEvent) => OnLandEvent?.Invoke(_animationEvent);
}
