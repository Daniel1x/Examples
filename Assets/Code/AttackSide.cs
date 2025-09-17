[System.Flags]
public enum AttackSide
{
    None = 0,
    Right = 1 << 0,
    Left = 1 << 1,
    Both = Right | Left
}
