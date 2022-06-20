using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdjustHUDataValue : MonoBehaviour
{
    private Slider slider;
    public GameObject slider_text;
    //public string value_display;
    public int point_index;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        point_index = slider_text.GetComponent<EditHUPoint>().point_index;
        //value_display = slider_text.GetComponent<TMP_InputField>().text;
    }

    public void AdjustColorPointDataValue(Slider slider)
    {

        if (TF_Utilities.Instance.allow_changing_values)
        {
            if (TF_Utilities.Instance.GetCurrentColorPointDataIndex() != point_index)
            {
                TF_Utilities.Instance.SetCurrentColorPointDataIndex(point_index);
            }

            TF_Utilities.Instance.UpdateColorDataValue(slider.value);
            //value_display = slider.value.ToString();
            slider_text.GetComponent<TMP_InputField>().text = slider.value.ToString();

        }

    }

    public void AdjustAlphaPointDataValue(Slider slider)
    {
        if (TF_Utilities.Instance.allow_changing_values)
        {
            if (TF_Utilities.Instance.GetCurrentAlphaPointDataIndex() != point_index)
            {
                TF_Utilities.Instance.SetCurrentAlphaPointDataIndex(point_index);
            }

            TF_Utilities.Instance.UpdateAlphaDataValue(slider.value);
            //value_display = slider.value.ToString();
            slider_text.GetComponent<TMP_InputField>().text = slider.value.ToString();

        }
    }
}
