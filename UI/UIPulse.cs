using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIPulse : MonoBehaviour
{
    [Header("Size")]
    [SerializeField] protected Vector3 startSize;
    [SerializeField] protected Vector3 minSize;
    [SerializeField] protected Vector3 maxSize;
    [SerializeField] protected float sizeCycleTime;
    [SerializeField] protected StartDirection sizeStartDirection;
    [SerializeField] protected bool sizeUseUnscaledTime;
    protected Coroutine sizeCoroutine;

    [Header("Color")]
    [SerializeField] protected Graphic graphic; 
    [SerializeField] protected Color startColor;
    [SerializeField] protected Color minColor;
    [SerializeField] protected Color maxColor;
    [SerializeField] protected float colorCycleTime;
    [SerializeField] protected StartDirection colorStartDirection;
    [SerializeField] protected bool colorUseUnscaledTime;
    protected Coroutine colorCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnEnable()
    {
        sizeCoroutine = StartCoroutine(SizeCycle());
        colorCoroutine = StartCoroutine(ColorCycle());
    }

    public IEnumerator SizeCycle()
    {
        Vector3 first;
        Vector3 second;
        Vector3 init = startSize;

        if (sizeStartDirection == StartDirection.Forward)
        {
            first = maxSize;
            second = minSize;
        }
        else
        {
            first = minSize;
            second = maxSize;
        }

        if (sizeUseUnscaledTime)
        {
            while (true)
            {
                // Forward
                for (float t = 0; t < sizeCycleTime / 2; t += Time.unscaledDeltaTime)
                {
                    transform.localScale = Vector3.Lerp(init, first, 2 * t / sizeCycleTime);
                    yield return null;
                }
                init = first;

                // Backward
                for (float t = 0; t < sizeCycleTime / 2; t += Time.unscaledDeltaTime)
                {
                    transform.localScale = Vector3.Lerp(init, second, 2 * t / sizeCycleTime);
                    yield return null;
                }
                init = second;

                yield return null;
            }
        }
        else
        {
            while (true)
            {
                // Forward
                for (float t = 0; t < sizeCycleTime / 2; t += Time.deltaTime)
                {
                    transform.localScale = Vector3.Lerp(init, first, 2 * t / sizeCycleTime);
                    yield return null;
                }
                init = first;

                // Backward
                for (float t = 0; t < sizeCycleTime / 2; t += Time.deltaTime)
                {
                    transform.localScale = Vector3.Lerp(init, second, 2 * t / sizeCycleTime);
                    yield return null;
                }
                init = second;

                yield return null;
            }
        }
    }

    public IEnumerator ColorCycle()
    {
        Color first;
        Color second;
        Color init = startColor;

        if (sizeStartDirection == StartDirection.Forward)
        {
            first = maxColor;
            second = minColor;
        }
        else
        {
            first = minColor;
            second = maxColor;
        }

        if (sizeUseUnscaledTime)
        {
            while (true)
            {
                // Forward
                for (float t = 0; t < sizeCycleTime / 2; t += Time.unscaledDeltaTime)
                {
                    graphic.color = Color.Lerp(init, first, 2 * t / sizeCycleTime);
                    yield return null;
                }
                init = first;

                // Backward
                for (float t = 0; t < sizeCycleTime / 2; t += Time.unscaledDeltaTime)
                {
                    graphic.color = Color.Lerp(init, second, 2 * t / sizeCycleTime);
                    yield return null;
                }
                init = second;

                yield return null;
            }
        }
        else
        {
            while (true)
            {
                // Forward
                for (float t = 0; t < sizeCycleTime / 2; t += Time.deltaTime)
                {
                    graphic.color = Color.Lerp(init, first, 2 * t / sizeCycleTime);
                    yield return null;
                }
                init = first;

                // Backward
                for (float t = 0; t < sizeCycleTime / 2; t += Time.deltaTime)
                {
                    graphic.color = Color.Lerp(init, second, 2 * t / sizeCycleTime);
                    yield return null;
                }
                init = second;

                yield return null;
            }
        }
    }

    public void OnDisable()
    {
        StopAllCoroutines();
    }

    protected enum StartDirection
    {
        Forward,
        Backward
    }
}
