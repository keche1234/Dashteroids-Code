using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [SerializeField] protected GameObject firstOption;
    protected Stack<GameObject> prevOptionStack;
    protected Stack<SubMenu> subMenuStack;
    void Awake()
    {
        EventSystem.current.SetSelectedGameObject(firstOption);
        firstOption.GetComponent<MenuOptionVisualizerUI>().OnSelect();
        prevOptionStack = new Stack<GameObject>();
        subMenuStack = new Stack<SubMenu>();
    }

    private void OnEnable()
    {
        Awake();
    }

    public void LoadGameMode(string str)
    {
        SceneManager.LoadScene(str);
    }

    public void OpenSubMenu(SubMenu menu)
    {
        // when I come back, I want to be on this selected game object
        prevOptionStack.Push(EventSystem.current.currentSelectedGameObject);
        if (subMenuStack.Count > 0)
            subMenuStack.Peek().gameObject.SetActive(false);

        menu.gameObject.SetActive(true);
        subMenuStack.Push(menu);
        Debug.Log(menu.GetFirstOption());
        EventSystem.current.SetSelectedGameObject(menu.GetFirstOption());
    }

    public void CloseSubMenu()
    {
        subMenuStack.Pop().gameObject.SetActive(false);
        if (subMenuStack.Count > 0)
            subMenuStack.Peek().gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(prevOptionStack.Pop());
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

}
