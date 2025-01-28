using UnityEngine;
using UnityEngine.UI;

public class DashMeterUI : MonoBehaviour
{
    [SerializeField] protected Slider slider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSliderPercentage(float p)
    {
        slider.value = p;
    }
}
