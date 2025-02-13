using UnityEngine;
using Unity.Cinemachine;


public class ShipState : MonoBehaviour
{
    [Header("Respawning")]
    //[SerializeField] protected int startingLives;
    [SerializeField] protected float respawnDelay;
    [SerializeField] protected float respawnInvinicibilityTime;
    [SerializeField] protected MeterUI respawnUI;
    //[SerializeField] protected LifeUI lifeCounter;

    //private int currentLives;
    private float respawnTimer;
    private float invincTimer;
    private float burstMercyInvicMod = 1.5f;
    private float burstMercyInvicTimer;

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
    [SerializeField] protected TrailRenderer dashTrail;
    [SerializeField] protected Color superDashStartColor;
    [SerializeField] protected Color superDashEndColor;
    [SerializeField] protected Color burstDashStartColor;
    [SerializeField] protected Color burstDashEndColor;
    protected int scoreMultiplier = 1;

    [Header("Burst Dash Size Scaling")]
    [SerializeField] protected Vector3 baseSize;
    [Range(1f, float.MaxValue)]
    [SerializeField] protected float burstMaxShipScale;
    protected bool canDoubleBurst = false;

    protected CinemachineImpulseSource impulseSource;

    //[Header("Dash Mercy")]
    //[Tooltip("Mercy invincibility applied after starting combo, in addition to the remaining burstTime.")]
    //[SerializeField] protected float mercyComboInvinc;
    //[Tooltip("Mercy invincibility applied after a dash or burst ends.")]
    //[SerializeField] protected float mercyFinishInvinc;

    [Header("Scoring")]
    [Tooltip("Points per units travelled while dashing")]
    [SerializeField] protected float dashPoints;
    [Tooltip("Base value of time earned when destroying an Asteroid")]
    [Range(0, float.MaxValue)]
    [SerializeField] protected float baseTimeBonus;
    [SerializeField] protected MessageFadeUI stateMessage;
    [SerializeField] protected ScoreManager scoreManager;
    protected float comboScore = 0;
    protected bool comboStarted;

    [Header("Timer")]
    [SerializeField] protected TimeManager timeManager;

    private SpriteRenderer sprite;

    // Other ship scripts
    private ShipController shipControl;

    private void Awake()
    {
        //currentLives = startingLives;
        respawnTimer = 0;
        invincTimer = 0;

        shipControl = GetComponent<ShipController>();
        sprite = GetComponent<SpriteRenderer>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        dashTrail.time = 0.0f;
        dashTrail.emitting = false;

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
    }

