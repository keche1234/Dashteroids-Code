using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

public class TutorialAsteroidChecker : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI startMessage;
    [SerializeField] protected GameObject pauseMenuToDestroy; // need to destroy upon starting the game
    [SerializeField] protected string nextScene;
    protected int tutorialAsteroidsDestroyed;
    protected Dictionary<TutorialAsteroidSpawner, bool> tutorialSpawners; // true <=> asteroid has been destroyed

    [Header("Screen Transition")]
    [SerializeField] protected Image screenWhite;
    [SerializeField] protected float transitionDelay;
    [SerializeField] protected float fadeTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    /*
     * Should only be called by Spawner!
     */
    public void AddSpawner(TutorialAsteroidSpawner spawner)
    {
        if (tutorialSpawners == null)
        {
            tutorialSpawners = new Dictionary<TutorialAsteroidSpawner, bool>();
            tutorialAsteroidsDestroyed = 0;
        }

        tutorialSpawners.Add(spawner, false);
        startMessage.gameObject.SetActive(tutorialAsteroidsDestroyed >= tutorialSpawners.Keys.Count);
    }

    /*
     * Should only be called by Spawner!
     */
    public void RemoveSpawner(TutorialAsteroidSpawner spawner)
    {
        if (tutorialSpawners.ContainsKey(spawner))
        {
            if (tutorialSpawners[spawner])
                tutorialAsteroidsDestroyed -= 1;

            tutorialSpawners.Remove(spawner);
            startMessage.gameObject.SetActive(tutorialAsteroidsDestroyed >= tutorialSpawners.Keys.Count);
        }
    }

    public bool AsteroidDestroyed(TutorialAsteroidSpawner spawner)
    {
        if (tutorialSpawners.ContainsKey(spawner))
        {
            if (!tutorialSpawners[spawner])
                tutorialAsteroidsDestroyed += 1;

            startMessage.gameObject.SetActive(tutorialAsteroidsDestroyed >= tutorialSpawners.Keys.Count);

            tutorialSpawners[spawner] = true;
            return true;
        }
        return false;
    }

    public void StartGame(CallbackContext callback)
    {
        StartGame(nextScene);
    }

    public void StartGame(string scene)
    {
        if (tutorialAsteroidsDestroyed >= tutorialSpawners.Keys.Count)
        {
            if (pauseMenuToDestroy)
                Destroy(pauseMenuToDestroy);
            StartCoroutine(StartGameCoroutine(scene));
        }
    }

    private IEnumerator StartGameCoroutine(string scene)
    {
        // Fade to white, hide tutorialScreen, start countdown
        yield return new WaitForSecondsRealtime(transitionDelay);
        for (float t = 0; t < fadeTime; t += Time.unscaledDeltaTime)
        {
            screenWhite.color = new Color(1, 1, 1, t / fadeTime);
            yield return null;
        }
        screenWhite.color = new Color(1, 1, 1, 1);
        SceneManager.LoadScene(scene);
        yield return null;
    }

    public void OnDisable()
    {
        StopAllCoroutines();
    }
}
