using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShipController : MonoBehaviour
{
    //[Header("Controls")]
    //[SerializeField] private KeyCode forwardBtn;
    //[SerializeField] private KeyCode turnLeftBtn;
    //[SerializeField] private KeyCode turnRightBtn;
    [SerializeField] private KeyCode actionButton;

    [Header("Movement")]
    [Tooltip("In degrees per second")]
    [Range(0f, 360f)]
    [SerializeField] protected float turnSpeed;
    [Range(0f, 180f)]
    [SerializeField] protected float easeAtDegrees;
    //[SerializeField] private float thrust;
    //[SerializeField] private float turnStrength;
    //[SerializeField] private float maxFlightSpeed;
    //[SerializeField] private float maxRotationSpeed;
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
    [SerializeField] protected DashMeterUI meterUI;

    [Header("Burst Dash")]
    [SerializeField] protected float burstSpeed;
    [SerializeField] protected float burstDuration;
    //[SerializeField] protected float burstCooldown;
    //protected float burstCooldownTimer = 0.0f;
    //[SerializeField] protected float maxBurstBonus;
    //protected float burstCooldownTimer = 0.0f;
    //protected float burstSpeedGrowth = 1.5f;
    //protected float burstingSpeed; // based on dash speed
    //protected float burstBonus; // increases based on how close you are when initiating burst
    //protected float burstWrapBuffer = Mathf.Pow(Asteroid.GetBaseSize(), 3);

    // Other ship scripts
    protected ShipState shipState;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        shipState = GetComponent<ShipState>();
        screenWrapper = GetComponent<ScreenWrapper>();

        //burstingSpeed = baseDashSpeed * burstSpeedGrowth;
        //burstBonus = 1.0f;
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
                playerRb.linearVelocity = transform.up * burstSpeed;
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

                meterUI.gameObject.SetActive(true);
                meterUI.SetSliderPercentage(chargeProgress);
                //Debug.Log("Charge: " + (chargeProgress / chargeDuration));
            }
            else if (Input.GetKeyUp(actionButton))
            {
                if (chargeTimer >= chargeDuration)
                {
                    Debug.Log("Super Dash!");
                    shipState.StartSuperDash(superDashDuration);
                    //burstBonus = 1.0f;
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

                        Debug.Log("Level " + bandProgress + " Dash!");
                    }
                    else
                    {
                        playerRb.AddForce(transform.up * dashStrengthBase * counteractingDashForce, ForceMode2D.Impulse);
                        if (playerRb.linearVelocity.magnitude > dashStrengthBase)
                            playerRb.linearVelocity = playerRb.linearVelocity.normalized * dashStrengthBase;
                        Debug.Log("Dash!");
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
        {
            shipState.StartBurstDash(burstDuration);
        }
        else
            playerRb.linearVelocity = transform.up * superDashBaseSpeed;
    }

    protected void RotateShip()
    {
        Vector3 mouseWorldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 dashAim = (mouseWorldPoint - transform.position).normalized;
        float easing = 1f - Mathf.Clamp01(((easeAtDegrees - Vector3.Angle(transform.up, dashAim)) / easeAtDegrees) * 0.5f);

        Vector3 lookDirection = Vector3.RotateTowards(transform.up, dashAim, easing * turnSpeed * Mathf.PI / 180.0f, 0);
        transform.rotation = Quaternion.LookRotation(transform.forward, lookDirection);
    }

    public float GetComboDuration()
    {
        return comboDashDuration;
    }

    private void OnDisable()
    {
        chargeTimer = 0;
        if (meterUI)
            meterUI.gameObject.SetActive(false);
    }
}
