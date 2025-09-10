using UnityEngine;

public abstract class CharacterInputProvider : MonoBehaviour
{
    public abstract Vector2 Move { get; }
    public abstract bool Jump { get; set; }
    public abstract bool Sprint { get; }
    public abstract bool AnalogMovement { get; }

    public abstract bool ChangeRightArmWeapon { get; set; }
    public abstract bool ChangeLeftArmWeapon { get; set; }
}
