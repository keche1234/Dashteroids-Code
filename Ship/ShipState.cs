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

    //[Header("Dash State")]
    protected DashState dashState = DashState.Neutral;
    protected float superDashTimer;
    protected float burstDashTimer;
    protected Rigidbody2D playerRb;

    [Header("Super Dash Trail")]
    [Tooltip("Combo Trail time is set to Time Remaining on Dash * trailRate")]
    [SerializeField] protected float trailRate; // combo trail is rendered with time = dashTimer * trailRate
    [SerializeField] protected float trailDecay; //rate of decay of the time on the combo trail
    [SerializeField] protected TrailRenderer superDashTrail;
    protected int scoreMultiplier = 1;

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

        superDashTrail.time = 0.0f;
        superDashTrail.emitting = false;

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
        //if (bursting)
        //{
        //    if (burstTimer <= 0)
        //        bursting = false;

        //    burstTimer -= Time.deltaTime;
        //}
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

                    superDashTrail.emitting = true;
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
        switch (dashState)
        {
            case DashState.SuperDashing:
                // Add Score
                superDashTrail.time = Mathf.Clamp(superDashTrail.time + (trailRate * Time.deltaTime), 0.0f, superDashTimer * trailRate);

                if (superDashTimer > 0.0f)
                {
                    scoreManager.AddScore(playerRb.linearVelocity.magnitude * Time.deltaTime * dashPoints, false);
                    superDashTrail.emitting = true;
                }
                else
                {
                    dashState = DashState.Neutral;
                    scoreManager.ResetMultiplier();
                }
                superDashTimer -= Time.deltaTime;
                break;
            case DashState.Bursting:
                // Add Score
                superDashTrail.time = Mathf.Clamp(superDashTrail.time + (trailRate * Time.deltaTime), 0.0f, burstDashTimer * trailRate);

                if (burstDashTimer > 0.0f)
                {
                    scoreManager.AddScore(playerRb.linearVelocity.magnitude * Time.deltaTime * dashPoints, false);
                    superDashTrail.emitting = true;
                }
                else
                {
                    dashState = DashState.Neutral;
                    scoreManager.ResetMultiplier();
                }

                burstDashTimer -= Time.deltaTime;
                break;
            default:
                break;
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
                if (dashState == DashState.Bursting)
                {
                    asteroid.BreakAsteroid(normal);

                    scoreManager.AddScore(asteroid.GetScoreValue(), false);
                    scoreManager.IncrementMultiplier();

                    StartSuperDash(shipControl.GetComboDuration());
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
        superDashTimer = 0;
        bursting = false;

        // Stop combo Trail
        superDashTrail.emitting = false;
        superDashTrail.time = 0;

        // Update life counter
        lifeCounter.SetLifeCount(currentLives);

        // Update Score Multiplier
        scoreManager.ResetMultiplier();
    }

    public bool IsAlive()
    {
        return alive;
    }

    public DashState GetDashState()
    {
        return dashState;
    }

    public void SetDashState(DashState newState)
    {
        dashState = newState;
    }

    /*****************************************
     * Initiate a super dash for `t` seconds.
     *****************************************/
    public void StartSuperDash(float t)
    {
        superDashTimer = t;
        superDashTrail.emitting = true;
        superDashTrail.time = trailRate;

        dashState = DashState.SuperDashing;
    }

    /************************************
     * Initiate a burst for `t` seconds.
     ************************************/
    public void StartBurstDash(float t)
    {
        burstDashTimer = t;
        superDashTrail.emitting = true;
        superDashTrail.time = trailRate;

        dashState = DashState.Bursting;
    }

    //public bool IsDashing()
    //{
    //    return dashTimer > 0;
    //}

    //public bool IsBursting()
    //{
    //    return bursting;
    //}

    public enum DashState
    {
        Neutral = 0b_001,
        SuperDashing = 0b_010,
        Bursting = 0b_100
    }
}
