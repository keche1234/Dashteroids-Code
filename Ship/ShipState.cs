using UnityEngine;

public class ShipState : MonoBehaviour
{
    [Header("Lives")]
    public int startingLives;
    public float respawnDelay;
    public float respawnInvinicibilityTime;
    public float burstInvincibilityTime;

    private int currentLives;
    private float respawnTimer;
    private float invincTimer;

    private bool alive = true;

    //[Header("Dashing")]
    private float dashTimer;

    //[Header("Bursting")]
    private bool bursting;
    private float burstActiveTime = 1f; // paranoia: burst should end upon contacting asteroid
    private float burstTimer = 0f; // paranoia: brust should end upon contacting asteroid

    private SpriteRenderer sprite;

    // Other ship scripts
    private ShipController shipControl;

    private void Awake()
    {
        currentLives = startingLives;
        respawnTimer = 0;
        invincTimer = 0;

        shipControl = GetComponent<ShipController>();
        sprite = GetComponent<SpriteRenderer>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        LifeUpdate();
        DashUpdate();

        // paranoia
        if (bursting)
        {
            if (burstTimer <= 0)
                bursting = false;

            burstTimer -= Time.deltaTime;
        }
    }

    public void LifeUpdate()
    {
        if (currentLives <= 0)
        {
            // Game Over!
            Debug.Log("Game over!");
            this.enabled = false;
        }
        else
        {
            //Debug.Log("Invinc Timer: " + invincTimer);
            if (!alive)
            {
                if (respawnTimer <= 0)
                {
                    alive = true;
                    shipControl.enabled = true;
                    sprite.enabled = true;

                    respawnTimer = respawnDelay;
                    invincTimer = respawnInvinicibilityTime;

                    transform.position = Vector3.zero;
                    transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
                    GetComponent<Rigidbody2D>().linearVelocity = Vector3.zero;
                    GetComponent<Rigidbody2D>().angularVelocity = 0f;
                }
                else
                    respawnTimer -= Time.deltaTime;
            }
            else
            {
                if (invincTimer > 0)
                {
                    invincTimer -= Time.deltaTime;
                }
            }
        }
    }

    public void DashUpdate()
    {
        dashTimer -= Time.deltaTime;
        dashTimer = Mathf.Clamp(dashTimer, 0, dashTimer);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject target;
        Asteroid asteroid;

        if ((target = collision.gameObject) && (asteroid = target.GetComponent<Asteroid>()))
        {
            if (alive)
            {
                Vector2 normal = (transform.position - target.transform.position).normalized;
                if (IsBursting())
                {
                    asteroid.BreakAsteroid(normal);
                    StartDash(shipControl.GetComboDuration());
                    invincTimer = burstInvincibilityTime;
                    bursting = false;
                }
                else if (invincTimer > 0)
                    asteroid.BreakAsteroid(normal);
                else
                    Explode();
            }
        }
    }

    public void Explode()
    {
        // Explode
        Debug.Log("BOOM!");
        alive = false;
        shipControl.enabled = false;
        sprite.enabled = false;
        currentLives--;

        // Start respawn
        respawnTimer = respawnDelay;

        // Reset dash stats
        dashTimer = 0;
        bursting = false;
    }

    public bool IsAlive()
    {
        return alive;
    }

    /*
     * Initiate a dash for `t` seconds.
     */
    public void StartDash(float t)
    {
        dashTimer = t;
    }

    /*
     * Sets the player state to bursting
     */
    public void StartBurst()
    {
        bursting = true;
        burstTimer = burstActiveTime;
        invincTimer = burstInvincibilityTime;
    }

    public bool IsDashing()
    {
        return dashTimer > 0;
    }

    public bool IsBursting()
    {
        return bursting;
    }
}
