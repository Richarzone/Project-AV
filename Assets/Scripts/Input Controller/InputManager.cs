using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // Input Asset
    private PlayerInput playerInput;

    // Input Actions
    public InputAction moveAction { get; private set; } = new InputAction();
    public InputAction aimAction { get; private set; } = new InputAction();
    public InputAction shootAction { get; private set; } = new InputAction();
    public InputAction reloadAction { get; private set; } = new InputAction();
    public InputAction zoomAction { get; private set; } = new InputAction();
    public InputAction weaponOneAction { get; private set; } = new InputAction();
    public InputAction weaponTwoAction { get; private set; } = new InputAction();
    public InputAction weaponThreeAction { get; private set; } = new InputAction();
    public InputAction abilityWheelAction { get; private set; } = new InputAction();
    public InputAction useAbilityAction { get; private set; } = new InputAction();
    public InputAction cancelAbilityAction { get; private set; } = new InputAction();
    public InputAction healAction { get; private set; } = new InputAction();
    public InputAction pauseAction { get; private set; } = new InputAction();

    public bool abilitySwitch { get; private set; }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Movement"];
        aimAction = playerInput.actions["Aim"];
        shootAction = playerInput.actions["Shoot"];
        reloadAction = playerInput.actions["Reload"];
        zoomAction = playerInput.actions["Zoom"];
        weaponOneAction = playerInput.actions["Weapon 1"];
        weaponTwoAction = playerInput.actions["Weapon 2"];
        weaponThreeAction = playerInput.actions["Weapon 3"];
        abilityWheelAction = playerInput.actions["Ability Menu"];
        useAbilityAction = playerInput.actions["Use Ability"];
        cancelAbilityAction = playerInput.actions["Cancel Ability"];
        healAction = playerInput.actions["Heal"];
        pauseAction = playerInput.actions["Pause"];
    }

    private void OnEnable()
    {
        moveAction.Enable();
        aimAction.Enable();
        shootAction.Enable();
        reloadAction.Enable();
        weaponOneAction.Enable();
        weaponTwoAction.Enable();
        weaponThreeAction.Enable();
        abilityWheelAction.Enable();
        useAbilityAction.Enable();
        cancelAbilityAction.Enable();
        healAction.Enable();
        pauseAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        aimAction.Disable();
        shootAction.Disable();
        reloadAction.Disable();
        weaponOneAction.Disable();
        weaponTwoAction.Disable();
        weaponThreeAction.Disable();
        abilityWheelAction.Disable();
        useAbilityAction.Disable();
        cancelAbilityAction.Disable();
        healAction.Disable();
        pauseAction.Disable();
    }

    private void Start()
    {
        
    }

    public void EnableTankControls()
    {
        moveAction.Enable();
        aimAction.Enable();
        shootAction.Enable();
        reloadAction.Enable();
        weaponOneAction.Enable();
        weaponTwoAction.Enable();
        weaponThreeAction.Enable();
        abilityWheelAction.Enable();
        healAction.Enable();
    }

    public void DisableTankControls()
    {
        moveAction.Disable();
        aimAction.Disable();
        shootAction.Disable();
        reloadAction.Disable();
        weaponOneAction.Disable();
        weaponTwoAction.Disable();
        weaponThreeAction.Disable();
        abilityWheelAction.Disable();
        useAbilityAction.Disable();
        cancelAbilityAction.Disable();
        healAction.Disable();
    }

    #region Ability Menu
    public void EnableAbilityMenuSheme()
    {
        shootAction.Disable();
        reloadAction.Disable();
        weaponOneAction.Disable();
        weaponTwoAction.Disable();
        weaponThreeAction.Disable();

        abilitySwitch = true;
    }

    public void DisableAbilityMenuScheme()
    {
        shootAction.Enable();
        reloadAction.Enable();
        weaponOneAction.Enable();
        weaponTwoAction.Enable();
        weaponThreeAction.Enable();

        abilitySwitch = false;
    }

    public void EnableAbilityControls()
    {
        useAbilityAction.Enable();
        cancelAbilityAction.Enable();

        abilityWheelAction.Disable();
    }

    public void DisableAbilityControls()
    {
        useAbilityAction.Disable();
        cancelAbilityAction.Disable();

        abilityWheelAction.Enable();
    }
    #endregion
}
