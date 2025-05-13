using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput), typeof(PlayerBasicInputs))]
public class PlayerInputInstance : MonoBehaviour
{
    public PlayerInput PlayerInput { get; private set; } = null;
    public PlayerBasicInputs PlayerBasicInputs { get; private set; } = null;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();
        PlayerBasicInputs = GetComponent<PlayerBasicInputs>();
    }
}
