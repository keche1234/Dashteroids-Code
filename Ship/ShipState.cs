using UnityEngine;

public class ShipState : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] protected int startingLives;
    [SerializeField] protected float respawnDelay;
    [SerializeField] protected float respawnInvinicibilityTime;
    [SerializeField] protected float burstInvincibilityTime;
    [SerializeField] protected LifeUI lifeCounter;

    private int currentLives;
    private float respawnTimer;
    private float invincTimer;

    private bool alive = true;

    [Header("Dashing")]
    [Tooltip("Combo Trail time is set to Time Remaining on Dash * trailRate")]
    [SerializeField] protected float trailRate; // combo trail is rendered with time = dashTimer * trailRate
    [SerializeField] protected float trailDecay; //rate of decay of the time on the combo trail
    [SerializeField] protected TrailRenderer comboTrail;
    protected int scoreMultiplier = 1;
    protected float dashTimer;
    protected Rigidbody2D playerRb;

    [Header("Scoring")]
    [Tooltip("Points per units travelled while dashing")]
    [SerializeField] protected float dashPoints;
    [SerializeField] protected ScoreManager scoreManager;

    //[Header("Bursting")]
    private bool bursting;
    private float burstActiveTime = 0.2f; // paranoia: burst should end upon contacting asteroid
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

        comboTrail.time = 0.0f;
        comboTrail.emitting = false;

        playerRb = GetComponent<Rigidbody2D>();
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

                    comboTrail.emitting = true;
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
        if (dashTimer > 0)
        {
            // Add Score
            scoreManager.AddScore(playerRb.linearVelocity.magnitude * Time.deltaTime * dashPoints, false);

            dashTimer -= Time.deltaTime;
            comboTrail.time = Mathf.Clamp(comboTrail.time + (trailRate * Time.deltaTime), 0.0f, dashTimer * trailRate);
            comboTrail.emitting = true;

            if (dashTimer <= 0.0f)
            {
                invincTimer = 0.5f;
                dashTimer = 0.0f;
                comboTrail.emitting = false;
                scoreManager.ResetMultiplier();
            }
        }
        else
        {
            comboTrail.time = 0.0f;
            comboTrail.emitting = false;
        }
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

                    scoreManager.AddScore(asteroid.GetScoreValue(), false);
                    scoreManager.IncrementMultiplier();

                    StartDash(shipControl.GetComboDuration());
                    invincTimer = burstInvincibilityTime;
                    bursting = false;
                }
                else if (invincTimer > 0 || !asteroid.CanDestroyPlayer())
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

        // Stop combo Trail
        comboTrail.emitting = false;
        comboTrail.time = 0;

        // Update life counter
        lifeCounter.SetLifeCount(currentLives);

        // Update Score Multiplier
        scoreManager.ResetMultiplier();
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
        comboTrail.emitting = true;
        comboTrail.time = trailRate;
    }

    /*
     * Sets the player state to bursting
     */
    public void StartBurst()
    {
        bursting = true;
        burstTimer = burstActiveTime;
        //invincTimer = burstInvincibilityTime;
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
