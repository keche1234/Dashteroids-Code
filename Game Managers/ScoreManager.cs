using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Multiplier")]
    [SerializeField] protected float multiplierGrowth;
    [SerializeField] protected float maxMultiplier;
    protected float scoreMultiplier = 1.0f;

    [Header("UI")]
    [Tooltip("Points per second")]
    [SerializeField] protected float rollRateUI;
    [SerializeField] protected TextMeshProUGUI scoreText;
    [SerializeField] protected TextMeshProUGUI multiplierText;

    protected float trueScore = 0.0f;
    protected float rollingScore = 0.0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
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
        trueScore += score * scoreMultiplier;
        if (!rolling)
            rollingScore += score * scoreMultiplier;
    }

    public void IncrementMultiplier()
    {
        scoreMultiplier = Mathf.Clamp(scoreMultiplier + multiplierGrowth, 1.0f, maxMultiplier);
        multiplierText.text = "x" + scoreMultiplier.ToString("0.0");
    }

    public void ResetMultiplier()
    {
        scoreMultiplier = 1;
        multiplierText.text = "x1.0";
    }
}
