using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialConfirmSingle : MonoBehaviour
{
    protected bool confirmed = false;
    protected Coroutine animationCoroutine = null;
    [SerializeField] protected TutorialManager tutorialManager;

    [SerializeField] protected Image confirmLight;

    [Header("Confirm animation")]
    [SerializeField] protected float growTime;
    [SerializeField] protected Vector3 targetScale;
    [Range(1f, 2f)]
    [SerializeField] protected float overshootMultiplier;
    [SerializeField] protected float correctTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Confirm(Color lightColor)
    {
        confirmLight.gameObject.SetActive(true);
        confirmLight.GetComponent<Image>().color = lightColor;
        if (!confirmed && animationCoroutine == null)
        {
            animationCoroutine = StartCoroutine(ConfirmAnimation());
        }
        confirmed = true;
    }

    public void Deconfirm()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        animationCoroutine = null;

        confirmed = false;
        confirmLight.gameObject.SetActive(false);
        tutorialManager.ConfirmMeInList(this, false);
    }

    public bool HasBeenConfirmed()
    {
        return confirmed;
    }

    public IEnumerator ConfirmAnimation()
    {
        for (float t = 0; t < growTime; t += Time.unscaledDeltaTime)
        {
            confirmLight.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale * overshootMultiplier, t / growTime);
            yield return null;
        }

        for (float t = 0; t < correctTime; t += Time.unscaledDeltaTime)
        {
            confirmLight.transform.localScale = Vector3.Lerp(targetScale * overshootMultiplier, targetScale, t / correctTime);
            yield return null;
        }

        animationCoroutine = null;
        tutorialManager.ConfirmMeInList(this, true);
        yield return null;
    }

    public void SetTutorialManager(TutorialManager tm)
    {
        tutorialManager = tm;
    }
}
