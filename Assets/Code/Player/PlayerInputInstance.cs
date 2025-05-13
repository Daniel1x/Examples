using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputInstance : MonoBehaviour
{
    public PlayerInput PlayerInput { get; private set; } = null;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();
    }
}
