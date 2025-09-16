using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : IEquipment
{
    [SerializeField, Min(0)] private int damage = 10;

    protected readonly HashSet<Collider> collisionsHandledInCurrentAttack = new HashSet<Collider>();
    protected readonly HashSet<IDamageable> damageablesHitInCurrentAttack = new HashSet<IDamageable>();

    protected override void onNewCollisionEnter(Collider _other)
    {
        if (_other == null)
        {
            return;
        }

        // If we are in the damage window, try to apply immediately (one-shot per attack window).
        if (CanApplyDamage && !collisionsHandledInCurrentAttack.Contains(_other))
        {
            tryToApplyDamage(_other, false);
        }
    }

    protected override void onCanApplyDamageStateChanged()
    {
        if (CanApplyDamage)
        {
            // When window opens, also process anything already overlapping.
            pruneCollisionsInProgress();

            if (collisionsInProgress.Count > 0)
            {
                for (int i = 0; i < collisionsInProgress.Count; i++)
                {
                    tryToApplyDamage(collisionsInProgress[i], true);
                }
            }
        }
        else // When attack ends, clear the list of already hit damageables.
        {
            collisionsHandledInCurrentAttack.Clear();
            damageablesHitInCurrentAttack.Clear();
        }
    }

    protected void tryToApplyDamage(Collider _collider, bool _checkIfContains)
    {
        if (_collider == null || CanApplyDamage == false)
        {
            return; // No damage can be applied
        }

        if (isTheSameLayerAsOwner(_collider))
        {
            return; // Ignore collisions with same layer as owner
        }

        if (_checkIfContains == false || !collisionsHandledInCurrentAttack.Contains(_collider))
        {
            collisionsHandledInCurrentAttack.Add(_collider);
        }

        IDamageable _damageable = _collider.GetComponent<IDamageable>();

        if (_damageable == null)
        {
            _damageable = _collider.GetComponentInParent<IDamageable>();
        }

        if (_damageable == null || damageablesHitInCurrentAttack.Contains(_damageable))
        {
            return; // No damageable found or already hit in this attack
        }

        _damageable.CanReceiveDamage(damage, true);
        damageablesHitInCurrentAttack.Add(_damageable);
    }
}
