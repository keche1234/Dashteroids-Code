using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] protected GameMode gameMode;
    [SerializeField] protected BountyManager bountyManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Application.targetFrameRate = 120;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGameMode(GameMode mode)
    {
        gameMode = mode;
    }

    public GameMode GetGetMode()
    {
        return gameMode;
    }

    public BountyManager GetBountyManager()
    {
        return bountyManager;
    }

    public void UpdateBounty()
    {
        
    }

    public enum GameMode
    {
        None = 0b_00,
        ScoreAttack = 0b_01,
        ArenaBattle = 0b_10
    }
}
