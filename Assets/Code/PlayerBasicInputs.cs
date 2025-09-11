using UnityEngine;
using UnityEngine.InputSystem;
using static Input_Actions;

public class PlayerBasicInputs : CharacterInputProvider, IPlayerActions
{
    [Header("Character Input Values")]
    [SerializeField, ReadOnlyProperty] private Vector2 move = default;
    [SerializeField, ReadOnlyProperty] private bool jump = false;
    [SerializeField, ReadOnlyProperty] private bool sprint = false;
    [SerializeField, ReadOnlyProperty] private bool changeRightWeapon = false;
    [SerializeField, ReadOnlyProperty] private bool changeLeftWeapon = false;

    [Header("Movement Settings")]
    [SerializeField] private bool analogMovement = false;

    public override Vector2 Move => move;
    public override bool Jump { get => jump; set => jump = value; }
    public override bool Sprint => sprint;
    public override bool AnalogMovement => analogMovement;
    public override bool ChangeRightArmWeapon { get => changeRightWeapon; set => changeRightWeapon = value; }
    public override bool ChangeLeftArmWeapon { get => changeLeftWeapon; set => changeLeftWeapon = value; }

    public bool FastAttack { get; set; }
    public bool HeavyAttack { get; set; }

    public void OnMove(InputValue _input) => MoveInput(_input.Get<Vector2>());
    public void OnLook(InputValue _input) { }
    public void OnJump(InputValue _input) => JumpInput(_input.isPressed);
    public void OnSprint(InputValue _input) => SprintInput(_input.isPressed);
    public void OnChangeRightWeapon(InputValue _input) => ChangeRightWeaponInput(_input.isPressed);
    public void OnChangeLeftWeapon(InputValue _input) => ChangeLeftWeaponInput(_input.isPressed);
    public void OnFastAttack(InputValue _input) => FastAttackInput(_input.isPressed);
    public void OnHeavyAttack(InputValue _input) => HeavyAttackInput(_input.isPressed);

    public void MoveInput(Vector2 _newMoveDirection) => move = _newMoveDirection;
    public void JumpInput(bool _newJumpState) => jump = _newJumpState;
    public void SprintInput(bool _newSprintState) => sprint = _newSprintState;
    public void ChangeRightWeaponInput(bool _newChangeState) => changeRightWeapon = _newChangeState;
    public void ChangeLeftWeaponInput(bool _newChangeState) => changeLeftWeapon = _newChangeState;
    public void FastAttackInput(bool _newAttackState) => FastAttack = _newAttackState;
    public void HeavyAttackInput(bool _newAttackState) => HeavyAttack = _newAttackState;

    public void OnMove(InputAction.CallbackContext _context) => MoveInput(_context.ReadValue<Vector2>());
    public void OnLook(InputAction.CallbackContext _context) { }
    public void OnJump(InputAction.CallbackContext _context) => JumpInput(_context.performed);
    public void OnSprint(InputAction.CallbackContext _context) => SprintInput(_context.performed);
    public void OnChangeRightWeapon(InputAction.CallbackContext _context) => ChangeRightWeaponInput(_context.performed);
    public void OnChangeLeftWeapon(InputAction.CallbackContext _context) => ChangeLeftWeaponInput(_context.performed);
    public void OnFastAttack(InputAction.CallbackContext _context) => FastAttackInput(_context.performed);
    public void OnHeavyAttack(InputAction.CallbackContext _context) => HeavyAttackInput(_context.performed);
}
