using UnityEngine;
using UnityEngine.Events;

public class UnitAnimationEventReceiver : MonoBehaviour
{
    public event UnityAction<AnimationEvent> OnFootstepEvent = null;
    public event UnityAction<AnimationEvent> OnLandEvent = null;
    public event UnityAction<AnimationEventData> OnCustomEvent = null;

    public virtual void OnFootstep(AnimationEvent _animationEvent) => OnFootstepEvent?.Invoke(_animationEvent);
    public virtual void OnLand(AnimationEvent _animationEvent) => OnLandEvent?.Invoke(_animationEvent);
    public virtual void OnCustom(AnimationEventData _data) => OnCustomEvent?.Invoke(_data);

    public void OnEvent(AnimationEvent _animationEvent)
    {
        if (_animationEvent != null && _animationEvent.objectReferenceParameter is AnimationEventData _data)
        {
            OnCustom(_data);
        }
    }

    public void RegisterCustomReceiver<T>(T _receiver) where T : UnitAnimationEventReceiver
    {
        if (_receiver == null || _receiver == this)
        {
            return; // Prevent self-registration or null registration
        }

        OnFootstepEvent += _receiver.OnFootstep;
        OnLandEvent += _receiver.OnLand;
        OnCustomEvent += _receiver.OnCustom;
    }

    public void UnregisterCustomReceiver<T>(T _receiver) where T : UnitAnimationEventReceiver
    {
        if (_receiver == null || _receiver == this)
        {
            return; // Prevent self-unregistration or null unregistration
        }

        OnFootstepEvent -= _receiver.OnFootstep;
        OnLandEvent -= _receiver.OnLand;
        OnCustomEvent -= _receiver.OnCustom;
    }
}
