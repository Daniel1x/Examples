public interface IRequiresCharacterInputProvider<T> where T : CharacterInputProvider
{
    public T InputProvider { get; set; }
}
