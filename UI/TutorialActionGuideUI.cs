using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TutorialActionGuideUI : MonoBehaviour
{
    [SerializeField] protected PlayerInput playerInput;
    [SerializeField] protected List<DynamicButtonIconUI> icons;

    public void OnEnable()
    {
        if (playerInput)
            playerInput.controlsChangedEvent.AddListener(SetButtonIcons);
    }

    public void OnDisable()
    {
        if (playerInput)
            playerInput.controlsChangedEvent.RemoveListener(SetButtonIcons);
    }

    public void SetPlayer(PlayerInput input)
    {
        if (playerInput)
            playerInput.controlsChangedEvent.RemoveListener(SetButtonIcons);
        playerInput = input;
        playerInput.controlsChangedEvent.AddListener(SetButtonIcons);
        SetButtonIcons(input);
    }

    public void SetButtonIcons(PlayerInput input)
    {
        foreach (DynamicButtonIconUI button in icons)
            button.SetIcon(input);
    }
}
