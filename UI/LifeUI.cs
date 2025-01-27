using UnityEngine;
using TMPro;

public class LifeUI : MonoBehaviour
{
    protected TextMeshProUGUI lifeText;

    private void Awake()
    {
        lifeText = GetComponent<TextMeshProUGUI>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool SetLifeCount(int txt)
    {
        if (lifeText)
            lifeText.text = "x" + txt;
        return lifeText;
    }
}
