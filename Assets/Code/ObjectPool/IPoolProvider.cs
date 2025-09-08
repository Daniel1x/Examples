using UnityEngine;

public interface IPoolProvider
{
    Transform ParentTransform { get; }
    float ObjectMaxLifetime { get; }
}
