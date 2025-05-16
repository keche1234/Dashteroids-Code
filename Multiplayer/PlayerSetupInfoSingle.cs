using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerSetupInfoSingle : MonoBehaviour
{
    [Header("Visual Icons")]
    [SerializeField] protected Image background;
    [SerializeField] protected Image shipImage;
    [SerializeField] protected Image shipIcon;
    [SerializeField] protected TextMeshProUGUI playerName;
    [SerializeField] protected GameObject shipVisual;

    // Get player index and device
    protected PlayerInput playerInput;
    protected int playerIndex;
    protected InputDevice playerDevice;
    //private PlayerManager playerManager;
    protected PlayerSetupManager playerSetupManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerIndex = playerInput.playerIndex;
        playerDevice = playerInput.devices[0];
        name = "Player " + (playerIndex + 1) + " Setup";
        //playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
        playerSetupManager = GameObject.Find("PlayerSetupManager").GetComponent<PlayerSetupManager>();

        transform.SetParent(GameObject.Find("ShipSetupGrid").transform);
        // sort children in parent
        int childCount = transform.parent.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject.Find("Player " + (i + 1) + " Setup").transform.SetAsLastSibling();
        }

        PaletteSet palette = playerSetupManager.GetPaletteSet(playerIndex);
        if (palette != null)
        {
            background.color = palette.backgroundColor;
            shipImage.color = palette.shipColor;

            shipIcon.sprite = palette.iconSprite;
            shipIcon.color = palette.iconColor;
            shipIcon.SetNativeSize();
            shipIcon.rectTransform.sizeDelta = new Vector2(shipIcon.rectTransform.rect.width / 5f, shipIcon.rectTransform.rect.height / 5f);

            playerName.text = "Player " + (playerIndex + 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        RotateShip();
    }

    protected void RotateShip()
    {
        Vector2 dashAim = playerInput.actions.FindAction("AimDemo").ReadValue<Vector2>();
        if (dashAim != Vector2.zero)
        {
            float angle = Vector2.Angle(transform.up, dashAim);
            if (angle == 180)
                dashAim = Quaternion.Euler(0, 0, 1) * dashAim;

            Vector3 lookDirection = Vector3.RotateTowards(transform.up, dashAim, 2 * Mathf.PI, 1f);
            shipVisual.transform.rotation = Quaternion.LookRotation(transform.forward, lookDirection);
        }
    }

    public void LeaveGame()
    {
        playerSetupManager.RemovePlayerDevice(playerInput);
        Destroy(gameObject);
    }

    public void StartGame()
    {
        switch (playerSetupManager.GetGameMode())
        {
            case PlayerSetupManager.GameMode.ScoreAttack:
                playerSetupManager.StartingGame(true);
                SceneManager.LoadScene("ScoreAttack_Tutorial");
                playerSetupManager.GetComponent<PlayerInputManager>().DisableJoining();
                break;
            case PlayerSetupManager.GameMode.ArenaBattle:
                playerSetupManager.StartingGame(true);
                SceneManager.LoadScene("ArenaBattle_Tutorial");
                playerSetupManager.GetComponent<PlayerInputManager>().DisableJoining();
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