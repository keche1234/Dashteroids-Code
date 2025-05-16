using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlayerTutorialManager : PlayerManager
{
    //protected PlayerSetupManager playerSetupManager;
    //protected PlayerInputManager playerInputManager;

    //protected List<GameObject> players;

    //[Header("Multiplayer Visuals")]
    //[SerializeField] protected List<PaletteSet> playerPalettes;

    [Header("Asteroids")]
    [SerializeField] protected TutorialAsteroidSpawner asteroidSpawnerPrefab;
    [SerializeField] protected TutorialAsteroidChecker tutorialAsteroidChecker;

    [Header("Wall")]
    [SerializeField] protected GameObject wallPrefab;

    [Header("Action Guides")]
    [SerializeField] protected TutorialActionGuideUI actionGuidePrefab;
    [SerializeField] protected HorizontalLayoutGroup actionGuideHolder;

    public void Awake()
    {
        playerSetupManager = GameObject.Find("PlayerSetupManager").GetComponent<PlayerSetupManager>();
        playerInputManager = GetComponent<PlayerInputManager>();

        // Creating players from the setup
        int maxPlayerCount = playerInputManager.maxPlayerCount;
        bool[] joinedPlayers = playerSetupManager.GetJoinedPlayers();
        int joinedPlayerCount = playerSetupManager.GetJoinedPlayerCount();
        players = new List<GameObject>();

        float screenWidthWorld = Camera.main.orthographicSize * Camera.main.aspect * 2;
        float screenHeightWorld = Camera.main.orthographicSize * 2;
        //Vector3 worldVector = Camera.main.ScreenToWorldPoint(new Vector3(screenWidth, screenHeight, 0)) -
        //                      Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        //float worldWidth = worldVector.x;  // Used for spacing out asteroids
        //float worldHeight = worldVector.y; // and player walls
        float playerSegmentWidth = screenWidthWorld / joinedPlayerCount;

        int j = 0;
        for (int i = 0; i < maxPlayerCount; i++)
        {
            if (joinedPlayers[i])
            {
                GameObject shipObj = playerInputManager.JoinPlayer(i, -1, null, playerSetupManager.GetPlayerDevice(i)).gameObject;
                ShipState shipState = shipObj.GetComponent<ShipState>();
                PlayerInput playerInput = shipObj.GetComponent<PlayerInput>();
                players.Add(shipObj);
                shipState.SetPlayerNumberFromIndex(i);

                // 1) Set Player Position (evenly spread out horizontaly)
                Vector2 position = new((-screenWidthWorld / 2) +  (screenWidthWorld * ((2 * j) + 1) / (2 * joinedPlayerCount)), 0);
                shipObj.transform.SetPositionAndRotation(position, Quaternion.LookRotation(Vector3.forward, Vector3.up));
                shipState.SetRespawnPosition(shipObj.transform.position);
                shipState.SetRespawnLookVector(Vector3.up);

                // 2) Create TutorialAsteroidSpawner and set asteroid positions
                TutorialAsteroidSpawner spawner = Instantiate(asteroidSpawnerPrefab);
                spawner.SetShip(shipObj);
                spawner.AddSpawnPosition(shipObj.transform.position + (Vector3.up * 2));
                spawner.SetChecker(tutorialAsteroidChecker);
                spawner.SpawnAsteroidAtPositionIndex(0);
                spawner.ResumeSpawning();

                // 3) Add asteroid checker start game to event
                UnityAction<InputAction.CallbackContext> callback = new(tutorialAsteroidChecker.StartGame);
                playerInput.actionEvents[2].AddListener(tutorialAsteroidChecker.StartGame);

                for (int h = 0; h < 4; h++)
                    for (int w = -1; w < 2; w++)
                        spawner.AddSpawnPosition(shipObj.transform.position + (h * Vector3.up) +
                                                 (w * (playerSegmentWidth / 3) * Vector3.right));

                // 4) Create Action Guides
                TutorialActionGuideUI actionGuide = Instantiate(actionGuidePrefab);
                actionGuide.SetPlayer(shipObj.GetComponent<PlayerInput>());
                actionGuide.transform.SetParent(actionGuideHolder.gameObject.transform);
                j++;
            }
        }

        // 5) Create walls to divide players
        for (int i = 0; i < joinedPlayerCount - 1; i++)
        {
            Vector2 position = new((-screenWidthWorld / 2) + (screenWidthWorld * (i + 1) / joinedPlayerCount), 0);
            GameObject wall = Instantiate(wallPrefab, position, Quaternion.Euler(0, 0, 0));
            wall.transform.localScale = new Vector3(0.5f, screenHeightWorld * 2, 1);
        }
        SetPlayerMaps("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
