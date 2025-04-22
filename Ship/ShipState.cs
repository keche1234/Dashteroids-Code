using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;


[RequireComponent(typeof(Rigidbody2D))]
public class ShipState : MonoBehaviour
{
    protected GameManager gameManager;
    protected TimeManager timeManager;
    protected PlayerManager playerManager;
    protected BountyManager bountyManager;

    [Header("Respawning")]
    //[SerializeField] protected int startingLives;
    [SerializeField] protected float respawnDelay;
    [SerializeField] protected float respawnInvinicibilityTime;
    [SerializeField] protected GameObject respawnCanvas;
    [SerializeField] protected MeterUI respawnUI;
    //[SerializeField] protected LifeUI lifeCounter;

    //private int currentLives;
    private float respawnTimer;
    private float invincTimer;
    private float burstMercyInvincMod = 1.5f;
    private float burstMercyInvincTimer;

    private bool alive = true;

    //[Header("Dash State")]
    protected DashState dashState = DashState.Neutral;
    protected HitState hitState = HitState.None;
    protected ShipState hitCause = null;
    protected float superDashTimer;
    protected float burstDashTimer;
    protected Rigidbody2D playerRb;

    protected Vector2 lastSpeed; // for reciprocal ship interactions
    protected DashState lastDashState; // for reciprocal ship interactions

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
    protected float burstClashMod = 4f / 3;

    protected CinemachineImpulseSource impulseSource;

    [Header("Bonk and Spin States")]
    [SerializeField] protected float baseBonkTime;
    [SerializeField] protected float baseSpinTime;
    [SerializeField] protected float baseSpinLinearSpeed;
    [SerializeField] protected float baseSpinAngularSpeed;
    [Tooltip("Visual effects of Spin starts to decay at this time")]
    [SerializeField] protected float spinDecayAt;
    [Tooltip("The amount of time it takes to reset the target that hit me once Bonk or Spin ends")]
    [SerializeField] protected float hitMeClearTime;
    [SerializeField] protected float spinTrailDecayRate;
    protected float bonkTimer;
    protected float spinTimer;
    protected float spinAngularSpeed;
    protected float SPIN_CONSTANT = 20f;

    protected ShipState lastHitMe; // who put me in Bonk or Spin last? They get a point if I hit an asteroid
    protected float hitMeTimer; // clears hitMe
    protected Dictionary<ShipState, float> superDashHits; // prevents Super Dash double hits
    protected float DOUBLE_HIT_CLEAR_TIME = 0.1f; // clears superDashHits

    [Header("ScoreAttack Scoring")]
    [Tooltip("Points per units travelled while dashing")]
    [SerializeField] protected float dashPoints;
    [Tooltip("Base value of time earned when destroying an Asteroid")]
    [Range(0, float.MaxValue)]
    [SerializeField] protected float baseTimeBonus;
    [SerializeField] protected MessageFadeUI stateMessage;
    [SerializeField] protected ScoreManager scoreManager;
    protected float comboScore = 0;
    protected bool comboStarted;

    [Header("BattleMode Scoring")]
    [SerializeField] protected int minScore;
    [SerializeField] protected int maxScore;
    [Tooltip("Points other players earn by knocking this player into an asteroid")]
    [SerializeField] protected int spinKoValue;
    [Tooltip("Points other players earn by burst dashing through this player")]
    [SerializeField] protected int burstKoValue;
    [Tooltip("Points added to the player's score if they run into an Asteroid")]
    [SerializeField] protected int selfDestructPenalty;
    [Tooltip("ADDITIONAL points earned per Spin KO on if they have a bounty on them")]
    [SerializeField] protected int spinBountyValue;
    [Tooltip("ADDITIONAL points earned per Burst KO if they have a bounty on them")]
    [SerializeField] protected int burstBountyValue;
    [Tooltip("ADDITIONAL Points added to the player's score if they run into an Asteroid while they have a bounty on them")]
    [SerializeField] protected int selfDestructBountyPenalty;
    protected int arenaBattleScore = 0;

    protected static int shipCount = 0;

