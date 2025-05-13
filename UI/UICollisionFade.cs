using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class UICollisionFade : MonoBehaviour
{
    [SerializeField] protected List<Image> imagesToFade;
    [SerializeField] protected List<TextMeshProUGUI> textToFade;
    [SerializeField] protected float minAlpha;
    [SerializeField] protected float maxAlpha = 1;
    [SerializeField] protected float fadeTime;
    [Tooltip("Objects with this tag trigger the fade.")]
    [SerializeField] protected List<string> collisionTags;

    protected BoxCollider2D myCollider;
    protected List<GameObject> collided;
    protected Coroutine currentCoroutine;

    void Awake()
    {
        //myCollider = new GameObject(name + " UI Collider").AddComponent<BoxCollider2D>();
        myCollider = GetComponent<BoxCollider2D>();
        myCollider.isTrigger = true;
        myCollider.gameObject.transform.SetParent(transform);
        collided = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        // Set collider to UI world space
        Vector3[] fourCorners = new Vector3[4];
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.GetWorldCorners(fourCorners);

        for (int i = 0; i < 4; i++)
            fourCorners[i] = Camera.main.ScreenToWorldPoint(fourCorners[i]);

        // Calculate location and dimensions
        Vector3 midPoint = (fourCorners[0] + fourCorners[1] + fourCorners[2] + fourCorners[3]) / 4f;
        float width = Mathf.Max((fourCorners[3].x - fourCorners[0].x), (fourCorners[2].x - fourCorners[1].x));
        float height = Mathf.Max((fourCorners[1].y - fourCorners[0].y), (fourCorners[3].y - fourCorners[2].y));

        myCollider.offset = new Vector3(midPoint.x, midPoint.y, 0) - transform.position;
        myCollider.size = new Vector2(width, height);
    }

    // Collision
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collisionTags.Contains(collision.tag))
        {
            collided.Add(collision.gameObject);
            if (collided.Count == 1)
            {
                if (currentCoroutine != null)
                    StopCoroutine(currentCoroutine);
                currentCoroutine = StartCoroutine(SetAlpha(maxAlpha, minAlpha));
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        // Rremove collider if in list, check alpha
        if (collided.Contains(collision.gameObject))
        {
            collided.Remove(collision.gameObject);
            if (collided.Count == 0)
            {
                if (currentCoroutine != null)
                    StopCoroutine(currentCoroutine);
                currentCoroutine = StartCoroutine(SetAlpha(minAlpha, maxAlpha));
            }
        }
    }

    public IEnumerator SetAlpha(float startAlpha, float endAlpha)
    {
        foreach (Image image in imagesToFade)
            image.color = new Color(image.color.r, image.color.g, image.color.b, startAlpha);
        foreach (TextMeshProUGUI text in textToFade)
            text.color = new Color(text.color.r, text.color.g, text.color.b, startAlpha);

        for (float t = 0; t < fadeTime; t += Time.unscaledDeltaTime)
        {
            foreach (Image image in imagesToFade)
                image.color = new Color(image.color.r, image.color.g, image.color.b, Mathf.Lerp(startAlpha, endAlpha, t / fadeTime));
            foreach (TextMeshProUGUI text in textToFade)
                text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Lerp(startAlpha, endAlpha, t / fadeTime));
            yield return null;
        }

        foreach (Image image in imagesToFade)
            image.color = new Color(image.color.r, image.color.g, image.color.b, endAlpha);
        foreach (TextMeshProUGUI text in textToFade)
            text.color = new Color(text.color.r, text.color.g, text.color.b, endAlpha);

        yield return null;
    }
}
