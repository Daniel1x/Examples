using UnityEngine;
using UnityEngine.Events;

public class MonoBehaviourEventCaller : MonoBehaviour
{
    public event UnityAction<MonoBehaviourEventCaller> OnBehaviourEnabled = null;
    public event UnityAction<MonoBehaviourEventCaller> OnBehaviourDisabled = null;
    public event UnityAction<MonoBehaviourEventCaller> OnBehaviourDestroyed = null;

    private void OnEnable()
    {
        OnBehaviourEnabled?.Invoke(this);
    }

    private void OnDisable()
    {
        OnBehaviourDisabled?.Invoke(this);
    }

    private void OnDestroy()
    {
        OnBehaviourDestroyed?.Invoke(this);
    }
}
