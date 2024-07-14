using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Player Status
public class PlayerStatus : UnitHealth
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerBody;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private GameObject destroyedModel;
    [SerializeField] private GameObject redeploy;
    [SerializeField] private Transform spawnPoint;


    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuView;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button exitButton;

    [Header("Controls Menu")]
    [SerializeField] private GameObject controlsMenuView;
    [SerializeField] private Button controlsBackButton;

    [Header("Player Status")]
    [SerializeField] private int lifes;
    [SerializeField] private int maxRepairKits;
    [SerializeField] private int repairKitHealing;
    [SerializeField] private float respawnTime;
    private int currentRepairKits;

    [Header("Player Health UI")]
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private TextMeshProUGUI playerLifes;
    [SerializeField] private TextMeshProUGUI playerRepairKitsText;
    [SerializeField] private TextMeshProUGUI maxRepairKitsText;

    [Header("Weapon UI")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI currentAmmoText;
    [SerializeField] private TextMeshProUGUI reserveAmmoText;
    [SerializeField] private GameObject lowAmmo;
    [SerializeField] private GameObject noAmmo;

    [Header("Out Of Bounds UI")]
    [SerializeField] private OutOfBoundsTimer outOfBoundsTimer;

    [Header("Mission Status")]
    [SerializeField] private GameObject missionComplete;
    [SerializeField] private GameObject missionFailed;
    [SerializeField] private GameObject gameOverFade;

    [Header("Enemy Danger UI")]
    [SerializeField] private GameObject airstrikeWarning;

    private TankController controller;
    private bool death;
    private bool respawning;
    private bool outOfBounds;

    protected override void Start()
    {
        health = new Health(maxHealth);
        invulnerable = false;

        healthBar.SetHealthManager(health);
        health.OnDeath += Health_OnDeath;

        outOfBoundsTimer.SetPlayerStatus(this);

        controller = GetComponent<TankController>();
        controller.InputManager().healAction.started += UseRepairKit;
        controller.InputManager().pauseAction.started += PauseGame;
        
        currentRepairKits = maxRepairKits;

        playerLifes.text = lifes.ToString();
        playerRepairKitsText.text = maxRepairKits.ToString();
        maxRepairKitsText.text = maxRepairKits.ToString();

        playerModel.SetActive(false);
        deathScreen.SetActive(false);
        airstrikeWarning.SetActive(false);
        missionComplete.SetActive(false);
        missionFailed.SetActive(false);
        gameOverFade.SetActive(false);
        lowAmmo.SetActive(false);
        noAmmo.SetActive(false);

        controller.InputManager().DisableTankControls();

        GameObject.FindGameObjectWithTag("Mission Manager").GetComponent<MissionManager>().SetPlayerStatus(this);

        StartCoroutine(StartingSpawn());
    }

    private void Update()
    {
        if (respawning)
        {
            transform.position = spawnPoint.position;
            playerBody.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    #region Pause
    private void PauseGame(InputAction.CallbackContext context)
    {
        Time.timeScale = 0f;
        controller.InputManager().DisableTankControls();
        pauseMenuView.SetActive(true);
    }

    public void PlayButton()
    {
        pauseMenuView.SetActive(false);
        controller.InputManager().EnableTankControls();
        Time.timeScale = 1f;
    }

    public void ControlsButton()
    {
        controlsMenuView.SetActive(true);
    }

    public void ExitButton()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void ControlsBackButton()
    {
        pauseMenuView.SetActive(true);
        controlsMenuView.SetActive(false);
    }

    #endregion

    #region Spawning
    private IEnumerator StartingSpawn()
    {
        Instantiate(redeploy, player.position, player.rotation);

        yield return new WaitForSeconds(2f);

        playerModel.SetActive(true);
        controller.InputManager().EnableTankControls();
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        respawning = true;

        deathScreen.SetActive(false);
        healthBar.ResetHealthBar();
        health.Heal(maxHealth);

        transform.position = spawnPoint.position;
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        Instantiate(redeploy, transform.position, transform.rotation);

        yield return new WaitForSeconds(2.1f);

        death = false;
        respawning = false;
        controller.InputManager().EnableTankControls();

        playerModel.SetActive(true);
        controller.ResetAllWeapons();
        controller.ActivateCollision();
    }

    public void SetSpawnPoint(Transform position)
    {
        spawnPoint = position;
    }
    #endregion

    #region Death
    private void Health_OnDeath(object sender, System.EventArgs e)
    {
        StartCoroutine(Death());
    }

    private IEnumerator Death()
    {
        death = true;

        healthBar.EmptyHealthBar();
        controller.InputManager().DisableTankControls();
        controller.DeactivateCollision();
        controller.StopAllWeapons();

        ResetOutOfBounds();

        Instantiate(destroyedModel, transform.position, playerBody.transform.rotation);
        playerModel.SetActive(false);

        yield return new WaitForSeconds(1f);

        if (lifes > 0)
        {
            deathScreen.SetActive(true);
            deathScreen.GetComponent<Animator>().Play("Unit Lost");

            lifes--;
            playerLifes.text = lifes.ToString();
            // Rienforncemt voice line
            StartCoroutine(Respawn());
        }
        else
        {
            StartCoroutine(MissionFailed());
        }
    }

    public void CallDeath()
    {
        StartCoroutine(Death());
    }

    public bool GetDeathFlag()
    {
        return death;
    }
    #endregion

    #region Weapon
    public void UpdateWeaponText(Trajectory weapon)
    {
        currentAmmoText.text = weapon.GetCurrentAmmo().ToString();
        reserveAmmoText.text = weapon.GetReserveAmmo().ToString();

        CheckForAmmo(weapon);
    }

    public void ChangeWeapon(Trajectory weapon)
    {
        weaponNameText.text = weapon.GetWeaponName();
        currentAmmoText.text = weapon.GetCurrentAmmo().ToString();
        reserveAmmoText.text = weapon.GetReserveAmmo().ToString();

        CheckForAmmo(weapon);
    }

    private void CheckForAmmo(Trajectory weapon)
    {
        if (weapon.GetCurrentAmmo() == 0f && weapon.GetReserveAmmo() == 0f)
        {
            lowAmmo.SetActive(false);
            noAmmo.SetActive(true);
            return;
        }
        else
        {
            noAmmo.SetActive(false);
        }

        if (weapon.GetReserveAmmo() <= (weapon.GetMaxAmmo() * 0.2f))
        {
            lowAmmo.SetActive(true);
        }
        else
        { 
            lowAmmo.SetActive(false); 
        }
    }
    #endregion

    #region Repair Kit
    private void UseRepairKit(InputAction.CallbackContext context)
    {
        if (currentRepairKits > 0)
        {
            health.Heal(repairKitHealing);
        
            currentRepairKits--;
            playerRepairKitsText.text = currentRepairKits.ToString();
        }
    }

    public void RefillRepairKit()
    {
        currentRepairKits = maxRepairKits;
        playerRepairKitsText.text = currentRepairKits.ToString();
    }
    #endregion

    #region Out Of Bounds
    public void SetOutOfBounds()
    {
        outOfBoundsTimer.gameObject.SetActive(true);
    }

    public void ResetOutOfBounds()
    {
        outOfBoundsTimer.ResetTimer();
    }
    #endregion

    #region Warnings
    public void AirstrikeWarning()
    {
        airstrikeWarning.SetActive(true);
        airstrikeWarning.GetComponent<Animator>().Play("Warning Notification");
    }
    #endregion

    #region Game Over
    private IEnumerator MissionFailed()
    {
        controller.InputManager().DisableTankControls();

        missionFailed.SetActive(true);

        yield return new WaitForSeconds(2f);

        gameOverFade.SetActive(true);
    }

    private IEnumerator MissionComplete()
    {
        controller.InputManager().DisableTankControls();
        missionComplete.SetActive(true);
        invulnerable = true;

        yield return new WaitForSeconds(3f);

        gameOverFade.SetActive(true);
    }

    public void CallMissionEnd()
    {
        StartCoroutine(MissionComplete());
    }
    #endregion
}