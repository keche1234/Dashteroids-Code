using UnityEngine;
using TMPro;

public class MessageFadeUI : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI message;
    [SerializeField] protected Color baseColor; // gradually shifts alpha to 0
    [Tooltip("Duration of alpha = 1")]
    [SerializeField] protected float fullDuration;
    [Tooltip("Duration of fade")]
    [SerializeField] protected float fadeDuration;

    protected float fullTimer;
    protected float fadeTimer;
    protected bool isFading;
    void Awake()
    {
        message = GetComponent<TextMeshProUGUI>();
        if (!message)
            Debug.LogError("Missing TextMeshUGUI Pro Component!");
    }

    // Update is called once per frame
    void Update()
    {
        if (isFading)
        {
            if (fullTimer > 0)
            {
                message.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
                fullTimer -= Time.deltaTime;
            }
            else if (fadeTimer > 0)
            {
                message.color = new Color(baseColor.r, baseColor.g, baseColor.b, fadeTimer / fadeDuration);
                fadeTimer -= Time.deltaTime;
            }
            else
                gameObject.SetActive(false);
        }
        else
            gameObject.SetActive(false);
    }

    public void SetMessage(string s, bool startFade)
    {
        if (!message) message = GetComponent<TextMeshProUGUI>();

        if (!message)
        {
            message.text = s;
            isFading = startFade;

            if (isFading)
            {
                fullTimer = fullDuration;
                fadeTimer = fadeDuration;
                message.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
            }
        }
    }

    public void OnEnable()
    {
        isFading = true;

        fullTimer = fullDuration;
        fadeTimer = fadeDuration;
        message.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
    }
}
