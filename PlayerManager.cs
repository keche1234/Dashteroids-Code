using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [Header("Mode")]
    [SerializeField] protected GameMode gameMode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // protected GameObject setupWindow;
    protected Dictionary<int, InputDevice> players;
    protected GameObject[] shipControllers;

    void Awake()
    {
        // TODO: Determine scene and whether I should read from a Json or not
        players = new Dictionary<int, InputDevice>();
        DontDestroyOnLoad(gameObject);
        FindShipControllers();
    }

    void FindShipControllers()
    {
        shipControllers = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < shipControllers.Length; i++)
        {
            for (int j = i + 1; j < shipControllers.Length; j++)
            {
                if (string.Compare(shipControllers[i].name, shipControllers[j].name) > 0)
                {
                    GameObject temp = shipControllers[i];
                    shipControllers[i] = shipControllers[j];
                    shipControllers[j] = temp;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Debug
        StartGame();
    }

    public void AddPlayer(int index, InputDevice device)
    {
        players.Remove(index);
        players.Add(index, device);
    }

    public void RemovePlayer(int index)
    {
        players.Remove(index);
    }

    public bool SetDeviceForController(int index, InputDevice device)
    {
        if (index < 0 || index >= shipControllers.Length)
            return false;

        shipControllers[index].GetComponent<ShipController>().SetDevice(device);
        Debug.Log("Player manager setting controller " + index + " to device " + device);
        return true;
    }


    public void StartGame()
    {
        //FindShipControllers();
        for (int i = 0; i < players.Count; i++)
        {
            SetDeviceForController(i, players[i]);
        }
    }

    public void SaveIntoJson()
    {
        string info = JsonUtility.ToJson(players);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/PlayerManagerData.json", info);
    }

    public void LoadFromJson()
    {
        string infoJson = System.IO.File.ReadAllText(Application.persistentDataPath + "/PlayerManagerData.json");
        players = JsonUtility.FromJson<Dictionary<int, InputDevice>>(infoJson);
    }

    public GameMode GetGameMode()
    {
        return gameMode;
    }

    public enum GameMode
    {
        TimeAttack = 0b_0,
        BattleMode = 0b_1
    }
}
