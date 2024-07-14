using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class AbilityWheel : MonoBehaviour
{
    private TankController controller;

    [Header("Ability Menu")]
    [SerializeField] private GameObject abilityWheel;
    [SerializeField] private GameObject useAbilityUI;
    [SerializeField] private GameObject cancelAbilityUI;
    public List<Ability> abilities = new List<Ability>();
    public List<Button> buttons = new List<Button>();
    private Ability selectedAbility;
    private Button selectedButton;
    private bool abilityLock;
    private bool useAbility;

    private void Start()
    {
        // Subscribe input manager actions to their corresponding function
        controller = transform.parent.GetComponent<TankController>();

        controller.InputManager().abilityWheelAction.started += OpenMenu;

        controller.InputManager().useAbilityAction.started += context => useAbility = true;
        controller.InputManager().useAbilityAction.canceled += context => useAbility = false;

        controller.InputManager().cancelAbilityAction.started += CancelAbility;

        // Disable the use ability action
        controller.InputManager().DisableAbilityMenuScheme();
        controller.InputManager().DisableAbilityControls();

        // Disable the ability menu
        abilityWheel.SetActive(false);
        useAbilityUI.SetActive(false);
        cancelAbilityUI.SetActive(false);

        // Get selected abilities in the ability holder
        foreach (Transform child in transform)
        {
            abilities.Add(child.GetComponent<Ability>());
        }

        // Get the UI buttons from the ability wheel
        RMF_RadialMenu radialMenu = abilityWheel.GetComponent<RMF_RadialMenu>();
        
        foreach (RMF_RadialMenuElement element in radialMenu.elements)
        {
            buttons.Add(element.GetComponentInChildren<Button>());
        }

        // Change to foreach
        for (int i = 0; i < abilities.Count; i++)
        {
            abilities[i].SetUIButton(buttons[i]);
        }
    }

    private void Update()
    {
        if (useAbility)
        {
            UseAbility();
        }
    }

    #region Ability Wheel
    private void OpenMenu(InputAction.CallbackContext context)
    {
        if (!abilityLock)
        {
            controller.InputManager().EnableAbilityMenuSheme();
            abilityWheel.SetActive(true);
            abilityLock = true;
        }
        else
        {
            controller.InputManager().DisableAbilityMenuScheme();
            abilityWheel.SetActive(false);
            abilityLock = false;
        }
    }

    private void CancelAbility(InputAction.CallbackContext context)
    {
        if (selectedAbility)
        {
            controller.InputManager().DisableAbilityMenuScheme();
            controller.InputManager().DisableAbilityControls();

            selectedAbility.DeactivateAbility();
            selectedAbility = null;

            abilityLock = false;

            useAbilityUI.SetActive(false);
            cancelAbilityUI.SetActive(false);
        }
    }

    private void UseAbility()
    {
        selectedAbility.UseInput(useAbility);
        selectedAbility = null;

        controller.InputManager().DisableAbilityMenuScheme();
        controller.InputManager().DisableAbilityControls();

        abilityLock = false;
        useAbility = false;

        useAbilityUI.SetActive(false);
        cancelAbilityUI.SetActive(false);
    }

    public void SelectAbility(int selection)
    {
        abilityWheel.SetActive(false);
        controller.InputManager().EnableAbilityControls();
        
        selectedAbility = abilities[selection];
        selectedAbility.ActivateAbility();

        useAbilityUI.SetActive(true);
        cancelAbilityUI.SetActive(true);
    }
    #endregion
}
