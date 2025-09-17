using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class IEquipment : MonoBehaviour
{
    protected readonly List<Collider> collisionsInProgress = new List<Collider>(32);

    public Collider EquipmentCollider { get; private set; } = null;
    public AttackSide Side { get; protected set; } = AttackSide.None;
    public ICanApplyDamage CanApplyDamageProvider { get; private set; } = null;
    public bool CanApplyDamage { get; private set; } = false;
    public int OwnerLayer { get; private set; } = -1;

    protected virtual void Awake()
    {
        EquipmentCollider = GetComponent<Collider>();

        // Ensure trigger mode so Trigger callbacks are received reliably.
        if (EquipmentCollider != null && EquipmentCollider.isTrigger == false)
        {
            Debug.LogWarning("Collider is not set to Trigger. Setting isTrigger = true for proper functionality.", this);
            EquipmentCollider.isTrigger = true;
        }
    }

    protected virtual void OnDisable()
    {
        collisionsInProgress.Clear();
    }

    protected virtual void OnDestroy()
    {
        collisionsInProgress.Clear();
        unregisterFromCanApplyDamageProvider();
        CanApplyDamageProvider = null;
    }

    protected virtual void OnTriggerEnter(Collider _other)
    {
        if (_other == null || isTheSameLayerAsOwner(_other))
        {
            return;
        }

        if (!collisionsInProgress.Contains(_other))
        {
            collisionsInProgress.Add(_other);
        }

        onNewCollisionEnter(_other);
    }

    protected virtual void OnTriggerExit(Collider _other)
    {
        if (_other == null)
        {
            return;
        }

        collisionsInProgress.Remove(_other);
    }

    public void Initialize(int _ownerLayer, ICanApplyDamage _canApplyDamage, AttackSide _side)
    {
        unregisterFromCanApplyDamageProvider();

        OwnerLayer = _ownerLayer;
        CanApplyDamageProvider = _canApplyDamage;
        Side = _side;

        registerToCanApplyDamageProvider();
    }

    private void registerToCanApplyDamageProvider()
    {
        if (CanApplyDamageProvider != null)
        {
            CanApplyDamageProvider.OnCanApplyDamageStateUpdated += onCanApplyDamageStateUpdated;
            onCanApplyDamageStateUpdated();
        }
    }

    private void unregisterFromCanApplyDamageProvider()
    {
        if (CanApplyDamageProvider != null)
        {
            CanApplyDamageProvider.OnCanApplyDamageStateUpdated -= onCanApplyDamageStateUpdated;
        }

        CanApplyDamage = false;
    }

    private void onCanApplyDamageStateUpdated()
    {
        bool _shouldApply = CanApplyDamageProvider != null
            && (CanApplyDamageProvider.CanApplyDamage & Side) != 0;

        if (CanApplyDamage != _shouldApply)
        {
            CanApplyDamage = _shouldApply;
            onCanApplyDamageStateChanged();
        }
    }

    protected bool isTheSameLayerAsOwner(Collider _other)
    {
        return OwnerLayer != -1
            && _other.gameObject.layer == OwnerLayer;
    }

    protected void pruneCollisionsInProgress()
    {
        for (int i = collisionsInProgress.Count - 1; i >= 0; i--)
        {
            if (collisionsInProgress[i] == null)
            {
                collisionsInProgress.RemoveAt(i);
            }
        }
    }

    protected abstract void onNewCollisionEnter(Collider _other);
    protected abstract void onCanApplyDamageStateChanged();
}
