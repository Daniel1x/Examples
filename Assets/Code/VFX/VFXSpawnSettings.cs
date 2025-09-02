using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class VFXSpawnSettings
{
    public bool Spawn = true;
    [Min(0f)] public float Lifetime = 2f;
    public AssetReferenceGameObject Prefab = new AssetReferenceGameObject(string.Empty);

    public void TryToSpawn(Vector3 _position) => TryToSpawn(_position, Quaternion.identity);

    public void TryToSpawn(Vector3 _position, Quaternion _rotation)
    {
        if (Spawn && Prefab.RuntimeKeyIsValid())
        {
            Prefab.InstantiateAsync(_position, _rotation).Completed += handleVFXInstantiated;
        }
    }

    private void handleVFXInstantiated(AsyncOperationHandle<GameObject> _handle)
    {
        if (_handle.Status is AsyncOperationStatus.Succeeded)
        {
            _handle.Result.GetOrAddComponent<DestructionAfterTime>(out _).Lifetime = Lifetime;
        }
    }
}
