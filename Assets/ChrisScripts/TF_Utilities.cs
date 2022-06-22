using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityVolumeRendering;
using TMPro;
using System;



public class TF_Utilities : MonoBehaviour {
    public static TF_Utilities Instance;


    public float HUScaleMax;
    public float HUScaleMin;

    public GameObject colorPointsContent;
    public GameObject alphaPointsContent;

    public GameObject colorPointUIField;
    public GameObject alphaPointUIField;

    public VolumeRenderedObject volumeRenderedObject;
    float new_datavalue;

    public int current_point_color_index;
    public int current_color_point_data_index;
    public int current_point_alpha_index;
    public int current_alpha_point_data_index;

    public bool allow_changing_values = false;

    private TFColourControlPoint new_color_point;
    private TFAlphaControlPoint new_alpha_point;

    private AppManager.OnSelectVolume onSelectVolumeEvent = null;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
        }
        else {
            Instance = this;
        }
        onSelectVolumeEvent = volume => volumeRenderedObject = volume;
        AppManager.Instance.AddOnSelectVolumeEventListener(onSelectVolumeEvent);
    }

    public void GenerateColorPoint() {

        GameObject new_colorpoint = Instantiate(colorPointUIField, colorPointsContent.transform, false);
        string new_HU_value = new_colorpoint.GetComponentInChildren<TMP_InputField>().text;
        TFColourControlPoint color_point = new TFColourControlPoint();
        Color new_color = new Color(0, 0, 0, 1);
        new_datavalue = float.Parse(new_HU_value);
        color_point.dataValue = new_datavalue;
        color_point.colourValue = new_color;
        new_colorpoint.GetComponentInChildren<EditHUPoint>().point_index = volumeRenderedObject.transferFunction.colourControlPoints.Count;
        new_colorpoint.GetComponentInChildren<Slider>().value = new_datavalue;
        volumeRenderedObject.transferFunction.colourControlPoints.Add(color_point);
        if (!allow_changing_values) {
            allow_changing_values = true;
        }
    }



    public void GenerateAlphaPoint() {
        GameObject new_alphapoint = Instantiate(alphaPointUIField, alphaPointsContent.transform, false);
        string new_HU_value = new_alphapoint.GetComponentInChildren<TMP_InputField>().text;
        TFAlphaControlPoint alpha_point = new TFAlphaControlPoint();

        //new_alphapoint.GetComponent<TMP_InputField>().text = "0.5";
    }

    public void ClearIndex() {
        current_point_color_index = 0;
    }

    public void EditTF() {
        if (allow_changing_values) {
            allow_changing_values = false;
        }

        int current_index = 0;


        if (volumeRenderedObject.transferFunction.colourControlPoints.Count != 0) {
            foreach (TFColourControlPoint point in volumeRenderedObject.transferFunction.colourControlPoints) {

                GameObject new_colorpoint = Instantiate(colorPointUIField, colorPointsContent.transform, false);

                new_colorpoint.GetComponentInChildren<TMP_InputField>().text = Mathf.Round(HUScaleTransform.ReverseNormalization(point.dataValue, HUScaleMin, HUScaleMax)).ToString();
                new_colorpoint.GetComponentInChildren<EditHUPoint>().point_index = current_index;
                new_colorpoint.GetComponentInChildren<Slider>().value = Mathf.Round(HUScaleTransform.ReverseNormalization(point.dataValue, HUScaleMin, HUScaleMax));
                new_colorpoint.transform.GetChild(1).GetComponent<Image>().color = point.colourValue;
                new_colorpoint.transform.GetChild(1).GetComponent<EditPointColor>().point_index = current_index;
                new_colorpoint.transform.GetChild(1).GetComponent<EditPointColor>().point_color = point.colourValue;
                current_index++;
            }
        }


        current_index = 0;

        if (volumeRenderedObject.transferFunction.alphaControlPoints.Count > 0) {
            foreach (TFAlphaControlPoint point in volumeRenderedObject.transferFunction.alphaControlPoints) {

                GameObject new_alphapoint = Instantiate(alphaPointUIField, alphaPointsContent.transform, false);

                new_alphapoint.GetComponentInChildren<TMP_InputField>().text = Mathf.Round(HUScaleTransform.ReverseNormalization(point.dataValue, HUScaleMin, HUScaleMax)).ToString();
                new_alphapoint.GetComponentInChildren<EditHUPoint>().point_index = current_index;
                new_alphapoint.transform.GetChild(0).GetComponentInChildren<Slider>().value = Mathf.Round(HUScaleTransform.ReverseNormalization(point.dataValue, HUScaleMin, HUScaleMax));
                new_alphapoint.transform.GetChild(1).GetComponent<Slider>().value = point.alphaValue;
                new_alphapoint.transform.GetChild(1).GetComponent<EditPointAlpha>().point_index = current_index;
                current_index++;
            }
        }






        allow_changing_values = true;
        //maybe allow here to datavalues to change????
    }

    public void SetCurrentPointColorIndex(int index) {
        current_point_color_index = index;
    }

    public void SetCurrentPointAlphaIndex(int index) {
        current_point_alpha_index = index;
    }

    public void SetCurrentColorPointDataIndex(int index) {
        current_color_point_data_index = index;
    }

    public void SetCurrentAlphaPointDataIndex(int index) {
        current_alpha_point_data_index = index;
    }


    public int GetCurrentPointColorIndex() {
        return current_point_color_index;
    }

    public int GetCurrentPointAlphaIndex() {
        return current_point_alpha_index;
    }

    public int GetCurrentColorPointDataIndex() {
        return current_color_point_data_index;
    }

    public int GetCurrentAlphaPointDataIndex() {
        return current_alpha_point_data_index;
    }

    public void SetColor(Color color_to_set) {

        new_color_point = volumeRenderedObject.transferFunction.colourControlPoints[GetCurrentPointColorIndex()];
        new_color_point.colourValue = color_to_set;
        volumeRenderedObject.transferFunction.colourControlPoints[GetCurrentPointColorIndex()] = new_color_point;
        volumeRenderedObject.UpdateTFTextureOnShader();

    }

    public void SetAlpha(float alpha_to_set) {
        new_alpha_point = volumeRenderedObject.transferFunction.alphaControlPoints[GetCurrentPointAlphaIndex()];
        new_alpha_point.alphaValue = alpha_to_set;
        volumeRenderedObject.transferFunction.alphaControlPoints[GetCurrentPointAlphaIndex()] = new_alpha_point;
        volumeRenderedObject.UpdateTFTextureOnShader();

    }


    public void CancelColor(Color current_color) {
        Debug.Log("Canceled Color Changing");
    }

    public Color GetCurrentPointColor() {
        return volumeRenderedObject.transferFunction.colourControlPoints[current_point_color_index].colourValue;
    }

    public Color GetPointColor(int index) {
        return volumeRenderedObject.transferFunction.colourControlPoints[index].colourValue;
    }

    public void UpdateColorDataValue(float new_datavalue) {
        //index based searching?
        new_color_point = volumeRenderedObject.transferFunction.colourControlPoints[GetCurrentColorPointDataIndex()];
        new_color_point.dataValue = HUScaleTransform.NormalizedValue(new_datavalue, HUScaleMin, HUScaleMax);
        volumeRenderedObject.transferFunction.colourControlPoints[GetCurrentColorPointDataIndex()] = new_color_point;
        volumeRenderedObject.UpdateTFTextureOnShader();
        Debug.Log("newdatavalue " + new_datavalue);
    }

    public void UpdateAlphaDataValue(float new_datavalue) {
        //index based searching?
        new_color_point = volumeRenderedObject.transferFunction.colourControlPoints[GetCurrentColorPointDataIndex()];
        new_color_point.dataValue = HUScaleTransform.NormalizedValue(new_datavalue, HUScaleMin, HUScaleMax);
        volumeRenderedObject.transferFunction.colourControlPoints[GetCurrentColorPointDataIndex()] = new_color_point;
        volumeRenderedObject.UpdateTFTextureOnShader();
        Debug.Log("newdatavalue " + new_datavalue);
    }
}
