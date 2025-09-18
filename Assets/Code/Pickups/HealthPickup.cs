using UnityEngine;

public class HealthPickup : Pickup
{
    [SerializeField, Min(0f)] private float healthAmount = 20f;

    public override bool OnPickup(UnitStats _unitStats)
    {
        if (_unitStats.CanHeal(gameObject, healthAmount, true))
        {
            visualizePickupCollected();
            return true;
        }

        return false;
    }
}
