using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class Pickup : MonoBehaviour
{
    [SerializeField] protected AudioClip pickupSound = null;
    [SerializeField] protected VFXSpawnSettings pickupVFX = new();

    public abstract bool OnPickup(UnitStats _unitStats);

    protected virtual void visualizePickupCollected(bool _destroy = true)
    {
        playPickupSound();
        spawnPickupVFX();

        if (_destroy)
        {
            gameObject.ReleaseAssetInstanceOrDestroy();
        }
    }

    protected void playPickupSound()
    {
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }

    protected void spawnPickupVFX()
    {
        pickupVFX.TryToSpawn(transform.position);
    }
}
