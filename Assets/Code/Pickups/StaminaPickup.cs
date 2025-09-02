using UnityEngine;

public class StaminaPickup : Pickup
{
    [SerializeField, Min(0f)] private float staminaAmount = 20f;

    public override bool OnPickup(UnitStats _unitStats)
    {
        if (_unitStats.CanRestoreStamina(staminaAmount, true))
        {
            visualizePickupCollected();
            return true;
        }

        return false;
    }
}
