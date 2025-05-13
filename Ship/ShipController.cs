using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(ShipState))]
public class ShipController : MonoBehaviour
{
    //[Header("Controls")]
    protected PlayerInput playerInput;

    [SerializeField] protected ModeTutorialConfirmSingle tutorialConfirm;
    protected bool gameStarted = false;
    //protected DashteroidsActions inputActions;

    // Managers
    protected GameManager gameManager;

    [Header("Multiplayer")]
    [SerializeField] protected int playerIndex = 0;
    protected InputDevice myDevice;
    protected static int activePlayerCount = 0;
    protected static List<bool> activePlayerSlots = new List<bool>();

    [Header("Movement")]
    [Tooltip("In degrees per second")]
    [Range(0f, 360f)]
    [SerializeField] protected float turnSpeed;
    [Range(0f, 180f)]
    [SerializeField] protected float easeAtDegrees;
    protected Rigidbody2D playerRb;
    protected ScreenWrapper screenWrapper;

    [Header("Dash")]
    [Tooltip("\"Dash Strength\" applies an impulse of this magnitude")]
    [SerializeField] protected float dashStrengthBase;
    [Tooltip("\"Dash Strength\" applies an impulse of this magnitude")]
    [SerializeField] protected float dashStrengthMax;

    [SerializeField] protected float chargeDuration;
    [SerializeField] protected float dashCooldown;
    [Tooltip("The number of intervals of dash strengths before the Super Dash. Each band linearly increases dash strength from Base Dash Strength to Max Dash Strength. (If this value is set to 1, then Base Dash Strength is used.)")]
    [Range(1, 9)]
    [SerializeField] protected int dashBandCount;

    //[SerializeField] protected float dashSpeedCap;
    [Range(1f, 2f)]
    [Tooltip("Multiplies dash strength force by this value when dashing to counteract current velocity.")]
    [SerializeField] protected float counteractingDashForce;
    protected float chargeTimer = 0.0f;
    protected float dashCooldownTimer = 0.0f;

    [Header("Dash - Visual")]
    [Range(float.Epsilon, 1f)]
    [SerializeField] protected float chargeSquashMax = 1;
    [Range(1, float.MaxValue)]
    [SerializeField] protected float chargeStretchMax = 1;
    [Range(1f, float.MaxValue)]
    [SerializeField] protected float launchStretchMax = 1;
    [Range(float.Epsilon, 1f)]
    [SerializeField] protected float launchSquishMax = 1;
    [SerializeField] protected float launchStretchTime = 0.5f;

    [Header("Super Dash")]
    [SerializeField] protected float superDashDuration; // duration of an initial super dash
    [SerializeField] protected float comboDashDuration; // duration of a super dash off of a burst combo
    //[SerializeField] private float dashTurnStrength;
    [SerializeField] protected float superDashBaseSpeed;
    [SerializeField] protected GameObject dashCanvas;

    [Header("Super Dash - Visual")]
    [Range(1f, float.MaxValue)]
    [SerializeField] protected float superLaunchStretchMax = 1;
    [Range(float.Epsilon, 1f)]
    [SerializeField] protected float superLaunchSquishMax = 1;
    [SerializeField] protected float superLaunchStretchTime = 0.5f;

    [Header("Burst Dash")]
    [SerializeField] protected float burstSpeed;
    [SerializeField] protected float burstDuration;
    [Tooltip("When successfully parrying an asteroid, adds this value times the remaining percentage of burst as a bonus, which decays over time")]
    [Range(0f, float.MaxValue)]
    [SerializeField] protected float comboBoostMax;
    protected float comboBoostEarned;
    protected float comboBoostDecay;

    // Other ship scripts
    protected ShipState shipState;
    protected ShipSound shipSound;
    protected InputSystemUIInputModule inputSystemUIInputModule;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        shipState = GetComponent<ShipState>();
        shipSound = GetComponent<ShipSound>();
        screenWrapper = GetComponent<ScreenWrapper>();

        playerInput = GetComponent<PlayerInput>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        dashCanvas.transform.SetParent(transform.parent);
        tutorialConfirm?.transform.SetParent(GameObject.Find("Confirm Grid")?.transform);

