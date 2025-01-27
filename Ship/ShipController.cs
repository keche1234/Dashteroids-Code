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

    [Header("Dash")]
    [SerializeField] private float chargeDuration;
    private float chargeProgress;
    [SerializeField] private float dashDuration; // duration of an initial dash
    [SerializeField] private float comboDuration; // duration of an dash off of a burst combo
    [SerializeField] private float dashTurnStrength;
    [SerializeField] private float baseDashSpeed;

    [Header("Burst")]
    [SerializeField] private float burstRange;
    [SerializeField] private float burstCooldown;
    [SerializeField] private float maxBurstBonus;
    private float burstCooldownTimer = 0.0f;
    private float burstSpeedGrowth = 1.5f;
    private float burstingSpeed; // based on dash speed
    private float burstBonus;

    //[Header("Bullets")]
    //public GameObject bulletPrefab;
    //public ObjectPool bulletPool;
    //public float bulletSpeed;
    //public float rateOfFire; // Bullets Per Second, used for holding the fire button
    //private float fireTimer = 0;

    // Other ship scripts
    private ShipState shipState;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        shipState = GetComponent<ShipState>();
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
            }
        }

        if (shipState.IsDashing())
        {
            //Debug.Log("Dashing now...");
            if (Input.GetKeyDown(chargeBtn) && burstCooldownTimer <= 0.0f)
            {
                // Use RayCast to detect asteroid nearby for burst
                Debug.DrawRay(transform.position - (transform.right * (transform.localScale.x / 2)), transform.up * (burstRange + 0.5f), Color.green, 4f);
                Debug.DrawRay(transform.position, transform.up * (burstRange + 0.5f), Color.blue, 4f);
                Debug.DrawRay(transform.position + (transform.right * (transform.localScale.x / 2)), transform.up * (burstRange + 0.5f), Color.green, 4f);
                RaycastHit2D hitInfo;
                if (hitInfo = Physics2D.CircleCast(transform.position, (transform.localScale.x / 2), transform.up, burstRange + 0.5f, LayerMask.GetMask("Asteroid")))
                {
                    // TODO: Screen Wrap Burst:
                    // if ship has screenwrapper component and is less than burstRange from edge in any direction
                    // start another Circlecast at wrap point

                    burstBonus = Mathf.Max(burstSpeedGrowth, maxBurstBonus * ((burstRange - hitInfo.distance) / burstRange));
                    shipState.StartBurst();
                }
                else
                    burstCooldownTimer = burstCooldown;
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
                    playerRb.AddForce(transform.up * thrust);
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
    }
}
