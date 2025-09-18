using UnityEngine;

public class MeleeWeapon : IEquipment
{
    private const int MAX_OVERLAPPING_COLLIDERS = 100;

    public enum DamageZoneShape
    {
        Sphere = 1,
        Box = 2,
    }

    [SerializeField, Min(0)] private int damage = 10;
    [SerializeField] private Transform damageCenter = null;
    [SerializeField] private DamageZoneShape damageZoneShape = DamageZoneShape.Sphere;
    [SerializeField] private float damageRadius = 1f;
    [SerializeField] private Vector3 boxSize = Vector3.one;

    public Transform DamageCenter => damageCenter != null ? damageCenter : transform;

    private Collider[] colliders = new Collider[MAX_OVERLAPPING_COLLIDERS];

    private void OnDrawGizmosSelected()
    {
        Color _defaultGizmosColor = Gizmos.color;
        Gizmos.color = Color.red;

        switch (damageZoneShape)
        {
            case DamageZoneShape.Sphere:
                Gizmos.DrawWireSphere(DamageCenter.position, damageRadius);
                Gizmos.color = Color.red.WithAlpha(0.1f);
                Gizmos.DrawSphere(DamageCenter.position, damageRadius);
                break;

            case DamageZoneShape.Box:
                //Make sure the box is drawn with the correct rotation
                var _defaultMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(DamageCenter.position, DamageCenter.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, boxSize);
                Gizmos.color = Color.red.WithAlpha(0.1f);
                Gizmos.DrawCube(Vector3.zero, boxSize);
                Gizmos.matrix = _defaultMatrix;
                break;
        }

        Gizmos.color = _defaultGizmosColor;
    }

    public override void UseEquipment(AnimationEventData.Action _action)
    {
        Transform _center = DamageCenter;

        if (_center == null)
        {
            return;
        }

        int _numColliders = 0;

        switch (damageZoneShape)
        {
            case DamageZoneShape.Sphere:
                _numColliders = Physics.OverlapSphereNonAlloc(_center.position, damageRadius, colliders, DamageableLayerMask);
                break;

            case DamageZoneShape.Box:
                _numColliders = Physics.OverlapBoxNonAlloc(_center.position, boxSize * 0.5f, colliders, _center.rotation, DamageableLayerMask);
                break;
        }

        for (int i = 0; i < _numColliders; i++)
        {
            tryToApplyDamage(colliders[i]);
        }
    }

    protected void tryToApplyDamage(Collider _collider)
    {
        if (_collider == null)
        {
            return;
        }

        IDamageableObject _damageable = _collider.GetComponent<IDamageableObject>();

        if (_damageable != null)
        {
            _damageable.CanReceiveDamage(gameObject, damage, true);
            return;
        }

        // If not found directly on the collider, try to find it in the parent hierarchy.
        _damageable = _collider.GetComponentInParent<IDamageableObject>();

        if (_damageable != null)
        {
            _damageable.CanReceiveDamage(gameObject, damage, true);
        }
    }
}
