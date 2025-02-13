using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShipController : MonoBehaviour
{
    //[Header("Controls")]
    [SerializeField] private KeyCode actionButton;

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
    [Tooltip("Multiplies dash strength force by this value when dashing to counteract current velocity.")]
    [Range(1f, 2f)]
    [SerializeField] protected float counteractingDashForce;
    protected float chargeTimer = 0.0f;
    protected float dashCooldownTimer = 0.0f;

    [Header("Super Dash")]
    [SerializeField] protected float superDashDuration; // duration of an initial super dash
    [SerializeField] protected float comboDashDuration; // duration of a super dash off of a burst combo
    //[SerializeField] private float dashTurnStrength;
    [SerializeField] protected float superDashBaseSpeed;
    [SerializeField] protected MeterUI meterUI;

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
    }

    // Update is called once per frame
    void Update()
    {
        ShipState.DashState myDashState = shipState.GetDashState();

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

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;
    }

    protected void NeutralDashUpdate()
    {
        RotateShip();
        if (dashCooldownTimer <= 0f)
        {
            float chargeProgress;
            if (Input.GetKey(actionButton))
            {
                chargeTimer += Time.deltaTime;
                chargeTimer = Mathf.Clamp(chargeTimer, 0, chargeDuration * 1.05f);
                chargeProgress = chargeTimer / chargeDuration;

                meterUI.SetSliderPercentage(chargeProgress);

                if (dashBandCount > 1)
                {
                    if (chargeProgress > 1f / dashBandCount) // if charge progress exceeds 1 band
                        meterUI.gameObject.SetActive(true);
                    else
                        meterUI.gameObject.SetActive(false);

                }
                else
                {
                    meterUI.gameObject.SetActive(true);
                }

            }
            else if (Input.GetKeyUp(actionButton))
            {
                if (chargeTimer >= chargeDuration)
                {
                    Debug.Log("Super Dash!");
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
                meterUI.gameObject.SetActive(false);
            }
        }
    }

    protected void SuperDashUpdate()
    {
        RotateShip();
        if (Input.GetKeyDown(actionButton))
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
        if (Input.GetKeyDown(actionButton) && shipState.CanBurstAgain())
            shipState.StartBurstDash(burstDuration);
        playerRb.linearVelocity = transform.up * burstSpeed;
    }

    protected void RotateShip()
    {
        Vector3 mouseWorldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 dashAim = (mouseWorldPoint - transform.position).normalized;
        float easing = 1f - Mathf.Clamp01(((easeAtDegrees - Vector3.Angle(transform.up, dashAim)) / easeAtDegrees) * 0.5f);

        Vector3 lookDirection = Vector3.RotateTowards(transform.up, dashAim, easing * turnSpeed * Mathf.PI / 180.0f, 1f);
        transform.rotation = Quaternion.LookRotation(transform.forward, lookDirection);
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

    private void OnDisable()
    {
        chargeTimer = 0;
        if (meterUI)
            meterUI.gameObject.SetActive(false);
    }
}
