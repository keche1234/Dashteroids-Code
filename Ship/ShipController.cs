using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShipController : MonoBehaviour
{
    [Header("Controls")]
    public KeyCode forwardBtn;
    public KeyCode turnLeftBtn;
    public KeyCode turnRightBtn;
    public KeyCode chargeBtn;

    [Header("Movement")]
    public float thrust;
    public float turnStrength;
    public float maxFlightSpeed;
    public float maxRotationSpeed;
    private Rigidbody2D playerRb;

    [Header("Dash")]
    public float chargeDuration;
    private float chargeProgress;
    public float dashDuration;
    public float extendDuration;
    public float dashTurnStrength;
    public float baseDashSpeed;

    [Header("Burst")]
    public float burstCooldown;
    private float burstMod = 4.0f;
    private float burstBonus = 1.0f;

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
                Debug.Log("Charge: " + (chargeProgress / chargeDuration));
            }
            else if (Input.GetKeyUp(chargeBtn))
            {
                if (chargeProgress >= chargeDuration)
                {
                    shipState.StartDash(dashDuration);
                    Debug.Log("Dash!");
                }
                chargeProgress = 0;
            }
        }

        if (shipState.IsDashing())
        {
            // TODO: Use RayCast to detect asteroid nearby for burst
            // Set burstBonus as well

            if (shipState.IsBursting())
            {
                playerRb.linearVelocity = transform.up * baseDashSpeed * burstMod;
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
    }

    private void OnDisable()
    {
        chargeProgress = 0;
    }
}
