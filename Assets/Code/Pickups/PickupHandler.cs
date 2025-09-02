using UnityEngine;

[RequireComponent(typeof(Collider), typeof(UnitStats))]
public class PickupHandler : MonoBehaviour
{
    private UnitStats unitStats = null;

    private void Awake()
    {
        unitStats = GetComponent<UnitStats>();
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (unitStats == null)
        {
            return;
        }

        if (_other.TryGetComponent<Pickup>(out var _pickup))
        {
            _pickup.OnPickup(unitStats);
        }
    }
}
