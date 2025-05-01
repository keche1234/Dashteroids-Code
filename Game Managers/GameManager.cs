using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.InputSystem.UI;

public class GameManager : MonoBehaviour
{
    protected Coroutine startGameCoroutine;

    // Other Management
    protected TimeManager timeManager;
    protected PlayerManager playerManager;
    protected BountyManager bountyManager;
    protected TutorialManager tutorialManager;

    protected PlayerInput soloPlayer;

    [Header("General Game Information")]
    [SerializeField] protected GameMode gameMode;
    [SerializeField] protected GameState gameState;

    [Header("Visual Elements - General")]
    [SerializeField] protected Image screenWhite;
    [SerializeField] protected float transitionFadeTime;

    [Header("Visual Elements - Pause")]
    [SerializeField] protected GameObject pauseMenu;
    [SerializeField] protected GameObject continueButton;

    [Header("Visual Elements - Countdown")]
    [SerializeField] protected TextMeshProUGUI countdownNumber;

    [Header("Audio Elements")]
    [SerializeField] protected AudioSource musicSource;
    [SerializeField] protected MusicLoop musicLooper;

    protected float actionBuffer = 0f;

    void Awake()
    {
        Application.targetFrameRate = 120;

        timeManager = GetComponent<TimeManager>();
        playerManager = GameObject.Find("PlayerManager")?.GetComponent<PlayerManager>();
        bountyManager = GetComponent<BountyManager>();
        tutorialManager = GetComponent<TutorialManager>();

        if (gameMode == GameMode.ScoreAttack)
        {
            soloPlayer = GameObject.Find("ShipSolo").GetComponent<PlayerInput>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        actionBuffer = Mathf.Clamp(actionBuffer -= Time.unscaledTime, 0, actionBuffer);
        switch (gameState)
        {
            case GameState.Tutorial:
                Time.timeScale = 0;
                break;
            case GameState.Countdown:
                Time.timeScale = 0;
                break;
            case GameState.Paused:
                Time.timeScale = 0;
                break;
            default:
                Time.timeScale = 1;
                break;
        }
    }

    /********************
     * Starting the game
     ********************/
    public void StartGame()
    {
        pauseMenu.SetActive(false);
        startGameCoroutine = StartCoroutine(StartGameCoroutine());
    }

    protected IEnumerator StartGameCoroutine()
    {
        // Turn off screen fade
        for (float t = 0; t < transitionFadeTime; t += Time.unscaledDeltaTime)
        {
            screenWhite.color = new Color(1, 1, 1, 1 - (t / transitionFadeTime));
            yield return null;
        }
        // Set state to countdown
        gameState = GameState.Countdown;
        yield return null;

        // Do the countdown
        // TOJUICE: numbers shrink from 1.5 scale to 1, fade numbers as time goes down
        countdownNumber.gameObject.SetActive(true);
        for (float t = 3; t >= 0; t -= Time.unscaledDeltaTime)
        {
            countdownNumber.text = Mathf.Ceil(t).ToString("0");
            yield return null;
        }

        // Set state to playing, show "DASH!" for 1 second
        gameState = GameState.Playing;

        soloPlayer?.SwitchCurrentActionMap("Player");
        playerManager?.SetPlayerMaps("Player");

        // Show "DASH!" for 1 second
        if (!musicLooper.enabled)
            musicLooper.enabled = true;
        for (float t = 0; t < 1; t += Time.unscaledDeltaTime)
        {
            countdownNumber.text = "DASH!!";
            yield return null;
        }

        // Enable TimeManager
        timeManager.enabled = true;
        countdownNumber.gameObject.SetActive(false);
        yield return null;
    }

    public void EndGame()
    {
        playerManager?.AddScoresToLeaderboard();
        gameState = GameState.Results;
    }

    public void ShowTutorial()
    {
        gameState = GameState.Tutorial;
        tutorialManager.ShowTutorial();
    }

    public void PauseGame()
    {
        if (gameState != GameState.Paused) // Pause the game
        {
            gameState = GameState.Paused;
            pauseMenu.SetActive(true);
            EventSystem.current.SetSelectedGameObject(continueButton);
            continueButton.GetComponent<MenuOptionVisualizerUI>().OnSelect();

            soloPlayer?.SwitchCurrentActionMap("UI");
            playerManager?.SetPlayerMaps("UI");
        }
        else // Unpause the game
        {
            Debug.Log("Unpausing");
            EventSystem.current.SetSelectedGameObject(null);
            StartGame();
            soloPlayer?.SwitchCurrentActionMap("Player");
            playerManager?.SetPlayerMaps("Player");
        }
    }

    public void StartCountdown()
    {
        gameState = GameState.Countdown;
    }

    public void StartPlaying()
    {
        gameState = GameState.Playing;
    }

    /*****************************
     * Getting the other managers
     *****************************/

    public BountyManager GetBountyManager()
    {
        return bountyManager;
    }
    public TutorialManager GetTutorialManager()
    {
        return tutorialManager;
    }

    public void PlayerUISelectAll(GameObject select)
    {
        Debug.Log("erm");
        List<GameObject> players = playerManager.GetPlayers();
        foreach (GameObject player in players)
        {
            player.GetComponent<MultiplayerEventSystem>()?.SetSelectedGameObject(null);
            player.GetComponent<MultiplayerEventSystem>()?.SetSelectedGameObject(select);
        }
    }

    /************************
     * Game Modes and States
     ************************/
    public void SetGameMode(GameMode mode)
    {
        gameMode = mode;
    }

    public GameMode GetGameMode()
    {
        return gameMode;
    }

    public GameState GetGameState()
    {
        return gameState;
    }

    public void AddActionBuffer(float f)
    {
        actionBuffer += f;
    }

    public void LoadScene(string str)
    {
        if (actionBuffer <= 0)
        {
            SceneManager.LoadScene(str);
            if (str == "MainMenu")
                Destroy(GameObject.Find("PlayerSetupManager"));
            actionBuffer += 1f / 60f;
        }
    }

    public void QuitGameMode()
    {
        if (actionBuffer <= 0)
        {
            LoadScene("MainMenu");
            actionBuffer += 1f / 60f;
        }
    }

    public enum GameMode
    {
        None = 0b_00,
        ScoreAttack = 0b_01,
        ArenaBattle = 0b_10
    }

    public enum GameState
    {
        Tutorial,
        Countdown,
        Playing,
        Paused,
        Results
    }
}
