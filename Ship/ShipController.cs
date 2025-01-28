using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShipController : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private KeyCode forwardBtn;
    [SerializeField] private KeyCode turnLeftBtn;
    [SerializeField] private KeyCode turnRightBtn;
    [SerializeField] private KeyCode chargeBtn;

    [Header("Movement")]
    [SerializeField] private float thrust;
    [SerializeField] private float turnStrength;
    [SerializeField] private float maxFlightSpeed;
    [SerializeField] private float maxRotationSpeed;
    private Rigidbody2D playerRb;
    private ScreenWrapper screenWrapper;

    [Header("Dash")]
    [SerializeField] private float chargeDuration;
    private float chargeProgress;
    [SerializeField] private float dashDuration; // duration of an initial dash
    [SerializeField] private float comboDuration; // duration of an dash off of a burst combo
    [SerializeField] private float dashTurnStrength;
    [SerializeField] private float baseDashSpeed;
    [SerializeField] protected DashMeterUI meterUI;

    [Header("Burst")]
    [SerializeField] private float burstRange;
    [SerializeField] private float burstCooldown;
    [SerializeField] private float maxBurstBonus;
    private float burstCooldownTimer = 0.0f;
    private float burstSpeedGrowth = 1.5f;
    private float burstingSpeed; // based on dash speed
    private float burstBonus; // increases based on how close you are when initiating burst
    private float burstWrapBuffer = Mathf.Pow(Asteroid.GetBaseSize(), 3);

    // Other ship scripts
    private ShipState shipState;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        shipState = GetComponent<ShipState>();
        screenWrapper = GetComponent<ScreenWrapper>();

        burstingSpeed = baseDashSpeed * burstSpeedGrowth;
        burstBonus = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!shipState.IsDashing())
        {
            if (Input.GetKey(chargeBtn))
            {
                chargeProgress += Time.deltaTime;
                chargeProgress = Mathf.Clamp(chargeProgress, 0, chargeDuration * 1.05f);
                meterUI.gameObject.SetActive(true);
                meterUI.SetSliderPercentage(chargeProgress / chargeDuration);
                //Debug.Log("Charge: " + (chargeProgress / chargeDuration));
            }
            else if (Input.GetKeyUp(chargeBtn))
            {
                if (chargeProgress >= chargeDuration)
                {
                    shipState.StartDash(dashDuration);
                    burstBonus = 1.0f;
                }
                chargeProgress = 0;
                meterUI.gameObject.SetActive(false);
            }
        }

        if (shipState.IsDashing())
        {
             if (Input.GetKeyDown(chargeBtn) && burstCooldownTimer <= 0.0f)
             {
                float radius = (transform.localScale.x + transform.localScale.y) / 4f;
                // Use RayCast to detect asteroid nearby for burst
                Debug.DrawRay(transform.position - (transform.right * radius), transform.up * (burstRange + 0.5f), Color.green, 3f);
                Debug.DrawRay(transform.position, transform.up * (burstRange + 0.5f), Color.blue, 3f);
                Debug.DrawRay(transform.position + (transform.right * radius), transform.up * (burstRange + 0.5f), Color.green, 3f);
                RaycastHit2D hitInfo =
                    Physics2D.CircleCast(transform.position, radius,
                                         transform.up, burstRange + 0.5f, LayerMask.GetMask("Asteroid"));
                if (screenWrapper)
                {
                    int wraps = 0;
                    float totalDistance = 0.0f;
                    Vector2 castStart = transform.position;

                    float xMargin = screenWrapper.GetBaseMargin() + (playerRb ? Mathf.Abs(playerRb.linearVelocity.x) * 0.1f : 0.0f);
                    float yMargin = screenWrapper.GetBaseMargin() + (playerRb ? Mathf.Abs(playerRb.linearVelocity.y) * 0.1f : 0.0f);

                    float screenWidth = Camera.main.orthographicSize * Camera.main.aspect * 2;
                    float screenHeight = Camera.main.orthographicSize * 2;

                    float xWrap = (screenWidth + xMargin) / 2;
                    float yWrap = (screenHeight + yMargin) / 2;

                    while (totalDistance < burstRange && !hitInfo)
                    {
                        // Screen Wrap Burst:
                        // if ship has screenwrapper component and is less than burstRange from edge in any direction
                        // start another Circlecast at wrap point

                        // Calculate burst limit position and see if it goes out of bounds
                        bool shootsOob = false;
                        Vector2 burstEndPosition = castStart + (Vector2)(transform.up * burstRange);
                        if (Mathf.Abs(burstEndPosition.x) > xWrap || Mathf.Abs(burstEndPosition.y) > yWrap)
                            shootsOob = true;

                        if (shootsOob)
                        {
                            wraps++;
                            // Calculate wrap point
                            Vector2 borderPoint = new Vector2(Mathf.Clamp(burstEndPosition.x, -xWrap, xWrap),
                                                            Mathf.Clamp(burstEndPosition.y, -yWrap, yWrap));
                            Vector2 wrapPoint = borderPoint;

                            if (Mathf.Abs(burstEndPosition.x) > xWrap)
                                wrapPoint = new Vector2(wrapPoint.x * -1, wrapPoint.y);
                            if (Mathf.Abs(burstEndPosition.y) > yWrap)
                                wrapPoint = new Vector2(wrapPoint.x, wrapPoint.y * -1);

                            // Calculate new travel distance
                            float borderDist = (borderPoint - (Vector2)transform.position).magnitude;
                            totalDistance += borderDist;

                            // Start a new cast (behind where the asteroid can spawn)
                            castStart = wrapPoint - (Vector2)(transform.up * burstWrapBuffer);

                            Debug.DrawRay(castStart - (Vector2)(transform.right * radius),
                                          transform.up * (burstRange + burstWrapBuffer), Color.yellow, 3f);
                            Debug.DrawRay(castStart, transform.up * (burstRange + burstWrapBuffer), Color.magenta, 3f);
                            Debug.DrawRay(castStart + (Vector2)(transform.right * radius),
                                          transform.up * (burstRange+ burstWrapBuffer), Color.yellow, 3f);
                            hitInfo = Physics2D.CircleCast(castStart, radius,
                                         transform.up, (burstRange - totalDistance) + burstWrapBuffer, LayerMask.GetMask("Asteroid"));
                            castStart += (Vector2)(transform.up * burstWrapBuffer);
                        }
                        else
                        {
                            totalDistance = burstRange;
                        }
                    }

                    if (hitInfo)
                    {
                        // if a wrap occurred, the way distance is calculated changes slightly
                        float wrapModifier = wraps > 0 ? hitInfo.distance - burstWrapBuffer : 0;

                        burstBonus = Mathf.Max(burstSpeedGrowth, maxBurstBonus * ((burstRange - (totalDistance + wrapModifier)) / burstRange));
                        shipState.StartBurst();
                    }
                    else
                        burstCooldownTimer = burstCooldown;
                }
                else
                {
                    if (hitInfo)
                    {
                        burstBonus = Mathf.Max(burstSpeedGrowth, maxBurstBonus * ((burstRange - hitInfo.distance) / burstRange));
                        shipState.StartBurst();
                    }
                    else
                        burstCooldownTimer = burstCooldown;
                }
                
            }

            if (shipState.IsBursting())
            {
                playerRb.linearVelocity = transform.up * burstingSpeed;
            }
            else
            {
                // Apply dash force
                playerRb.linearVelocity = transform.up * baseDashSpeed * burstBonus;
                if (playerRb.linearVelocity.magnitude <= baseDashSpeed * burstBonus)
                    playerRb.AddForce(transform.up * ((baseDashSpeed * burstBonus) - playerRb.linearVelocity.magnitude), ForceMode2D.Impulse);

                if (Input.GetKey(turnLeftBtn))
                {
                    playerRb.AddTorque(dashTurnStrength);
                }

                if (Input.GetKey(turnRightBtn))
                {
                    playerRb.AddTorque(-dashTurnStrength);
                }
            }

            playerRb.angularVelocity = Mathf.Clamp(playerRb.angularVelocity, -maxRotationSpeed, maxRotationSpeed);
        }
        else
        {
            float chargeSpeedMod = chargeProgress > 0 ? 0.5f : 1f;
            if (Input.GetKey(forwardBtn))
            {
                if (playerRb.linearVelocity.magnitude <= maxFlightSpeed * chargeSpeedMod)
                    playerRb.AddForce(transform.up * thrust, ForceMode2D.Force);
            }

            if (Input.GetKey(turnLeftBtn))
            {
                playerRb.AddTorque(turnStrength);
            }

            if (Input.GetKey(turnRightBtn))
            {
                playerRb.AddTorque(-turnStrength);
            }

            playerRb.angularVelocity = Mathf.Clamp(playerRb.angularVelocity, -maxRotationSpeed * chargeSpeedMod, maxRotationSpeed * chargeSpeedMod);
        }

        burstCooldownTimer -= Time.deltaTime;
    }

    public float GetComboDuration()
    {
        return comboDuration;
    }

    private void OnDisable()
    {
        chargeProgress = 0;
        meterUI.gameObject.SetActive(false);
    }
}