        inputSystemUIInputModule = GameObject.Find("EventSystem").GetComponent<InputSystemUIInputModule>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (gameManager.GetGameState())
        {
            case GameManager.GameState.Tutorial:
                if (playerInput.actions.FindAction("Submit").WasPressedThisFrame() && !gameManager.GetTutorialManager().JustShowedTutorial())
                {
                    tutorialConfirm.Confirm(shipState.GetMainColor());
                    playerInput.SwitchCurrentActionMap("Player");
                }
                break;
            case GameManager.GameState.Countdown:
                if (!gameStarted)
                    RotateShip();
                break;
            case GameManager.GameState.Playing:
                gameStarted = true;
                if (shipState.IsAlive())
                {
                    ShipState.HitState myHitState = shipState.GetHitState();
                    ShipState.DashState myDashState = shipState.GetDashState();

                    switch (myHitState)
                    {
                        case ShipState.HitState.None:
                            switch (myDashState)
                            {
                                case ShipState.DashState.Neutral:
                                    NeutralDashUpdate();
                                    break;
                                case ShipState.DashState.SuperDashing:
                                    SuperDashUpdate();
                                    break;
                                case ShipState.DashState.Bursting:
                                    BurstDashUpdate();
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            chargeTimer = 0;
                            break;
                    }
                }
                else
                {
                    chargeTimer = 0;
                }
                if (playerInput.actions.FindAction("Start").WasPerformedThisFrame())
                {
                    PauseGame();
                    playerInput.SwitchCurrentActionMap("UI");
                    inputSystemUIInputModule.actionsAsset = playerInput.actions;
                }

                if (dashCooldownTimer > 0f)
                    dashCooldownTimer -= Time.deltaTime;
                break;
            case GameManager.GameState.Paused:
                if (playerInput.actions.FindAction("Start").WasPerformedThisFrame())
                    PauseGame();
                break;
            case GameManager.GameState.Results:
                playerInput.SwitchCurrentActionMap("UI");
                break;
        }
    }

    protected void NeutralDashUpdate()
    {
        RotateShip();
        if (dashCooldownTimer <= 0f)
        {
            float chargeProgress;
            if (playerInput.actions.FindAction("Dash").IsPressed())
            {
                chargeTimer += Time.deltaTime;
                chargeTimer = Mathf.Clamp(chargeTimer, 0, chargeDuration * 1.05f);
                chargeProgress = chargeTimer / chargeDuration;

                // Squash
                if (chargeTimer <= chargeDuration)
                    shipState.ScaleShipInstant(new Vector3(Mathf.Sqrt(Mathf.Lerp(1f, chargeStretchMax, chargeProgress)),
                                                           Mathf.Lerp(1f, chargeSquashMax, chargeProgress), 1),
                                               new Vector3(0.5f, 0.0f, 0.5f));

                // Particle System and Sound
                if (chargeTimer >= chargeDuration / dashBandCount && chargeTimer < chargeDuration && !shipState.chargeParticleSystem.isPlaying) // start the charge animation, but only if no charge has been accumulated
                    shipState.RestartParticleSystemAtMe(shipState.chargeParticleSystem);
                
                if (chargeProgress >= 1)
                {
                    if (!shipState.IsChargeFlashing()) shipState.StartShipChargeFlash();
                    if (shipSound.charge.isPlaying) shipSound.charge.Stop();
                    if (!shipSound.chargeReady.isPlaying)
                    {
                        shipSound.chargeReady.Play();
                    }
                }
                else
                {
                    if (!shipSound.charge.isPlaying) shipSound.charge.Play();
                }
            }
            else if (chargeTimer > 0) // if (playerInput.actions.FindAction("Dash").WasReleasedThisFrame())
            {
                shipState.chargeParticleSystem.time = shipState.chargeParticleSystem.main.duration;
                shipState.chargeParticleSystem.Stop();

                if (chargeTimer >= chargeDuration)
                {
                    shipState.StartSuperDash(superDashDuration);
                    shipState.ScaleShipGradulal(new Vector3(1, launchStretchMax, 1), Vector3.one, new Vector3(0.5f, 0.0f, 0.5f), launchStretchTime);

                    shipSound.chargeReady.Stop();
                    shipSound.superDashLaunch.Play();
                    shipSound.superDashSustain.Play();
                }
                else
                {
                    // Calculate dash strength
                    if (dashBandCount > 1)
                    {
                        chargeProgress = chargeTimer / chargeDuration;
                        int bandProgress = (int)Mathf.Floor(chargeProgress * dashBandCount);

                        float integralProgress = (float)bandProgress / (dashBandCount - 1);
                        float dashStrength = Mathf.Lerp(dashStrengthBase, dashStrengthMax, integralProgress);
                        playerRb.AddForce(counteractingDashForce * dashStrength * transform.up, ForceMode2D.Impulse);
                        if (playerRb.linearVelocity.magnitude > dashStrength)
                            playerRb.linearVelocity = playerRb.linearVelocity.normalized * dashStrength;
                        shipState.ScaleShipGradulal(new Vector3(Mathf.Lerp(1, launchSquishMax, integralProgress), Mathf.Lerp(1, launchStretchMax, integralProgress), 1),
                                                    Vector3.one, new Vector3(0.5f, 0.0f, 0.5f), launchStretchTime);
                    }
                    else
                    {
                        playerRb.AddForce(counteractingDashForce * dashStrengthBase * transform.up, ForceMode2D.Impulse);
                        if (playerRb.linearVelocity.magnitude > dashStrengthBase)
                            playerRb.linearVelocity = playerRb.linearVelocity.normalized * dashStrengthBase;
                        shipState.ScaleShipGradulal(new Vector3(Mathf.Lerp(1, launchStretchMax, 0.5f), Mathf.Lerp(1, launchStretchMax, 0.5f), 1),
                                                    Vector3.one, new Vector3(0.5f, 0.0f, 0.5f), launchStretchTime);
                    }
                    shipState.RestartParticleSystemAtMe(shipState.neutralDashParticleSystem);
                    shipSound.dash.Play();
                    shipSound.chargeReady.Stop();
                }

                dashCooldownTimer = dashCooldown;
                chargeTimer = 0;
            }
        }
    }

