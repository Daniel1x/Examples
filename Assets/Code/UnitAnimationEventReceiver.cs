using UnityEngine;

public class UnitAnimationEventReceiver : MonoBehaviour
{
    public virtual void OnFootstep(AnimationEvent _animationEvent) {  }
    public virtual void OnLand(AnimationEvent _animationEvent) { }
}
