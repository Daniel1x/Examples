using UnityEngine;

public class ManaPickup : Pickup
{
    [SerializeField, Min(0f)] private float manaAmount = 20f;

    public override bool OnPickup(UnitStats _unitStats)
    {
        if (_unitStats.CanRestoreMana(manaAmount, true))
        {
            visualizePickupCollected();
            return true;
        }

        return false;
    }
}
