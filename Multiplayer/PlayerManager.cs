using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class PaletteSet
{
    public Color shipColor;
    public Color backgroundColor;

    public Sprite iconSprite;
    public Color iconColor;

    public Color superDashStartColor;
    public Color superDashEndColor;
    public Color burstDashStartColor;
    public Color burstDashEndColor;
}

public class PlayerManager : MonoBehaviour
{
    //public static PlayerManager instance;

    protected PlayerSetupManager playerSetupManager;
    protected PlayerInputManager playerInputManager;
    protected int nextPlayer; // When a ship is created, it gets this value;

    [Header("Mode")]
    [SerializeField] protected GameMode gameMode;

    [Header("Multiplayer Visuals")]
    [SerializeField] protected List<PaletteSet> playerPalettes;
    protected List<BattleScoreUI> playerScores;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // protected GameObject setupWindow;
    // TODO: Player Device holder that is stored between scenes protected Dictionary<int, InputDevice> players;
    protected GameObject[] shipControllers;

    void Awake()
    {
        playerSetupManager = GameObject.Find("PlayerSetupManager").GetComponent<PlayerSetupManager>();
        playerInputManager = GetComponent<PlayerInputManager>();

        playerScores = new List<BattleScoreUI>();

        // Palettes
        for (int i = 0; i < playerPalettes.Count; i++)
        {
            GameObject holder = GameObject.Find("P" + (i + 1) + " Score");
            if (holder.GetComponent<BattleScoreUI>())
                playerScores.Add(holder.GetComponent<BattleScoreUI>());
            holder.SetActive(false);
        }

        // Creating players from the setup
        int maxPlayerCount = playerInputManager.maxPlayerCount;
        bool[] joinedPlayers = playerSetupManager.GetJoinedPlayers();
        int joinedPlayerCount = playerSetupManager.GetJoinedPlayerCount();
        for (int i = 0; i < maxPlayerCount; i++)
        {
            if (joinedPlayers[i])
            {
                GameObject shipObj = playerInputManager.JoinPlayer(i, -1, null, playerSetupManager.GetPlayerDevice(i)).gameObject;
                shipObj.GetComponent<ShipState>().SetPlayerNumberFromIndex(i);

                switch (joinedPlayerCount)
                {
                    case 2:
                        if (i == 0)
                        {
                            shipObj.transform.position = new Vector2(-4, 0);
                            shipObj.transform.rotation = Quaternion.LookRotation(transform.forward, transform.right);
                        }
                        else
                        {
                            shipObj.transform.position = new Vector2(4, 0);
                            shipObj.transform.rotation = Quaternion.LookRotation(transform.forward, -transform.right);
                        }
                        break;
                    case 3:
                        shipObj.transform.position = Quaternion.Euler(0, 0, 120 * i) * transform.up * 4;
                        shipObj.transform.rotation = Quaternion.LookRotation(transform.forward, Quaternion.Euler(0, 0, 120 * i) * transform.up * -1);
                        break;
                    case 4:
                        shipObj.transform.position = new Vector2(i % 2 == 0 ? -2 : 2, i < 2 ? 2 : -2);
                        if (i < 2) shipObj.transform.rotation = Quaternion.LookRotation(transform.forward, Quaternion.Euler(0, 0, -135 - (90 * i)) * transform.up);
                        else shipObj.transform.rotation = Quaternion.LookRotation(transform.forward, Quaternion.Euler(0, 0, -45 + (90 * (i - 2))) * transform.up);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Debug
        //StartGame();
    }

    public void AddPlayer(PlayerInput playerInput)
    {

    }

    public void RemovePlayer(int index)
    {
        //players.Remove(index);
    }

    public bool SetDeviceForController(int index, InputDevice device)
    {
        if (index < 0 || index >= shipControllers.Length)
            return false;

        shipControllers[index].GetComponent<ShipController>().SetDevice(device);
        Debug.Log("Player manager setting controller " + index + " to device " + device);
        return true;
    }

    public int GetNextPlayer()
    {
        return nextPlayer;
    }


    public void StartGame()
    {

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

    public BattleScoreUI GetScoreUI(int i)
    {
        if (i < 0 || i >= playerScores.Count)
        {
            Debug.LogWarning("Requested Player Score UI " + i + " is out of range [0," + "," + playerScores.Count + ")!");
            return null;
        }
        return playerScores[i];
    }

    public enum GameMode
    {
        None = 0b_00,
        ScoreAttack = 0b_01,
        ArenaBattle = 0b_10
    }
}
