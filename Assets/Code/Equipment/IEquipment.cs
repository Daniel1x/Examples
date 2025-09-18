using UnityEngine;

public abstract class IEquipment : MonoBehaviour
{
    public AttackSide Side { get; protected set; } = AttackSide.None;
    public int OwnerLayer { get; private set; } = -1;
    public LayerMask DamageableLayerMask { get; private set; } = ~0;

    public abstract void UseEquipment(AnimationEventData.Action _trigger);

    public void Initialize(int _ownerLayer, LayerMask _damageableLayers, AttackSide _side)
    {
        Side = _side;
        OwnerLayer = _ownerLayer;
        DamageableLayerMask = _damageableLayers;
    }
}
