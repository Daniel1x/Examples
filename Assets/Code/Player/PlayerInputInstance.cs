using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput), typeof(PlayerBasicInputs))]
public class PlayerInputInstance : MonoBehaviour, IRequiresCharacterInputProvider<PlayerBasicInputs>
{
    public PlayerInput PlayerInput { get; private set; } = null;
    public PlayerBasicInputs InputProvider { get; set; } = null;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();
        InputProvider = GetComponent<PlayerBasicInputs>();
    }
}
