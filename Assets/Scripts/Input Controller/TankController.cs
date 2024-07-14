using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
//using UnityEngine.UIElements;

public class TankController : MonoBehaviour
{
    private Rigidbody rb;
    private Collider tankCollider;
    private PlayerStatus playerStatus;

    // Input
    private InputManager inputManager;

    private Vector2 movement;
    private Vector2 aim;
    private bool shoot;
    private bool zoom;

    [Header("Tank References")]
    [SerializeField] private Transform tankBody;
    [SerializeField] private AudioSource resuplySource;

    [Header("Tank Movement Settings")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float reverseSpeed;
    [SerializeField] private float breakSpeed;

    [Header("Tank Rotation Settings")]
    [SerializeField] private float rotationSpeed;

    [Header("Camera")]
    [SerializeField] private GameObject cameraPivot;

    [Header("Weapons")]
    [SerializeField] private List<GameObject> weapons = new List<GameObject>();
    [SerializeField] private LayerMask aimingLayer;
    private Trajectory weaponHandler;
    private int currentWeapon;

    [Header("UI")]
    [SerializeField] private Canvas playerCanvas;
    [SerializeField] private Canvas UICanvas;
    [SerializeField] private Transform crosshair;
    [SerializeField] float aimRotationOffset;

    private void Awake()
    {
        // Get references
        inputManager = GetComponent<InputManager>();
        rb = GetComponent<Rigidbody>();
        tankCollider = GetComponent<Collider>();
        playerStatus = GetComponent<PlayerStatus>();
        
        foreach (GameObject weapon in weapons)
        {
            weapon.transform.GetChild(0).GetComponent<Trajectory>().PlayerCollider(tankCollider);
        }
    }

    private void Start()
    {
        // Subscribe input manager actions to their corresponding function
        inputManager.moveAction.performed += context => movement = context.ReadValue<Vector2>();
        
        inputManager.aimAction.performed += context => aim = context.ReadValue<Vector2>();

        inputManager.shootAction.started += context => shoot = true;
        inputManager.shootAction.canceled += context => shoot = false;

        inputManager.reloadAction.started += CallReload;

        inputManager.zoomAction.started += OnZoom;
        inputManager.zoomAction.canceled += ZoomOut;

        inputManager.weaponOneAction.started += context => ChangeWeapon(0);

        inputManager.weaponTwoAction.started += context => ChangeWeapon(1);

        inputManager.weaponThreeAction.started += context => ChangeWeapon(2);

        cameraPivot.transform.localPosition = Vector3.zero;

        currentWeapon = 0;
        weaponHandler = weapons[currentWeapon].transform.GetChild(0).GetComponent<Trajectory>();
        weaponHandler.Active(true);

        playerStatus.UpdateWeaponText(weaponHandler);
        playerStatus.ChangeWeapon(weaponHandler);

        Cursor.visible = false;
    }

    private void Update()
    {
        MoveCrosshair();

        if (zoom)
        {
            weaponHandler.Zoom(cameraPivot);
        }

        if (movement.y == 0)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, breakSpeed * Time.deltaTime);
        }

        if (shoot)
        {
            weaponHandler.Shoot();
            playerStatus.UpdateWeaponText(weaponHandler);
        }
        
