using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditPointColor : MonoBehaviour
{
    private Image point_image;
    public int point_index;
    public Color point_color;
    public Color start_color;



    private void Awake()
    {
        point_image = GetComponent<Image>();
    }
    public void ChooseColorButtonClick()
    {
        ColorPicker.Create(point_color, "", SetColor, Cancel, true);
    }
    
    private void SetColor(Color color)
    {
        point_image.color = color;
        point_color = color;
        TF_Utilities.Instance.SetColor(color);
    }

    private void Cancel(Color previous_color)
    {
        TF_Utilities.Instance.SetColor(previous_color);
    }

    public void UpdateCurrentIndex()
    {
        if (ColorPicker.done)
        {
            TF_Utilities.Instance.SetCurrentPointColorIndex(point_index);
        }
    }

    public void UpdateOriginalColor()
    {
        //start_color = point_color;
    }
}
