using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Multiplier")]
    [SerializeField] protected float multiplierGrowth;
    [SerializeField] protected float maxMultiplier;
    protected float scoreMultiplier = 0f;

    [Header("UI")]
    [Tooltip("Points per second")]
    [SerializeField] protected float rollRateUI;
    [SerializeField] protected TextMeshProUGUI scoreText;
    [SerializeField] protected TextMeshProUGUI multiplierText;

    protected float trueScore = 0.0f;
    protected float rollingScore = 0.0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        if (rollingScore < trueScore)
        {
            rollingScore += rollRateUI * Time.deltaTime;
            rollingScore = Mathf.Clamp(rollingScore, 0.0f, trueScore);
        }
        scoreText.text = rollingScore.ToString("00000000");
    }

    public void AddScore(float score, bool rolling)
    {
        trueScore += score;
        if (!rolling)
            rollingScore += score;
    }

    public void IncrementMultiplier()
    {
        scoreMultiplier = Mathf.Clamp(scoreMultiplier + multiplierGrowth, 1.0f, maxMultiplier);
        multiplierText.text = "x" + scoreMultiplier.ToString("0");
    }

    public float GetScoreMultiplier()
    {
        return scoreMultiplier;
    }

    public void ResetMultiplier()
    {
        scoreMultiplier = 0f;
        multiplierText.text = "x1";
    }
}
