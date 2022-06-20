using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditPointAlpha : MonoBehaviour
{
    public int point_index;



    public void SetAlpha(Slider slider)
    {
        if (TF_Utilities.Instance.allow_changing_values)
        {
            TF_Utilities.Instance.SetAlpha(slider.value);
        }
        
    }


    public void UpdateCurrentIndex()
    {

        if (TF_Utilities.Instance.allow_changing_values)
        {
            TF_Utilities.Instance.SetCurrentPointAlphaIndex(point_index);
        }
    }
}
