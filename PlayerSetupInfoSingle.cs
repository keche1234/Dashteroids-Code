using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerSetupInfoSingle : MonoBehaviour
{
    // TODO: Get player index and device
    private PlayerInput playerInput;
    private int playerIndex;
    private InputDevice playerDevice;
    private PlayerManager playerManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Debug.Log("Awake!");
        playerInput = GetComponent<PlayerInput>();
        playerIndex = playerInput.playerIndex;
        playerDevice = playerInput.devices[0];
        PlayerManager[] pm = FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);
        if (pm != null && pm.Length > 0)
        {
            playerManager = pm[0];
            playerManager.AddPlayer(playerIndex, playerDevice);
            // TODO: Set the device of player index
            playerManager.SetDeviceForController(playerIndex, playerDevice);
            DontDestroyOnLoad(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DestroySelf()
    {
        playerManager.RemovePlayer(playerIndex);
        Destroy(gameObject);
    }

    public void StartGame()
    {
        switch (playerManager.GetGameMode())
        {
            case PlayerManager.GameMode.TimeAttack:
                SceneManager.LoadScene("TimeAttack");
                break;
            case PlayerManager.GameMode.BattleMode:
                SceneManager.LoadScene("BattleMode");
                playerManager.StartGame();
                break;
            default:
                break;
        }
    }

    public int GetPlayerIndex()
    {
        return playerIndex;
    }

    public InputDevice GetPlayerDevice()
    {
        return playerDevice;
    }

}