        if (inputManager.abilitySwitch)
        {
            crosshair.gameObject.SetActive(false);
        }
        else
        {
            crosshair.gameObject.SetActive(true);
        }
    }

    private void FixedUpdate()
    {
        Accelerate();
        TankRotatation();
    }

    #region Tank Movement
    private void Accelerate()
    {
        if (movement.y > 0)
        {
            rb.AddForce((tankBody.forward * movement.y * movementSpeed) - rb.velocity, ForceMode.VelocityChange);
        }
        else if (movement.y < 0)
        {
            rb.AddForce((tankBody.forward * movement.y * reverseSpeed) - rb.velocity, ForceMode.VelocityChange);
        }

        if (!inputManager.moveAction.enabled)
        {
            movement.y = 0;
        }
    }

    private void TankRotatation()
    {
        tankBody.Rotate(0f, movement.x * rotationSpeed * Time.deltaTime, 0f);

        //rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, movement.x * rotationSpeed * Time.fixedDeltaTime, 0f));

        if (!inputManager.moveAction.enabled)
        {
            movement.x = 0;
        }
    }
    #endregion

    #region Tank Aiming
    private void MoveCrosshair()
    {
        Ray ray = Camera.main.ScreenPointToRay(aim);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, aimingLayer))
        {
            crosshair.position = raycastHit.point;
        }
    }

    private void OnZoom(InputAction.CallbackContext context)
    {
        zoom = true;
        
        weaponHandler.ActivateLines();
    }
    
    private void ZoomOut(InputAction.CallbackContext context)
    {
        zoom = false;
        weaponHandler.DeactivateLines();
        cameraPivot.transform.localPosition = Vector3.zero;
    }
    #endregion

    #region Tank Weapons
    private void CallReload(InputAction.CallbackContext context)
    {
        weaponHandler.CallReload();
    }

    public void StopAllWeapons()
    {
        foreach (GameObject weaponObject in weapons)
        {
            Trajectory weapon = weaponObject.transform.GetChild(0).GetComponent<Trajectory>();
            weapon.CancelInvoke();
        }
    }

    public void ResetAllWeapons()
    {
        foreach (GameObject weaponObject in weapons)
        {
            Trajectory weapon = weaponObject.transform.GetChild(0).GetComponent<Trajectory>();
            weapon.Reload();
        }
    }

    public void RefillReserveAmmo()
    {
        foreach (GameObject weaponObject in weapons)
        {
            Trajectory weapon = weaponObject.transform.GetChild(0).GetComponent<Trajectory>();
            weapon.RefillAmmo();
        }

        resuplySource.Play();
    }

    private void ChangeWeapon(int selected)
    {
        // Deactivate the current selected weapon and its line renderers
        weaponHandler.Active(false);
        weaponHandler.DeactivateLines();

        // Change the current selected weapon the newly selected one
        currentWeapon = selected;
        weaponHandler = weapons[currentWeapon].transform.GetChild(0).GetComponent<Trajectory>();

        // Activate the newly selected weapon
        weaponHandler.Active(true);

        playerStatus.ChangeWeapon(weaponHandler);

        // Change the crosshair to the newly selected weapon's crosshair
        crosshair.GetComponent<SpriteRenderer>().sprite = weaponHandler.GetCrosshair();
        crosshair.GetComponent<Renderer>().material = weaponHandler.GetCrosshairMaterial();
        crosshair.localScale = weaponHandler.GetCrosshairSize();

        // If the player is aming while changing weapons activate the newly slected weapons line renderers
        if (zoom)
        {
            weaponHandler.ActivateLines();
        }
    }
    #endregion

    #region Properties
    public void UpdateWeaponInfo()
    {
        playerStatus.UpdateWeaponText(weaponHandler);
    }

    public InputManager InputManager()
    {
        return inputManager;
    }

    public Collider TankCollider()
    {
        return tankCollider;
    }

    public void ActivateCollision()
    {
        tankCollider.enabled = true;
    }

    public void DeactivateCollision()
    {
        tankCollider.enabled = false;
    }

    /*public void SetUIObject(GameObject UIObject)
    {
        if (UICanvas.transform.childCount > 0)
        {
            Destroy(UICanvas.transform.GetChild(0).gameObject);
            Instantiate(UIObject, UICanvas.transform);
        }
        else
        {
            Instantiate(UIObject, UICanvas.transform);
        }
    }*/
    #endregion
}

/*
    “A hero would sacrifice you for the world. A villain would sacrifice the world for you”
    "If Im the villan for loving you then so be it"
*/