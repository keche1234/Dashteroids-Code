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
    public Material iconMaterial;
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

    protected List<GameObject> players;

    [Header("Mode")]
    [SerializeField] protected GameMode gameMode;

    [Header("Multiplayer Visuals")]
    [SerializeField] protected List<PaletteSet> playerPalettes;
    protected List<BattleScoreUI> playerScores;

    [Header("Podium")]
    [SerializeField] protected List<string> rankNames;
    [SerializeField] protected List<Color> rankColors;
    [SerializeField] protected List<float> rankHeights;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // protected GameObject setupWindow;
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
        players = new List<GameObject>();
        for (int i = 0; i < maxPlayerCount; i++)
        {
            if (joinedPlayers[i])
            {
                GameObject shipObj = playerInputManager.JoinPlayer(i, -1, null, playerSetupManager.GetPlayerDevice(i)).gameObject;
                players.Add(shipObj);
                shipObj.GetComponent<ShipState>().SetPlayerNumberFromIndex(i);

                switch (joinedPlayerCount)
                {
                    case 2:
                        if (i == 0)
                        {
                            shipObj.transform.position = new Vector2(-4, 0);
                            shipObj.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right);

                            shipObj.GetComponent<ShipState>().SetRespawnPosition(new Vector2(-4, 0));
                            shipObj.GetComponent<ShipState>().SetRespawnLookVector(Vector3.right);
                        }
                        else
                        {
                            shipObj.transform.position = new Vector2(4, 0);
                            shipObj.transform.rotation = Quaternion.LookRotation(Vector3.forward, -Vector3.right);

                            shipObj.GetComponent<ShipState>().SetRespawnPosition(new Vector2(4, 0));
                            shipObj.GetComponent<ShipState>().SetRespawnLookVector(-Vector3.right);
                        }
                        break;
                    case 3:
                        shipObj.transform.position = Quaternion.Euler(0, 0, 120 * i) * Vector3.up * 4;
                        shipObj.transform.rotation = Quaternion.LookRotation(Vector3.forward, Quaternion.Euler(0, 0, 120 * i) * Vector3.up * -1);

                        shipObj.GetComponent<ShipState>().SetRespawnPosition(shipObj.transform.position);
                        shipObj.GetComponent<ShipState>().SetRespawnLookVector(transform.up);
                        break;
                    case 4:
                        shipObj.transform.position = new Vector2(i % 2 == 0 ? -2 : 2, i < 2 ? 2 : -2);
                        if (i < 2) shipObj.transform.rotation = Quaternion.LookRotation(Vector3.forward, Quaternion.Euler(0, 0, -135 - (90 * i)) * Vector3.up);
                        else shipObj.transform.rotation = Quaternion.LookRotation(Vector3.forward, Quaternion.Euler(0, 0, -45 + (90 * (i - 2))) * Vector3.up);

                        shipObj.GetComponent<ShipState>().SetRespawnPosition(shipObj.transform.position);
                        shipObj.GetComponent<ShipState>().SetRespawnLookVector(transform.up);
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

    public bool SetDeviceForController(int index, InputDevice device)
    {
        if (index < 0 || index >= shipControllers.Length)
            return false;

        shipControllers[index].GetComponent<ShipController>().SetDevice(device);
        return true;
    }

    public int GetNextPlayer()
    {
        return nextPlayer;
    }

    public void SetPlayerMaps(string map)
    {
        for (int i = 0; i < players.Count; i++)
            players[i].GetComponent<PlayerInput>().SwitchCurrentActionMap(map);
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

    public void AddScoresToLeaderboard()
    {
        GameObject ranks = GameObject.Find("Ranks");
        GameObject leaderboard = GameObject.Find("Battle Leaderboard");

        // Sort players by scores, add their battle UIs
        for (int i = 0; i < players.Count; i++)
        {
            int maxIndex = i;
            for (int j = i + 1; j < players.Count; j++)
            {
                if (players[j].GetComponent<ShipState>().GetArenaBattleScore() > players[maxIndex].GetComponent<ShipState>().GetArenaBattleScore())
                    maxIndex = j;
            }
            // move max to front
            GameObject temp = players[i];
            players[i] = players[maxIndex];
            players[maxIndex] = temp;
            players[i].GetComponent<ShipState>().GetBattleScoreUI().transform.SetParent(leaderboard.transform);
            players[i].GetComponent<ShipState>().GetBattleScoreUI().transform.localScale = Vector3.one * 2;

            GameObject rank = ranks.transform.GetChild(i).gameObject;
            rank.SetActive(true);

            TextMeshProUGUI rankText;
            if (rank && (rankText = rank.GetComponent<TextMeshProUGUI>()))
            {
                if (i > 0 && players[i].GetComponent<ShipState>().GetArenaBattleScore() == players[i - 1].GetComponent<ShipState>().GetArenaBattleScore())
                {
                    TextMeshProUGUI prevRankText = ranks.transform.GetChild(i-1).gameObject.GetComponent<TextMeshProUGUI>();
                    rankText.text = prevRankText.text;
                    rankText.color = prevRankText.color;
                    rankText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, prevRankText.rectTransform.rect.width);
                    rankText.rectTransform.sizeDelta = prevRankText.rectTransform.sizeDelta;
                }
                else
                {
                    rankText.text = rankNames[i];
                    rankText.color = rankColors[i];
                    rankText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rankHeights[i]);
                }
            }
        }
    }

    public List<GameObject> GetPlayers()
    {
        return players;
    }

    public enum GameMode
    {
        None = 0b_00,
        ScoreAttack = 0b_01,
        ArenaBattle = 0b_10
    }
}
