using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityVolumeRendering;


public class ShaderOptionsController : MonoBehaviour {

    private static ShaderOptionsController instance;
    public static ShaderOptionsController Instance => instance;

    [SerializeField] VolumeRenderedObject currentVolume = null;

    [SerializeField] CanvasGroup LightingIntensityCanvasGroup;
    [SerializeField] Slider DensitySlider, LightingIntensitySlider, OpacitySlider, VisibleRangeMin, VisibleRangeMax, ZoomSlider, RotationX, RotationY, RotationZ;
    [SerializeField] Toggle EnableLightingToggle, EnableBack2FrontRaycasting;
    [SerializeField] InputField DensityInputField, LightIntensityInputField, OpacityInputField, MaxDepthInputField, MinVisibilityInputField, MaxVisibilityInputField, RotationXInputField, RotationYInputField, RotationZInputField;
    [SerializeField] Dropdown transferFunctionTypeDropdown;


    Vector2 visibilityMinMax = Vector2.zero;
    private Transform targetVolumeTransform = null;
    private Material volumeMaterial = null;
    private bool hasInitialised = false;

    private int densityPropertyIndex, lightIntensityPropertyIndex, opacityPropertyIndex, maxDepthPropertyIndex, densityPropertyNameID, lightIntensityPropertyNameID, opacityPropertyNameID, maxDepthPropertyNameID;
    private void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
        DensitySlider.wholeNumbers = true;
    }
    private void Start() {
        OnSelectVolume();
    }
    private void SetUpSlider(Slider slider, InputField inputField, string propertyName, out int propertyIndex, out int nameID, bool onlyIntValues = false) {
        propertyIndex = volumeMaterial.shader.FindPropertyIndex(propertyName);
        int temp = volumeMaterial.shader.GetPropertyNameId(propertyIndex);
        nameID = temp;

        slider.onValueChanged.AddListener(value => {
            inputField.SetTextWithoutNotify(value.ToString());
            if (onlyIntValues) volumeMaterial.SetInt(temp, (int)value);
            else volumeMaterial.SetFloat(temp, value);
        });
        inputField.onEndEdit.AddListener(value => {
            if (onlyIntValues) {
                int newIntValue;
                float newFloatValue;
                if (int.TryParse(value, out newIntValue)) {
                    volumeMaterial.SetInt(temp, newIntValue);
                    slider.SetValueWithoutNotify(newIntValue);
                }
                else if (float.TryParse(value, out newFloatValue)) {
                    volumeMaterial.SetFloat(temp, newFloatValue);
                    slider.SetValueWithoutNotify(newFloatValue);
                }
            }
        });
        Vector2 minMax = volumeMaterial.shader.GetPropertyRangeLimits(propertyIndex);
        slider.minValue = minMax.x;
        slider.maxValue = minMax.y;

    }

    private void SetMinMaxSliders() {
        VisibleRangeMin.minValue = 0;
        VisibleRangeMin.value = 0;
        MinVisibilityInputField.text = VisibleRangeMin.value.ToString();
        VisibleRangeMin.maxValue = 0.9f;

        VisibleRangeMax.minValue = 0.1f;
        VisibleRangeMax.value = 1;
        MaxVisibilityInputField.text = VisibleRangeMax.value.ToString();
        VisibleRangeMax.maxValue = 1;

        visibilityMinMax = new Vector2(VisibleRangeMin.value, VisibleRangeMax.value);
        currentVolume.SetVisibilityWindow(visibilityMinMax);


        VisibleRangeMin.onValueChanged.AddListener(value => {
            VisibleRangeMax.minValue = value + 0.01f;
            visibilityMinMax.x = value;
            currentVolume.SetVisibilityWindow(visibilityMinMax);
            MinVisibilityInputField.SetTextWithoutNotify("Min: " + value.ToString());
        });

        VisibleRangeMax.onValueChanged.AddListener(value => {
            VisibleRangeMin.maxValue = value - 0.01f;
            visibilityMinMax.y = value;
            currentVolume.SetVisibilityWindow(visibilityMinMax);
            MaxVisibilityInputField.SetTextWithoutNotify("Max: " + value.ToString());
        });
        MinVisibilityInputField.onValueChanged.AddListener(value => {
            float newValue = 0.0f;
            if (float.TryParse(value, out newValue)) {
                VisibleRangeMax.minValue = newValue - 0.01f;
                visibilityMinMax.x = newValue;
                currentVolume.SetVisibilityWindow(visibilityMinMax);
                VisibleRangeMin.SetValueWithoutNotify(newValue);
            }
        });
        MaxVisibilityInputField.onValueChanged.AddListener(value => {
            float newValue = 0.0f;
            if (float.TryParse(value, out newValue)) {
                VisibleRangeMin.maxValue = newValue + 0.01f;
                visibilityMinMax.y = newValue;
                currentVolume.SetVisibilityWindow(visibilityMinMax);
                VisibleRangeMax.SetValueWithoutNotify(newValue);
            }
        });

    }


    private void Initialise() {
        SetUpSlider(DensitySlider, DensityInputField, "_Density", out densityPropertyIndex, out densityPropertyNameID, true);
        SetUpSlider(LightingIntensitySlider, LightIntensityInputField, "_LightIntensity", out lightIntensityPropertyIndex, out lightIntensityPropertyNameID);
        SetUpSlider(OpacitySlider, OpacityInputField, "_Opacity", out opacityPropertyIndex, out opacityPropertyNameID);
        SetMinMaxSliders();

        maxDepthPropertyIndex = volumeMaterial.shader.FindPropertyIndex("_MaxDepth");
        maxDepthPropertyNameID = volumeMaterial.shader.GetPropertyNameId(maxDepthPropertyIndex);
        MaxDepthInputField.onEndEdit.AddListener(value => {
            float newValue = 0.0f;
            if (float.TryParse(value, out newValue))
                volumeMaterial.SetFloat(maxDepthPropertyNameID, newValue);
        });


        EnableLightingToggle.onValueChanged.AddListener(value => {
            if (value != LightingIntensityCanvasGroup.interactable)
                LightingIntensityCanvasGroup.interactable = value;
            currentVolume.SetLightingEnabled(value);
        });


        EnableBack2FrontRaycasting.onValueChanged.AddListener(value => {
            currentVolume.SetDVRBackwardEnabled(value);
        });

        Vector3 currPos = targetVolumeTransform.position;
        ZoomSlider.minValue = currPos.z - 3;
        ZoomSlider.maxValue = currPos.z + 2;
        ZoomSlider.onValueChanged.AddListener(value => { currPos.z = value; targetVolumeTransform.position = currPos; });

        SetRotationControll(RotationX, RotationXInputField, Vector3.right);
        SetRotationControll(RotationY, RotationYInputField, Vector3.up);
        SetRotationControll(RotationZ, RotationZInputField, Vector3.forward);

        hasInitialised = true;
    }

    private void SetRotationControll(Slider slider, InputField inputField, Vector3 dir) {
        slider.minValue = -180;
        slider.maxValue = 180;
        slider.wholeNumbers = true;
        inputField.contentType = InputField.ContentType.DecimalNumber;

        float lastValue = 0.0f;
        slider.onValueChanged.AddListener(value => {
            float diff = lastValue - value;
            targetVolumeTransform.rotation *= Quaternion.AngleAxis(diff, dir);
            inputField.SetTextWithoutNotify(value.ToString());
            slider.SetValueWithoutNotify(value);
            lastValue = value;
        });

        inputField.onEndEdit.AddListener(value => {
            float f = 0.0f;
            if (float.TryParse(value, out f)) {
                f = (float)Math.Round(f, 1);
                inputField.text = f.ToString();
                slider.value = f;
            }

        });

    }

    private void OnSelectVolume() {
        volumeMaterial = currentVolume.GetComponentInChildren<Renderer>().material;
        targetVolumeTransform = currentVolume.transform.GetChild(0).transform;
        if (!hasInitialised) Initialise();

        DensitySlider.SetValueWithoutNotify(volumeMaterial.GetInt(densityPropertyNameID));
        LightingIntensitySlider.SetValueWithoutNotify(volumeMaterial.GetFloat(lightIntensityPropertyNameID));
        OpacitySlider.SetValueWithoutNotify(volumeMaterial.GetFloat(opacityPropertyNameID));
        EnableLightingToggle.SetIsOnWithoutNotify(currentVolume.GetLightingEnabled());
        MaxDepthInputField.SetTextWithoutNotify(volumeMaterial.GetFloat(maxDepthPropertyNameID).ToString());

    }
}
