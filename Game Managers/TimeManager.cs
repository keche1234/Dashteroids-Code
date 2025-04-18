using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class TimeManager : MonoBehaviour
{
    protected DashteroidsActions inputActions;
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

    [Header("Game State")]
    [SerializeField] protected GameObject gameOverMessage;
    [SerializeField] protected GameObject audioSourceObject;
    [SerializeField] protected AudioClip timeOverSound;
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
        inputActions.bindingMask = InputBinding.MaskByGroup(inputActions.GamepadScheme.bindingGroup);

        shipControllers = FindObjectsByType<ShipController>(FindObjectsSortMode.None);
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
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
            timeUp = true;
            foreach (ShipController ship in shipControllers)
                ship.enabled = false;
            gameOverMessage.SetActive(true);

            if (audioSource && !victoryClipPlayed)
            {
                audioSource.loop = false;
                audioSource.Stop();
                audioSource.volume = 1f;
                audioSource.PlayOneShot(timeOverSound);
                victoryClipPlayed = true;
            }
        }

        if (inputActions.Player.Start.WasPerformedThisFrame())
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

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

    public void AddTime(float t)
    {
        timeRemaining += t;
        timeRemaining = Mathf.Clamp(timeRemaining, 0, maxTime);
    }
}