    protected void SuperDashUpdate()
    {
        RotateShip();
        if (playerInput.actions.FindAction("Dash").WasPerformedThisFrame())
        {
            shipState.StartBurstDash(burstDuration);
            shipSound.superDashSustain.Stop();
            shipSound.burstDash.Play();
        }
        else
        {
            playerRb.linearVelocity = transform.up * (superDashBaseSpeed + comboBoostEarned);
            comboBoostEarned += comboBoostDecay * Time.deltaTime;
        }
    }

    protected void BurstDashUpdate()
    {
        RotateShip(0.2f);
        if (playerInput.actions.FindAction("Dash").WasPerformedThisFrame() && shipState.CanBurstAgain())
            shipState.StartBurstDash(burstDuration);
        playerRb.linearVelocity = transform.up * burstSpeed;
    }

    protected void RotateShip(float turnSpeedModifier = 1)
    {
        Vector2 dashAim = Vector2.zero;
        switch (playerInput.currentControlScheme)
        {
            case "Gamepad":
                dashAim = playerInput.actions.FindAction("Aim").ReadValue<Vector2>();
                break;
            case "Keyboard&Mouse":
                Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(playerInput.actions.FindAction("Target").ReadValue<Vector2>());
                dashAim = (mouseWorld - new Vector2(transform.position.x, transform.position.y)).normalized;
                break;
            default:
                break;
        }

        if (dashAim != Vector2.zero)
        {
            float angle = Vector2.Angle(transform.up, dashAim);
            if (angle == 180)
                dashAim = Quaternion.Euler(0, 0, 1) * dashAim;

            float easing = 1f - Mathf.Clamp01(((easeAtDegrees - angle) / easeAtDegrees) * 0.5f);

            Vector3 lookDirection = Vector3.RotateTowards(transform.up, dashAim, easing * turnSpeed * turnSpeedModifier * Mathf.PI / 180.0f, 1f);
            transform.rotation = Quaternion.LookRotation(transform.forward, lookDirection);
        }
    }

    public void PauseGame()
    {
        gameManager.PauseGame(shipState);
    }

    public void ShowTutorial()
    {
        gameManager.ShowTutorial();
    }

    public void QuitGameMode()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public float GetComboDuration()
    {
        return comboDashDuration;
    }

    public float GetBurstDuration()
    {
        return burstDuration;
    }

    /********************************************************************************
     * Given a remaining percentage of burst, sets the combo bonus and rate of decay
     ********************************************************************************/
    public void SetComboBonus(float percentage)
    {
        comboBoostEarned = comboBoostMax * percentage;
        comboBoostDecay = -comboBoostEarned / comboDashDuration;
    }

    /**************
     * Multiplayer
     **************/
    public int GetPlayerIndex()
    {
        return playerIndex;
    }

    public void SetDevice(InputDevice device)
    {
        myDevice = device;
        Debug.Log("Device of Player " + playerIndex + " is " + device + " (" + device.deviceId + ")");
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        //inputActions.Player.Disable();
        chargeTimer = 0;
    }
}
