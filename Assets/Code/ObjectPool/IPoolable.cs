public interface IPoolable
{
    public bool CanReturnToPool { get; set; }

    void Initialize(IPoolProvider _pool);
    void OnPulledFromPool();
    void ReturnToPool();
}