using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Holds the PlayerSetupInfoSingles
public class PlayerSetupManager : MonoBehaviour
{
    public static PlayerSetupManager instance;
    protected PlayerInputManager playerInputManager;
    [SerializeField] protected GameMode gameMode;
    [SerializeField] protected int minimumPlayerCount;
    [SerializeField] protected Dictionary<int, InputDevice> playerDevices;
    [SerializeField] protected List<PaletteSet> playerPalettes;
    [SerializeField] protected GameObject startMessage;
    protected bool canStartGame = false;
    protected bool startingGame = false;

    [Header("Quit")]
    [SerializeField] protected float quitConfirmTime = 4f;
    protected float quitTimer;
    protected DashteroidsActions inputActions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        quitTimer = quitConfirmTime;

        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        playerInputManager = GetComponent<PlayerInputManager>();
        if (playerDevices == null)
            playerDevices = new Dictionary<int, InputDevice>();

        inputActions = new DashteroidsActions();
    }

    private void OnEnable()
    {
        inputActions.UI.Enable();
        startMessage = GameObject.Find("Game State Canvas").transform.GetChild(1).gameObject;
    }
    public void OnDisable()
    {
        inputActions.UI.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (inputActions.FindAction("Leave").IsPressed())
        {
            if (quitTimer <= 0)
            {
                SceneManager.LoadScene("MainMenu");
                Destroy(gameObject);
            }
            quitTimer -= Time.unscaledDeltaTime;
        }
        else
            quitTimer = quitConfirmTime;
    }

    public void AddPlayerDevice(PlayerInput playerInput)
    {
        playerDevices.Add(playerInput.playerIndex, playerInput.devices[0]);

        if (playerDevices.Count >= minimumPlayerCount)
        {
            canStartGame = true;
            startMessage.gameObject.SetActive(true);
        }
    }

    public void RemovePlayerDevice(PlayerInput playerInput)
    {
        if (!startingGame)
        {
            if (playerDevices.ContainsKey(playerInput.playerIndex))
            {
                playerDevices.Remove(playerInput.playerIndex);
                if (playerDevices.Count < minimumPlayerCount)
                {
                    canStartGame = false;
                    startMessage.gameObject.SetActive(false);
                }
            }
        }
    }

    public InputDevice GetPlayerDevice(int i)
    {
        if (!playerDevices.ContainsKey(i))
        {
            Debug.LogError("No device found for player " + i + "!");
            return null;
        }
        return playerDevices[i];
    }

    public int GetPlayerCount()
    {
        return playerDevices.Count;
    }

    public bool[] GetJoinedPlayers()
    {
        bool[] joined = new bool[playerInputManager.maxPlayerCount];
        for (int i = 0; i < playerInputManager.maxPlayerCount; i++)
        {
            if (playerDevices.ContainsKey(i))
                joined[i] = true;
        }
        return joined;
    }

    public int GetJoinedPlayerCount()
    {
        int joined = 0;
        for (int i = 0; i < playerInputManager.maxPlayerCount; i++)
        {
            if (playerDevices.ContainsKey(i))
                joined++;
        }
        return joined;
    }

    public void StartingGame(bool b)
    {
        startingGame = b;
        if (startingGame)
            inputActions.UI.Disable();
    }

    public PaletteSet GetPaletteSet(int i)
    {
        if (i < 0 || i >= playerPalettes.Count)
        {
            Debug.LogWarning("Requested palette set " + i + " is out of range [0," + "," + playerPalettes.Count + ")!");
            return null;
        }
        return playerPalettes[i];
    }

    

    public GameMode GetGameMode()
    {
        return gameMode;
    }

    public enum GameMode
    {
        None = 0b_00,
        ScoreAttack = 0b_01,
        ArenaBattle = 0b_10
    }
}
