using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(ShipState))]
public class ShipController : MonoBehaviour
{
    //[Header("Controls")]
    protected PlayerInput playerInput;
    //protected DashteroidsActions inputActions;

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

    [Header("Super Dash")]
    [SerializeField] protected float superDashDuration; // duration of an initial super dash
    [SerializeField] protected float comboDashDuration; // duration of a super dash off of a burst combo
    //[SerializeField] private float dashTurnStrength;
    [SerializeField] protected float superDashBaseSpeed;
    [SerializeField] protected GameObject dashCanvas;
    [SerializeField] protected MeterUI dashMeterUI;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        shipState = GetComponent<ShipState>();
        screenWrapper = GetComponent<ScreenWrapper>();

        playerInput = GetComponent<PlayerInput>();
        dashCanvas.transform.parent = transform.parent;

        //inputActions = new DashteroidsActions();
        //inputActions.Player.Enable();
        //inputActions.bindingMask = InputBinding.MaskByGroup(inputActions.GamepadScheme.bindingGroup);
    }

    // Update is called once per frame
    void Update()
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
                break;
        }

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;
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

                dashMeterUI.SetSliderPercentage(chargeProgress);

                if (dashBandCount > 1)
                {
                    if (chargeProgress > 1f / dashBandCount) // if charge progress exceeds 1 band
                        dashMeterUI.gameObject.SetActive(true);
                    else
                        dashMeterUI.gameObject.SetActive(false);
                }
                else
                {
                    dashMeterUI.gameObject.SetActive(true);
                }

            }
            else if (playerInput.actions.FindAction("Dash").WasReleasedThisFrame())
            {
                if (chargeTimer >= chargeDuration)
                {
                    shipState.StartSuperDash(superDashDuration);
                }
                else
                {
                    // Calculate dash strength
                    if (dashBandCount > 1)
                    {
                        chargeProgress = chargeTimer / chargeDuration;
                        int bandProgress = (int)Mathf.Floor(chargeProgress * dashBandCount);

                        float dashStrength = Mathf.Lerp(dashStrengthBase, dashStrengthMax, (float)bandProgress / (dashBandCount - 1));
                        playerRb.AddForce(transform.up * dashStrength * counteractingDashForce, ForceMode2D.Impulse);
                        if (playerRb.linearVelocity.magnitude > dashStrength)
                            playerRb.linearVelocity = playerRb.linearVelocity.normalized * dashStrength;
                    }
                    else
                    {
                        playerRb.AddForce(transform.up * dashStrengthBase * counteractingDashForce, ForceMode2D.Impulse);
                        if (playerRb.linearVelocity.magnitude > dashStrengthBase)
                            playerRb.linearVelocity = playerRb.linearVelocity.normalized * dashStrengthBase;
                    }
                }

                dashCooldownTimer = dashCooldown;
                chargeTimer = 0;
                dashMeterUI.gameObject.SetActive(false);
            }
        }
    }

    protected void SuperDashUpdate()
    {
        RotateShip();
        if (playerInput.actions.FindAction("Dash").WasPerformedThisFrame())
            shipState.StartBurstDash(burstDuration);
        else
        {
            //Debug.Log("Dash Speed = " + (superDashBaseSpeed + comboBoostEarned));
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
        Vector2 dashAim = playerInput.actions.FindAction("Aim").ReadValue<Vector2>();
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
        //inputActions.Player.Enable();

        //// Multiplayer

        //activePlayerCount++;
        //if (playerIndex < activePlayerSlots.Count && !activePlayerSlots[playerIndex]) // my slot is available
        //{
        //    activePlayerSlots[playerIndex] = true;
        //    return;
        //}

        //for (int i = 0; i < activePlayerSlots.Count; i++) // find a new slot
        //{
        //    if (!activePlayerSlots[i])
        //    {
        //        activePlayerSlots[i] = true;
        //        playerIndex = i;
        //        return;
        //    }
        //}

        //// All player slots are active
        //activePlayerSlots.Add(true);
        //playerIndex = activePlayerSlots.Count - 1;
    }

    private void OnDisable()
    {
        //inputActions.Player.Disable();
        chargeTimer = 0;
        if (dashMeterUI)
            dashMeterUI.gameObject.SetActive(false);

        //// Multiplayer
        //if (playerIndex >= 0 && playerIndex < activePlayerSlots.Count)
        //    activePlayerSlots[playerIndex] = false;
        //if (activePlayerCount > 0 && !activePlayerSlots[activePlayerCount - 1])
        //    activePlayerSlots.RemoveAt(activePlayerCount - 1);
        //activePlayerCount--;
    }
}