    public void LifeUpdate()
    {
        if (timeManager.GetTimeUp())
        {
            respawnUI.gameObject.SetActive(false);
        }
        else
        {
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

                    dashTrail.emitting = true;
                    respawnUI.gameObject.SetActive(false);
                }
                else
                {
                    respawnTimer -= Time.deltaTime;
                    respawnUI.SetSliderPercentage(1f - (respawnTimer / respawnDelay));
                }
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
        float remainingBurstPercentage = Mathf.Clamp01(burstDashTimer / shipControl.GetBurstDuration());
        switch (dashState)
        {
            case DashState.SuperDashing:
                // Add Score
                dashTrail.time = Mathf.Clamp(dashTrail.time + (trailRate * Time.deltaTime), 0.0f, superDashTimer * trailRate);

                if (superDashTimer > 0.0f)
                {
                    scoreManager.AddScore(playerRb.linearVelocity.magnitude * Time.deltaTime * dashPoints, false);
                    if (comboStarted)
                        comboScore += playerRb.linearVelocity.magnitude * Time.deltaTime * dashPoints;
                    dashTrail.emitting = true;
                }
                else
                {
                    dashState = DashState.Neutral;
                    scoreManager.ResetMultiplier();
                    if (comboStarted)
                    {
                        stateMessage.SetMessage(comboScore.ToString("0") + " pts.", true);
                        stateMessage.transform.parent.GetComponent<FollowTargetUI>().enabled = true;
                        stateMessage.transform.localPosition = Vector3.zero;
                    }
                    comboScore = 0;
                    //invincTimer = mercyFinishInvinc;
                }
                superDashTimer -= Time.deltaTime;

                if (burstMercyInvicTimer > 0f)
                {
                    transform.localScale = baseSize * Mathf.Clamp((1f + ((burstMaxShipScale - 1f) * remainingBurstPercentage)), 1f, burstMaxShipScale);
                    dashTrail.widthMultiplier = Mathf.Clamp((1f + ((burstMaxShipScale - 1f) * remainingBurstPercentage)), 1f, burstMaxShipScale);
                    dashTrail.emitting = true;

                    burstDashTimer -= Time.deltaTime;
                    burstDashTimer = Mathf.Clamp(burstDashTimer, 0f, shipControl.GetBurstDuration());

                    burstMercyInvicTimer -= Time.deltaTime;
                    burstMercyInvicTimer = Mathf.Clamp(burstMercyInvicTimer, 0f, shipControl.GetBurstDuration() * 1.1f);

                    dashTrail.startColor = burstDashStartColor;
                    dashTrail.endColor = burstDashEndColor;
                }
                else
                {
                    dashTrail.startColor = superDashStartColor;
                    dashTrail.endColor = superDashEndColor;
                }
                break;
            case DashState.Bursting:
                // Add Score
                dashTrail.time = Mathf.Clamp(dashTrail.time + (trailRate * Time.deltaTime), 0.0f, burstDashTimer * trailRate);
                if (burstMercyInvicTimer > 0f)
                {
                    scoreManager.AddScore(playerRb.linearVelocity.magnitude * Time.deltaTime * dashPoints, false);
                    if (comboStarted)
                        comboScore += playerRb.linearVelocity.magnitude * Time.deltaTime * dashPoints;

                    transform.localScale = baseSize * Mathf.Clamp((1f + ((burstMaxShipScale - 1f) * remainingBurstPercentage)), 1f, burstMaxShipScale);
                    dashTrail.widthMultiplier = Mathf.Clamp((1f + ((burstMaxShipScale - 1f) * remainingBurstPercentage)), 1f, burstMaxShipScale);
                    dashTrail.emitting = true;
                    dashTrail.startColor = burstDashStartColor;
                    dashTrail.endColor = burstDashEndColor;
                }
                else
                {
                    dashState = DashState.Neutral;
                    shipControl.SetComboBonus(0f);
                    transform.localScale = baseSize;
                    dashTrail.widthMultiplier = 1f;
                    scoreManager.ResetMultiplier();
                    burstDashTimer = 0f;
                }

                burstDashTimer -= Time.deltaTime;
                burstMercyInvicTimer -= Time.deltaTime;
                break;
            default:
                dashTrail.emitting = false;
                break;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!timeManager.GetTimeUp())
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
                        comboStarted = true;
                        asteroid.BreakAsteroid(normal);
                        CameraShakeManager.instance.CameraShake(impulseSource);

                        scoreManager.AddScore(asteroid.GetScoreValue(), false);
                        scoreManager.IncrementMultiplier();

                        float timeBonus = baseTimeBonus * scoreManager.GetScoreMultiplier();
                        timeManager.AddTime(timeBonus);
                        stateMessage.gameObject.SetActive(true);
                        stateMessage.SetMessage("+" + timeBonus.ToString("0.0###") + "s", true);
                        stateMessage.transform.parent.GetComponent<FollowTargetUI>().enabled = false;
                        stateMessage.transform.position = transform.position;

                        StartSuperDash(shipControl.GetComboDuration());
                        burstDashTimer += 0.25f;
                        burstMercyInvicTimer += 0.25f * burstMercyInvicMod;
                        burstDashTimer = Mathf.Clamp(burstDashTimer, 0f, shipControl.GetBurstDuration());
                        burstMercyInvicTimer = Mathf.Clamp(burstMercyInvicTimer, 0f, shipControl.GetBurstDuration() * burstMercyInvicMod);

                        //invincTimer = burstDashTimer;
                    }
                    else if (dashState == DashState.SuperDashing && burstMercyInvicTimer > 0f)
                    {
                        asteroid.BreakAsteroid(normal);
                        scoreManager.AddScore(asteroid.GetScoreValue(), false);
                        canDoubleBurst = true;
                        //StartSuperDash(shipControl.GetComboDuration());
                    }
                    else if (invincTimer > 0)
                    {
                        // do nothing
                    }
                    else
                        Explode();
                }
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

        // Start respawn
        respawnTimer = respawnDelay;
        respawnUI.gameObject.SetActive(true);
        respawnUI.SetSliderPercentage(1f - (respawnTimer / respawnDelay));

        // Reset dash stats
        superDashTimer = 0;
        transform.localScale = baseSize;
        dashTrail.widthMultiplier = 1f;

        // Stop combo Trail
        dashTrail.emitting = false;
        dashTrail.time = 0;

        //lifeCounter.SetLifeCount(currentLives);

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

    public bool CanBurstAgain()
    {
        return canDoubleBurst;
    }

    /*****************************************
     * Initiate a super dash for `t` seconds.
     *****************************************/
    public void StartSuperDash(float t)
    {
        superDashTimer = t;
        transform.localScale = baseSize;
        dashTrail.emitting = true;
        dashTrail.time = t * trailRate;
        dashTrail.widthMultiplier = 1f;
        dashTrail.startColor = superDashStartColor;
        dashTrail.endColor = superDashStartColor;

        dashState = DashState.SuperDashing;

        // Update controller's speed
        shipControl.SetComboBonus(burstDashTimer / shipControl.GetBurstDuration());
    }

    /************************************
     * Initiate a burst for `t` seconds.
     ************************************/
    public void StartBurstDash(float t)
    {
        burstDashTimer = t;
        burstMercyInvicTimer = t * burstMercyInvicMod;
        dashTrail.emitting = true;
        dashTrail.time = t * trailRate;
        dashTrail.startColor = burstDashStartColor;
        dashTrail.endColor = burstDashEndColor;

        dashState = DashState.Bursting;
        canDoubleBurst = false;
    }

    public enum DashState
    {
        Neutral = 0b_001,
        SuperDashing = 0b_010,
        Bursting = 0b_100
    }
}
