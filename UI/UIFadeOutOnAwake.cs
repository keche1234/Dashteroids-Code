using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Graphic))]
public class UIFadeOutOnAwake : MonoBehaviour
{
    protected Graphic graphic;
    [SerializeField] protected float delay;
    [SerializeField] protected float fadeTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        graphic = GetComponent<Graphic>();
        Debug.Log(graphic.GetType());
        StartCoroutine(FadeOut());
    }

    public IEnumerator FadeOut()
    {
        yield return new WaitForSecondsRealtime(delay);

        for (float t = 0; t < fadeTime; t += Time.unscaledDeltaTime)
        {
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 1f - (t / fadeTime));
            yield return null;
        }
        graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0f);
        yield return null;
    }

    public void OnDisable()
    {
        StopAllCoroutines();
    }
}