    private SpriteRenderer sprite;
    [Header("Multiplayer Visuals")]
    [SerializeField] private SpriteRenderer multiplayerIcon;
    [SerializeField] private BattleScoreUI scoreUI;

    // Other ship scripts
    private ShipController shipControl;

    private void Awake()
    {
        // TODO: Set the ship number properly
        shipCount++;
        //name = "Player " + shipCount.ToString();

        //currentLives = startingLives;
        respawnTimer = 0;
        invincTimer = 0;

        shipControl = GetComponent<ShipController>();
        sprite = GetComponent<SpriteRenderer>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        dashTrail.time = 0.0f;
        dashTrail.emitting = false;

        playerRb = GetComponent<Rigidbody2D>();

        respawnCanvas.transform.parent = transform.parent;
        respawnCanvas.transform.position = Vector3.zero;
        stateMessage.transform.parent.SetParent(transform.parent);

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        timeManager = GameObject.Find("GameManager").GetComponent<TimeManager>();
        playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
        bountyManager = gameManager.GetBountyManager();

        ScoreManager[] sm = FindObjectsByType<ScoreManager>(FindObjectsSortMode.None);
        if (sm == null || sm.Length < 1)
            Debug.LogWarning("No score manager found in scene!");
        else
            scoreManager = sm[0];

        if (minScore > maxScore)
            Debug.LogWarning("Min Score should not be greater than Max Score! Setting Max Score to Min Score.");

        superDashHits = new Dictionary<ShipState, float>();

        if (gameManager.GetGameMode() == GameManager.GameMode.ArenaBattle)
        {
            PaletteSet palette = playerManager.GetPaletteSet(shipCount - 1);
            if (palette != null)
            {
                sprite.color = palette.shipColor;
                multiplayerIcon.sprite = palette.iconSprite;
                multiplayerIcon.color = palette.iconColor;
                //multiplayerIcon.transform.SetParent(transform.parent);

                superDashStartColor = palette.superDashStartColor;
                superDashEndColor = palette.superDashEndColor;
                burstDashStartColor = palette.burstDashStartColor;
                burstDashEndColor = palette.burstDashEndColor;
            }

            scoreUI = playerManager.GetScoreUI(shipCount - 1);
            if (scoreUI != null)
            {
                scoreUI.gameObject.SetActive(true);
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        LifeUpdate();
        switch (hitState)
        {
            case HitState.Bonk:
                BonkUpdate();
                break;
            case HitState.Spin:
                SpinUpdate();
                break;
            default:
                DashUpdate();
                break;
        }
        lastSpeed = playerRb.linearVelocity;
        lastDashState = dashState;

        if (hitMeTimer > 0)
            hitMeTimer -= Time.deltaTime;
        else
            lastHitMe = null;
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
                    if (multiplayerIcon)
                        multiplayerIcon.enabled = true;

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
                transform.localScale = baseSize;

                if (superDashTimer > 0.0f)
                {
                    if (gameManager.GetGameMode() == GameManager.GameMode.ScoreAttack)
                    {
                        scoreManager.AddScore(playerRb.linearVelocity.magnitude * Time.deltaTime * dashPoints, false);
                        if (comboStarted)
                            comboScore += playerRb.linearVelocity.magnitude * Time.deltaTime * dashPoints;
                    }
                    dashTrail.emitting = true;
                }
                else
                {
                    dashState = DashState.Neutral;
                    if (gameManager.GetGameMode() == GameManager.GameMode.ScoreAttack)
                        scoreManager.ResetMultiplier();
                    if (comboStarted)
                    {
                        if (gameManager.GetGameMode() == GameManager.GameMode.ScoreAttack)
                        {
                            stateMessage.SetMessage(comboScore.ToString("0") + " pts.", true);
                            stateMessage.transform.parent.GetComponent<FollowTargetUI>().enabled = true;
                            stateMessage.transform.localPosition = Vector3.zero;
                        }
                    }
                    comboScore = 0;
                    //invincTimer = mercyFinishInvinc;
                }
                superDashTimer -= Time.deltaTime;

                if (burstMercyInvincTimer > 0f)
                {
                    transform.localScale = baseSize * Mathf.Clamp((1f + ((burstMaxShipScale - 1f) * remainingBurstPercentage)), 1f, burstMaxShipScale);
                    dashTrail.widthMultiplier = Mathf.Clamp((1f + ((burstMaxShipScale - 1f) * remainingBurstPercentage)), 1f, burstMaxShipScale);
                    dashTrail.emitting = true;

                    burstDashTimer -= Time.deltaTime;
                    burstDashTimer = Mathf.Clamp(burstDashTimer, 0f, shipControl.GetBurstDuration());

                    burstMercyInvincTimer -= Time.deltaTime;
                    burstMercyInvincTimer = Mathf.Clamp(burstMercyInvincTimer, 0f, shipControl.GetBurstDuration() * burstMercyInvincMod);

                    dashTrail.startColor = burstDashStartColor;
                    dashTrail.endColor = burstDashEndColor;
                }
                else
                {
                    dashTrail.startColor = superDashStartColor;
                    dashTrail.endColor = superDashEndColor;
                }

                foreach (ShipState ship in superDashHits.Keys)
                {
                    if (superDashHits[ship] > 0)
                        superDashHits[ship] -= Time.deltaTime;
                }

                break;
            case DashState.Bursting:
                // Add Score
                dashTrail.time = Mathf.Clamp(dashTrail.time + (trailRate * Time.deltaTime), 0.0f, burstDashTimer * trailRate);
                if (burstMercyInvincTimer > 0f)
                {
                    if (gameManager.GetGameMode() == GameManager.GameMode.ScoreAttack)
                    {
                        scoreManager.AddScore(playerRb.linearVelocity.magnitude * Time.deltaTime * dashPoints, false);
                        if (comboStarted)
                            comboScore += playerRb.linearVelocity.magnitude * Time.deltaTime * dashPoints;
                    }

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
                    if (gameManager.GetGameMode() == GameManager.GameMode.ScoreAttack) scoreManager.ResetMultiplier();
                    burstDashTimer = 0f;
                }

                burstDashTimer -= Time.deltaTime;
                burstMercyInvincTimer -= Time.deltaTime;
                break;
            default:
                dashTrail.emitting = false;
                transform.localScale = baseSize;
                break;
        }
    }

    public void BonkUpdate()
    {
        if (bonkTimer > 0f)
            bonkTimer -= Time.deltaTime;
        else
        {
            hitState = HitState.None;
            superDashTimer = 0;
            burstMercyInvincTimer = 0;
        }
    }

    public void SpinUpdate()
    {
        if (spinTimer > 0f)
        {
            float decay = Mathf.Clamp(spinTimer, 0, spinDecayAt);
            transform.Rotate(0, 0, spinAngularSpeed * (decay / spinDecayAt) * Time.deltaTime);
            spinTimer -= Time.deltaTime;
            dashTrail.time = Mathf.Clamp(dashTrail.time - (spinTrailDecayRate * Time.deltaTime), 0, spinTimer);
        }
        else
        {
            hitState = HitState.Bonk;
            bonkTimer = baseBonkTime / 2f;
            superDashTimer = 0;
            burstMercyInvincTimer = 0;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!timeManager.GetTimeUp())
        {
            GameObject target;
            Asteroid asteroid;
            ShipState opponent;

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

                        switch (gameManager.GetGameMode())
                        {
                            case GameManager.GameMode.ScoreAttack:
                                scoreManager.AddScore(asteroid.GetScoreValue(), false);
                                scoreManager.IncrementMultiplier();

                                float timeBonus = baseTimeBonus * scoreManager.GetScoreMultiplier();
                                timeManager.AddTime(timeBonus);
                                stateMessage.gameObject.SetActive(true);
                                stateMessage.SetMessage("+" + timeBonus.ToString("0.0###") + "s", true);
                                stateMessage.transform.parent.GetComponent<FollowTargetUI>().enabled = false;
                                stateMessage.transform.position = transform.position;
                                break;
                            default:
                                break;
                        }

                        SuperDashCombo(0.25f);

                        //invincTimer = burstDashTimer;
                    }
                    else if (dashState == DashState.SuperDashing && burstMercyInvincTimer > 0f)
                    {
                        asteroid.BreakAsteroid(normal);

                        if (gameManager.GetGameMode() == GameManager.GameMode.ScoreAttack)
                            scoreManager.AddScore(asteroid.GetScoreValue(), false);
                        canDoubleBurst = true;
                        //StartSuperDash(shipControl.GetComboDuration());
                    }
                    else if (invincTimer > 0)
                    {
                        // do nothing
                    }
                    else
                    {
                        Explode();
                        if (gameManager.GetGameMode() == GameManager.GameMode.ArenaBattle)
                        {
                            if (lastHitMe)
                            {
                                int points = spinKoValue + (bountyManager.HasBounty(this) ? spinBountyValue : 0);
                                lastHitMe.AddArenaBattlePoints(points);
                                CameraShakeManager.instance.CameraShake(impulseSource);
                            }
                            else
                            {
                                int penalty = selfDestructPenalty + (bountyManager.HasBounty(this) ? selfDestructBountyPenalty : 0);
                                AddArenaBattlePoints(penalty);
                            }
                        }
                    }
                }
            }
            else if (gameManager.GetGameMode() == GameManager.GameMode.ArenaBattle)
            {
                if (alive && (target = collision.gameObject) && (opponent = target.GetComponent<ShipState>()))
                {
                    if (opponent.IsAlive())
                    {
                        if (opponent.IsInvincible())
                        {
                            Bonk(null, -lastSpeed / 2f);
                        }
                        else
                        {
                            Rigidbody2D opponentRb = opponent.gameObject.GetComponent<Rigidbody2D>();
                            switch (lastDashState)
                            {
                                case DashState.Neutral:
                                    switch (opponent.GetLastDashState())
                                    {
                                        case DashState.Neutral:
                                            float opSpeedPercentage = opponent.GetLastSpeed().magnitude / (lastSpeed.magnitude + opponent.GetLastSpeed().magnitude);
                                            opponent.Bonk(this, lastSpeed, 1f - opSpeedPercentage);
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case DashState.SuperDashing:
                                    switch (opponent.GetLastDashState())
                                    {
                                        case DashState.Neutral: // Super Dash vs Neutral
                                            if (burstMercyInvincTimer > 0)
                                            {
                                                opponent.Explode();
                                                CameraShakeManager.instance.CameraShake(impulseSource);

                                                SuperDashCombo(0.25f);
                                                AddArenaBattlePoints(opponent.GetBurstKOValue() + (bountyManager.HasBounty(opponent) ? opponent.GetBurstBountyValue() : 0));
                                            }
                                            else
                                            {
                                                opponent.Spin(this, lastSpeed.normalized, baseSpinLinearSpeed, baseSpinAngularSpeed,
                                                              baseSpinTime, 1 + (lastSpeed.magnitude / SPIN_CONSTANT));
                                            }
                                            break;
                                        case DashState.SuperDashing: // Super Dash vs Super Dash
                                            if (burstMercyInvincTimer > 0)
                                            {
                                                Debug.Log("I have burst while super dashing! (" + burstMercyInvincTimer + ")");
                                                if (opponent.IsBursting()) // burst vs burst
                                                {
                                                    opponent.transform.rotation = Quaternion.LookRotation(Vector3.forward, lastSpeed.normalized);
                                                    opponentRb.linearVelocity = lastSpeed * opponentRb.linearVelocity.magnitude;
                                                }
                                                else // burst vs no brust
                                                {
                                                    opponent.Explode();
                                                    CameraShakeManager.instance.CameraShake(impulseSource);

                                                    SuperDashCombo(0.25f);
                                                    AddArenaBattlePoints(opponent.GetBurstKOValue() + (bountyManager.HasBounty(opponent) ? opponent.GetBurstBountyValue() : 0));
                                                }
                                            }
                                            else
                                            {
                                                Debug.Log("I DON\'T have burst while super dashing... (" + burstMercyInvincTimer + ")");
                                                if (opponent.IsBursting()) // no burst vs burst
                                                {
                                                    // lol get beaned (do nothing)
                                                }
                                                else // no burst vs no burst
                                                    opponent.Spin(this, lastSpeed.normalized, baseSpinLinearSpeed, baseSpinAngularSpeed, baseSpinTime);
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case DashState.Bursting:
                                    switch (opponent.GetLastDashState())
                                    {
                                        case DashState.Bursting: // Burst vs Burst
                                            opponent.transform.rotation = Quaternion.LookRotation(Vector3.forward, lastSpeed.normalized);
                                            opponentRb.linearVelocity = lastSpeed * opponentRb.linearVelocity.magnitude;

                                            SuperDashCombo(0.25f);
                                            break;
                                        default: // Burst vs Neutral/Super Dash
                                            comboStarted = true;
                                            opponent.Explode();
                                            CameraShakeManager.instance.CameraShake(impulseSource);

                                            SuperDashCombo(0.25f);
                                            AddArenaBattlePoints(opponent.GetBurstKOValue() + (bountyManager.HasBounty(opponent) ? opponent.GetBurstBountyValue() : 0));
                                            break;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void SuperDashCombo(float extraBurst)
    {
        StartSuperDash(shipControl.GetComboDuration());
        burstDashTimer = Mathf.Clamp(burstDashTimer + extraBurst, 0f, shipControl.GetBurstDuration());
        burstMercyInvincTimer = Mathf.Clamp(burstMercyInvincTimer + (extraBurst * burstMercyInvincMod),
                                            0f, shipControl.GetBurstDuration() * burstMercyInvincMod);
    }

    public void Explode()
    {
        // Explode
        alive = false;
        shipControl.enabled = false;
        sprite.enabled = false;

        // Start respawn
        respawnTimer = respawnDelay;
        respawnUI.gameObject.SetActive(true);
        respawnUI.SetSliderPercentage(1f - (respawnTimer / respawnDelay));

        // Reset dash stats
        superDashTimer = 0;

        // Stop combo Trail
        StopComboTrail();
        if (multiplayerIcon)
            multiplayerIcon.enabled = false;

        // Reset hit state
        hitState = HitState.None;

        transform.localScale = baseSize;

        //lifeCounter.SetLifeCount(currentLives);

        // Update Score Multiplier
        if (gameManager.GetGameMode() == GameManager.GameMode.ScoreAttack)
            scoreManager.ResetMultiplier();
    }

    /*
     * Bonks me
     */
    public void Bonk(ShipState cause, Vector2 velocity, float durationModifier = 1f)
    {
        playerRb.linearVelocity = velocity;

        if (hitState == HitState.None)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward, velocity.normalized);
            bonkTimer = baseBonkTime * durationModifier;
            hitState = HitState.Bonk;
            dashState = DashState.Neutral;
            spinTimer = 0;
            lastHitMe = cause;
            dashTrail.time = 0;

            transform.localScale = baseSize;
            StopComboTrail();
            burstMercyInvincTimer = 0;
        }
    }

    /*
     * Spins me
     */
    public void Spin(ShipState cause, Vector2 direction, float spinSpeed, float spinRotation, float spinTime, float impactMod = 1f)
    {
        direction = direction.normalized;
        if (hitState == HitState.Spin)
        {
            playerRb.linearVelocity += direction * spinSpeed * impactMod * Mathf.Pow(0.5f, spinTimer);
            spinAngularSpeed += spinRotation * impactMod * Mathf.Pow(0.5f, spinTimer);
            spinTimer += spinTime * impactMod * Mathf.Pow(0.5f, spinTimer);
        }
        else
        {
            playerRb.linearVelocity = direction * spinSpeed * impactMod;
            spinAngularSpeed = spinRotation * impactMod;
            spinTimer = spinTime * impactMod;
        }
        transform.localScale = baseSize;
        StopComboTrail();

        hitState = HitState.Spin;
        dashState = DashState.Neutral;

        lastHitMe = cause;
        hitMeTimer = hitMeClearTime;
    }

    /*********
     * States
     *********/
    public bool IsAlive()
    {
        return alive;
    }

    public bool IsInvincible()
    {
        return invincTimer > 0;
    }

    public DashState GetDashState()
    {
        return dashState;
    }

    // For collision usage ONLY
    public DashState GetLastDashState()
    {
        return lastDashState;
    }

    public Vector2 GetLastSpeed()
    {
        return lastSpeed;
    }

    public void SetDashState(DashState newState)
    {
        dashState = newState;
    }

    public HitState GetHitState()
    {
        return hitState;
    }

    public void SetHitState(HitState newState)
    {
        hitState = newState;
    }

    public bool CanBurstAgain()
    {
        return canDoubleBurst;
    }

    public bool IsBursting()
    {
        return dashState == DashState.Bursting || burstMercyInvincTimer > 0;
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

        superDashHits = new Dictionary<ShipState, float>();
    }

    /*********************
     * Multiplayer Points
     *********************/
    public int GetSpinKOValue()
    {
        return spinKoValue;
    }

    public int GetBurstKOValue()
    {
        return burstKoValue;
    }

    public int GetSpinBountyValue()
    {
        return spinBountyValue;
    }

    public int GetBurstBountyValue()
    {
        return burstBountyValue;
    }

    public void AddArenaBattlePoints(int points)
    {
        arenaBattleScore += points;
        arenaBattleScore = Mathf.Clamp(arenaBattleScore, minScore, maxScore);
        scoreUI.score.text = arenaBattleScore.ToString("00.#") + " pts.";

        stateMessage.gameObject.SetActive(true);
        string txt = (points > 0 ? "+" : "") + points.ToString("0.#") + " pts.";
        stateMessage.SetMessage(txt, true);
        stateMessage.transform.parent.GetComponent<FollowTargetUI>().enabled = false;
        stateMessage.transform.position = transform.position;

        bountyManager.NeedToComputeBounty();
    }

    public int GetArenaBattleScore()
    {
        return arenaBattleScore;
    }

    public void SetPlayerNumberFromIndex(int i)
    {
        name = "Player " + (i + 1);
        scoreUI?.gameObject.SetActive(false);
        SetPaletteFromIndex(i);
    }

    protected void SetPaletteFromIndex(int i)
    {
        PaletteSet palette = playerManager.GetPaletteSet(i);
        if (palette != null)
        {
            sprite.color = palette.shipColor;
            multiplayerIcon.sprite = palette.iconSprite;
            multiplayerIcon.color = palette.iconColor;
            //multiplayerIcon.transform.SetParent(transform.parent);

            superDashStartColor = palette.superDashStartColor;
            superDashEndColor = palette.superDashEndColor;
            burstDashStartColor = palette.burstDashStartColor;
            burstDashEndColor = palette.burstDashEndColor;
        }

        scoreUI = playerManager.GetScoreUI(i);
        scoreUI?.gameObject.SetActive(true);
    }

    /************************************
     * Initiate a burst for `t` seconds.
     ************************************/
    public void StartBurstDash(float t)
    {
        burstDashTimer = t;
        burstMercyInvincTimer = t * burstMercyInvincMod;
        dashTrail.emitting = true;
        dashTrail.time = t * trailRate;
        dashTrail.startColor = burstDashStartColor;
        dashTrail.endColor = burstDashEndColor;

        dashState = DashState.Bursting;
        canDoubleBurst = false;
    }

    public void StopComboTrail()
    {
        // Stop combo Trail
        dashTrail.widthMultiplier = 1f;
        dashTrail.emitting = false;
        dashTrail.time = 0;
        dashTrail.startColor = burstDashStartColor;
        dashTrail.endColor = burstDashEndColor;
    }

    private void OnDestroy()
    {
        shipCount--;
    }

    public enum DashState
    {
        Neutral = 0b_001,
        SuperDashing = 0b_010,
        Bursting = 0b_100
    }

    public enum HitState
    {
        None = 0b_001,
        Bonk = 0b_010,
        Spin = 0b_100,
    }
}
