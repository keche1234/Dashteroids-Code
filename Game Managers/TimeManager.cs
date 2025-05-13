using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class TimeManager : MonoBehaviour
{
    protected DashteroidsActions inputActions;
    protected InputSystemUIInputModule inputSystemUIInputModule;
    [SerializeField] protected ShipController[] shipControllers;

    [Header("Timer")]
    [Range(1f, 9000f)]
    [Tooltip("In seconds")]
    [SerializeField] protected float startingTime;
    [Range(1f, 9000f)]
    [SerializeField] protected float maxTime;
    [SerializeField] protected float bountyBeginsAt;
    [SerializeField] protected TextMeshProUGUI timerUI;
    protected float timeRemaining; // in seconds
    protected bool timeUp = false;

    protected float resultsTimer = 0;

    [Header("Game State")]
    [SerializeField] protected GameObject gameOverMessage;
    [SerializeField] protected GameObject audioSourceObject;
    [SerializeField] protected AudioClip timeOverSound;
    [SerializeField] protected GameObject gameOverOptions;
    [SerializeField] protected GameObject firstGameOverOption;
    [SerializeField] protected GameObject gameOverLeaderboard;
    protected AudioSource audioSource;
    protected MusicLoop audioMusicLoop;
    protected bool victoryClipPlayed = false;

    void Awake()
    {
        timeRemaining = startingTime;
        audioSource = audioSourceObject.GetComponent<AudioSource>();
        audioMusicLoop = audioSourceObject.GetComponent<MusicLoop>();

        inputActions = new DashteroidsActions();
        inputActions.Player.Enable();
        inputActions.UI.Enable();
        inputActions.bindingMask = InputBinding.MaskByGroup(inputActions.GamepadScheme.bindingGroup);

        shipControllers = FindObjectsByType<ShipController>(FindObjectsSortMode.None);
        inputSystemUIInputModule = GameObject.Find("EventSystem").GetComponent<InputSystemUIInputModule>();
    }

    public void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.UI.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;
            timeRemaining = Mathf.Clamp(timeRemaining, 0f, timeRemaining);
            int minutes = (int)timeRemaining / 60;
            int seconds = (int)timeRemaining % 60;
            float milliseconds = timeRemaining - Mathf.Floor(timeRemaining);

            int displayedSeconds = milliseconds > 0f ? seconds + 1 : seconds;
            int displayedMinutes = minutes;
            if (displayedSeconds == 60)
            {
                displayedSeconds = 0;
                displayedMinutes += 1;
            }


            timerUI.text = displayedMinutes.ToString("0") + ":" + displayedSeconds.ToString("00");
            if (timeRemaining <= 1f && audioSource)
            {
                audioSource.volume = timeRemaining;
            }
        }
        else
        {
            if (!timeUp)
            {
                gameOverMessage.SetActive(true);

                switch (GetComponent<GameManager>().GetGameMode())
                {
                    case GameManager.GameMode.ScoreAttack:
                        gameOverOptions.SetActive(true);
                        EventSystem.current.SetSelectedGameObject(firstGameOverOption);
                        break;
                    case GameManager.GameMode.ArenaBattle:
                        gameOverLeaderboard.SetActive(true);
                        break;
                    default:
                        break;
                }
                GetComponent<GameManager>().EndGame();
                shipControllers = FindObjectsByType<ShipController>(FindObjectsSortMode.None);
            }
            timeUp = true;

            if (audioSource && !victoryClipPlayed)
            {
                audioSource.loop = false;
                audioSource.Stop();
                audioSource.volume = 1f;
                audioSource.PlayOneShot(timeOverSound);
                victoryClipPlayed = true;
            }

            if (GetComponent<GameManager>().GetGameMode() == GameManager.GameMode.ArenaBattle)
            {
                if (resultsTimer < 1f)
                {
                    resultsTimer += Time.deltaTime;
                }
                else
                {
                    if (gameOverLeaderboard.activeSelf && inputActions.UI.Submit.WasPressedThisFrame())
                    {
                        gameOverLeaderboard.SetActive(false);
                        gameOverOptions.SetActive(true);
                        GetComponent<GameManager>().AddActionBuffer(1f / 60);
                        EventSystem.current.SetSelectedGameObject(firstGameOverOption);
                        foreach (ShipController ship in shipControllers)
                        {
                            MultiplayerEventSystem eventSystem = ship.gameObject.GetComponent<MultiplayerEventSystem>();
                            eventSystem.SetSelectedGameObject(firstGameOverOption);
                        }
                    }
                }
            }
        }

        //if (inputActions.Player.Start.WasPerformedThisFrame())
        //    SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        if (Input.GetKeyDown(KeyCode.Q))
            Application.Quit();
    }

    public float GetTimeRemaining()
    {
        return timeRemaining;
    }

    public bool GetTimeUp()
    {
        return timeUp;
    }

    public bool BountyIsActive()
    {
        return timeRemaining <= bountyBeginsAt;
    }

    public void AddTime(float t)
    {
        timeRemaining += t;
        timeRemaining = Mathf.Clamp(timeRemaining, 0, maxTime);
    }

}
