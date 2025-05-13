using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class MenuOptionVisualizerUI : MonoBehaviour
{
    [SerializeField] protected Color regularColor;
    [SerializeField] protected Color selectColor;
    [SerializeField] protected Color submitColor;
    [SerializeField] protected TextMeshProUGUI text;
    protected bool submitted = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable()
    {
        //text = GetComponent<TextMeshProUGUI>();
        submitted = false;
    }

    public void SetSelectColor(Color sc)
    {
        selectColor = sc;
    }

    public void OnSelect()
    {
        if (!submitted)
            text.color = selectColor;
    }

    public void OnDeselect()
    {
        if (!submitted)
        {
            text.color = regularColor;
        }
        submitted = false;
    }

    public void OnSubmit()
    {
        text.color = submitColor;
        submitted = true;
    }
}
