using UnityEngine;

[RequireComponent(typeof(TimeManager))]
public class BountyManager : MonoBehaviour
{
    protected int bountyScore; // all players with this value will have a bounty on them
    protected bool needToComputeBounty = false;
    protected TimeManager timeManager;
    protected bool bountyStarted = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        timeManager = GetComponent<TimeManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!bountyStarted && timeManager.BountyIsActive())
        {
            bountyStarted = true;
            needToComputeBounty = true;
        }


        if (needToComputeBounty)
        {
            ComputeBounty();
            needToComputeBounty = false;
        }
    }

    private void ComputeBounty()
    {
        // Find the max score amongst all players
        ShipState[] ships = FindObjectsByType<ShipState>(FindObjectsSortMode.None);
        int maxScore = int.MinValue;
        for (int i = 0; i < ships.Length; i++)
            if (ships[i].GetArenaBattleScore() > maxScore)
                maxScore = ships[i].GetArenaBattleScore();

        bountyScore = maxScore;
        if (timeManager.BountyIsActive())
            DisplayBounty();
    }

    private void DisplayBounty()
    {
        ShipState[] ships = FindObjectsByType<ShipState>(FindObjectsSortMode.None);
        for (int i = 0; i < ships.Length; i++)
            ships[i].DisplayBountyCrown(HasBounty(ships[i]));
    }

    public bool HasBounty(ShipState ship)
    {
        return timeManager.BountyIsActive() && ship.GetArenaBattleScore() == bountyScore;
    }

    /*
     * Will update the Bounty on the next frame.
     */
    public void NeedToComputeBounty()
    {
        needToComputeBounty = true;
    }

    public void OnEnable()
    {
        ComputeBounty();
    }
}
