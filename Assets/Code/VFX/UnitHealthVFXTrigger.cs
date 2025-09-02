using UnityEngine;

[RequireComponent(typeof(UnitStats))]
public class UnitHealthVFXTrigger : MonoBehaviour
{
    [SerializeField] private VFXSpawnSettings deathVFX = new();
    [SerializeField] private VFXSpawnSettings damagedVFX = new();
    [SerializeField] private Transform spawnPoint = null;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;

    private UnitStats unitStats = null;

    private void Awake()
    {
        unitStats = GetComponent<UnitStats>();

        if (unitStats != null)
        {
            unitStats.OnDeath += handleUnitDeath;
            unitStats.OnDamaged += handleUnitDamage;
        }
    }

    private void OnDestroy()
    {
        if (unitStats != null)
        {
            unitStats.OnDeath -= handleUnitDeath;
            unitStats.OnDamaged -= handleUnitDamage;
        }
    }

    private Vector3 getSpawnPosition()
    {
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }

        return spawnPoint.position + spawnOffset;
    }

    private void handleUnitDeath() => deathVFX.TryToSpawn(getSpawnPosition());
    private void handleUnitDamage() => damagedVFX.TryToSpawn(getSpawnPosition());
}
