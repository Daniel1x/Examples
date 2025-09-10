using System.Collections.Generic;
using UnityEngine.AddressableAssets;

public interface IAssetListProvider
{
    public List<AssetReferenceGameObject> GetAssets();
    public int GetNextAssetIndex(int _current, bool _next, bool _freeSpotAtNegOne = true);
    public AssetReferenceGameObject GetAsset(int _id);
}
