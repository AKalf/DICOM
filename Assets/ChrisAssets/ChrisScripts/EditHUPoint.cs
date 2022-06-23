using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EditHUPoint : MonoBehaviour
{
    public string data_text;
    public float data_value;
    public int point_index;
    public Slider datavalue_slider;

    //Need to be initialized on Start() because on Awake() it gets the values from the TF_Utilities.GeneratePoint();
    private void Start()
    {
        data_text = GetComponent<Text>().text;
        datavalue_slider.GetComponent<Slider>();
        //data_value = float.Parse(data_text);

    }

    public void UpdateCurrentDataValue()
    {
        data_text = datavalue_slider.value.ToString();    
    }

}
