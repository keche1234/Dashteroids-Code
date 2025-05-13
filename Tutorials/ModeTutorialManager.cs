using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ModeTutorialManager : MonoBehaviour
{
    protected ModeTutorialConfirmSingle[] tutorialConfirmsList;
    protected Dictionary<ModeTutorialConfirmSingle, bool> tutorialConfirmsDict;

    // Submitting to show tutorial also triggers confirmation in ShipController;
    // need to wait for this to be false
    protected bool justShowedTutorial = true; 

    [Header("Visuals")]
    [SerializeField] protected GameObject tutorialScreen;
    [SerializeField] protected Image screenWhite;
    [SerializeField] protected float transitionDelay;
    [SerializeField] protected float fadeTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        tutorialConfirmsList = FindObjectsByType<ModeTutorialConfirmSingle>(FindObjectsSortMode.None);
        tutorialConfirmsDict = new Dictionary<ModeTutorialConfirmSingle, bool>();
        for (int i = 0; i < tutorialConfirmsList.Length; i++)
        {
            tutorialConfirmsList[i].SetTutorialManager(this);
            tutorialConfirmsDict.Add(tutorialConfirmsList[i], tutorialConfirmsList[i].HasBeenConfirmed());
        }
    }

    private void OnEnable()
    {
        Awake();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        justShowedTutorial = false;
    }

    public bool ConfirmMeInList(ModeTutorialConfirmSingle confirm, bool b)
    {
        if (tutorialConfirmsDict == null)
        {
            Debug.LogError("Missing dictionary of tutoritalConfirmations!");
            return false;
        }
        
        if (!tutorialConfirmsDict.ContainsKey(confirm))
        {
            Debug.LogError("This TutorialConfirmSingle object (" + confirm + ") is not part of this Tutorial Manager's Dictionary!");
            return false;
        }
        
        tutorialConfirmsDict[confirm] = b;
        CheckConfirmations();
        return true;
    }

    protected bool CheckConfirmations()
    {
        foreach (ModeTutorialConfirmSingle confirm in tutorialConfirmsDict.Keys)
        {
            if (!tutorialConfirmsDict[confirm])
                return false;
        }
        StartCoroutine(HideTutorial());
        return true;
    }

    public bool JustShowedTutorial()
    {
        return justShowedTutorial;
    }

    public void ShowTutorial()
    {
        justShowedTutorial = true;
        tutorialScreen.SetActive(true);

        for (int i = 0; i < tutorialConfirmsList.Length; i++)
            tutorialConfirmsList[i].Deconfirm();
    }

    public IEnumerator HideTutorial()
    {
        // Fade to white, hide tutorialScreen, start countdown
        yield return new WaitForSecondsRealtime(transitionDelay);
        for (float t = 0; t < fadeTime; t += Time.unscaledDeltaTime)
        {
            screenWhite.color = new Color(1, 1, 1, t / fadeTime);
            yield return null;
        }
        tutorialScreen.SetActive(false);
        GetComponent<GameManager>().StartGame();
        yield return null;
    }
}
