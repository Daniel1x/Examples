using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class AssetListProvider : IAssetListProvider
{
    [SerializeField] private List<AssetReferenceGameObject> assets = new();

    public List<AssetReferenceGameObject> GetAssets() => assets;

    public int GetNextAssetIndex(int _current, bool _next, bool _defaultAtNegOne = true)
    {
        int _count = assets.Count;

        if (_count == 0)
        {
            return -1;
        }

        if (_defaultAtNegOne)
        {
            int _nextID = _next 
                ? _current + 1 
                : _current - 1;

            if (_nextID == -1 || _nextID == _count)
            {
                return -1;
            }
        }

        return _next
            ? (_current + 1) % _count
            : (_current - 1 + _count) % _count;
    }

    public AssetReferenceGameObject GetAsset(int _id)
    {
        if (_id < 0 || _id >= assets.Count)
        {
            return null;
        }

        return assets[_id];
    }
}
