using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class DynamicButtonIconUI : MonoBehaviour
{
    [Header("Image")]
    [SerializeField] protected Image image;

    [Header("Default Button")]
    [SerializeField] protected Sprite defaultSprite;
    [SerializeField] protected Vector2 defaultScale;

    [Header("Control and Buttons")]
    [SerializeField] protected List<string> controlSchemes;
    [SerializeField] protected List<Sprite> sprites;
    [Tooltip("Sets the image to the sprite[i]'s native size, then multiplies each dimension by scaling[i]")]
    [SerializeField] protected List<Vector2> scaling;
    protected Dictionary<string, (Sprite, Vector2)> controlToButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (controlSchemes.Count != sprites.Count || sprites.Count != scaling.Count || scaling.Count != controlSchemes.Count)
        {
            Debug.LogWarning("Control Schemes, Sprites, and Scaling don't have a one-to-one correspondence! (" +
                             controlSchemes.Count + " vs " + sprites.Count + " vs " + controlSchemes.Count + ")");
        }

        controlToButton = new Dictionary<string, (Sprite, Vector2)>();
        int smallestCount = Mathf.Min(Mathf.Min(controlSchemes.Count, sprites.Count), scaling.Count);
        for (int i = 0; i < smallestCount; i++)
        {
            if (controlToButton.ContainsKey(controlSchemes[i]))
                Debug.LogWarning("Control Schemes contains a duplicate! (" + controlSchemes[i] + ")");
            controlToButton.Add(controlSchemes[i], (sprites[i], scaling[i]));
        }

        SetIconDefault();
    }

    protected void SetIconDefault()
    {
        image.sprite = defaultSprite;
        image.SetNativeSize();
        image.rectTransform.sizeDelta = new Vector2(image.rectTransform.sizeDelta.x * defaultScale.x, image.rectTransform.sizeDelta.y * defaultScale.y);

        // if null, hide this
        gameObject.SetActive(defaultSprite != null);
    }

    protected void SetIcon(Sprite sprite, Vector2 scale)
    {
        image.sprite = sprite;
        image.SetNativeSize();
        image.rectTransform.sizeDelta = new Vector2(image.rectTransform.sizeDelta.x * scale.x, image.rectTransform.sizeDelta.y * scale.y);

        // if null, hide this
        gameObject.SetActive(sprite != null);
    }

    /**************************************************************
     * Sets the current image based on the provided control scheme
     **************************************************************/
    public void SetIcon(PlayerInput playerInput)
    {
        string controlScheme = playerInput.currentControlScheme;
        string deviceName = playerInput.devices[0].name;
        //// TODO: Figure out how to properly detect devices:
        //controlScheme = playerInput.devices[0].displayName;

        if (controlToButton == null || (!controlToButton.ContainsKey(controlScheme) && !controlToButton.ContainsKey(deviceName)))
            SetIconDefault();
        else // controlToButton is defined, and contains either controlScheme or deviceName
        {
            if (controlToButton.ContainsKey(deviceName))
                SetIcon(controlToButton[deviceName].Item1, controlToButton[deviceName].Item2);
            else
                SetIcon(controlToButton[controlScheme].Item1, controlToButton[controlScheme].Item2);
        }
    }
}
