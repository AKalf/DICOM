﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityVolumeRendering;


public class ShaderUIOptionsController : MonoBehaviour {

    private static ShaderUIOptionsController instance;
    public static ShaderUIOptionsController Instance => instance;


    [SerializeField] CanvasGroup LightingIntensityCanvasGroup;
    [SerializeField] Slider DensitySlider, LightingIntensitySlider, OpacitySlider, VisibleRangeMin, VisibleRangeMax, ZoomSlider, RotationX, RotationY, RotationZ;
    [SerializeField] Toggle EnableLightingToggle, EnableBack2FrontRaycasting, EnableOpacityBasedOnDepth;
    [SerializeField]
    InputField DensityInputField, LightIntensityInputField, OpacityInputField,
        MinDepthInputField, MaxDepthInputField,
        MinVisibilityInputField, MaxVisibilityInputField,
        RotationXInputField, RotationYInputField, RotationZInputField;
    [SerializeField] Button importRawButton = null, importPARCHG = null, importDICOM = null, resetRotationButton;
    [SerializeField] Dropdown transferFunctionTypeDropdown, renderModeDropdown;


    Vector2 visibilityMinMax = Vector2.zero;

    private bool hasInitialised = false;

    private int
        // Property indexes
        densityPropertyIndex, lightIntensityPropertyIndex, opacityPropertyIndex, opacityBasedOnDepthPropertyIndex, minDepthPropertyIndex, maxDepthPropertyIndex,
        // Name IDs
        densityPropertyNameID, lightIntensityPropertyNameID, opacityPropertyNameID, opacityBasedOnDepthPropertyNameID, maxDepthPropertyNameID, minDepthPropertyNameID;

    private void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
        DensitySlider.wholeNumbers = true;
        importRawButton.onClick.AddListener(() => RuntimeFileBrowser.ShowOpenFileDialog(AppManager.Instance.OnOpenRAWDatasetResult, "DataFiles"));
        importPARCHG.onClick.AddListener(() => RuntimeFileBrowser.ShowOpenFileDialog(AppManager.Instance.OnOpenPARDatasetResult, "DataFiles"));
        importDICOM.onClick.AddListener(() => RuntimeFileBrowser.ShowOpenDirectoryDialog(AppManager.Instance.OnOpenDICOMDatasetResult));
        renderModeDropdown.onValueChanged.AddListener(index => {
            if (AppManager.Instance.SelectedVolume == null)
                return;
            UnityVolumeRendering.RenderMode oldRenderMode = AppManager.Instance.SelectedVolume.GetRenderMode();
            UnityVolumeRendering.RenderMode newRenderMode = (UnityVolumeRendering.RenderMode)index;
            if (newRenderMode != oldRenderMode)
                AppManager.Instance.SelectedVolume.SetRenderMode(newRenderMode);
        });
    }

    private void SetUpSlider(Slider slider, InputField inputField, string propertyName, out int propertyIndex, out int nameID, bool onlyIntValues = false) {
        propertyIndex = AppManager.Instance.SelectedVolumeMaterial.shader.FindPropertyIndex(propertyName);
        int temp = AppManager.Instance.SelectedVolumeMaterial.shader.GetPropertyNameId(propertyIndex);
        nameID = temp;

        slider.onValueChanged.AddListener(value => {
            inputField.SetTextWithoutNotify(value.ToString());
            if (onlyIntValues) AppManager.Instance.SelectedVolumeMaterial.SetInt(temp, (int)value);
            else AppManager.Instance.SelectedVolumeMaterial.SetFloat(temp, value);
        });
        inputField.onEndEdit.AddListener(value => {
            if (onlyIntValues) {
                int newIntValue;
                float newFloatValue;
                if (int.TryParse(value, out newIntValue)) {
                    AppManager.Instance.SelectedVolumeMaterial.SetInt(temp, newIntValue);
                    if (newIntValue > slider.maxValue) slider.SetValueWithoutNotify(slider.maxValue);
                    else if (newIntValue < slider.minValue) slider.SetValueWithoutNotify(slider.minValue);
                    else slider.SetValueWithoutNotify(newIntValue);
                }
                else if (float.TryParse(value, out newFloatValue)) {
                    AppManager.Instance.SelectedVolumeMaterial.SetFloat(temp, newFloatValue);
                    if (newFloatValue > slider.maxValue) slider.SetValueWithoutNotify(slider.maxValue);
                    else if (newFloatValue < slider.minValue) slider.SetValueWithoutNotify(slider.minValue);
                    else slider.SetValueWithoutNotify(newFloatValue);
                }
            }
        });
        Vector2 minMax = AppManager.Instance.SelectedVolumeMaterial.shader.GetPropertyRangeLimits(propertyIndex);
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
        AppManager.Instance.SelectedVolume.SetVisibilityWindow(visibilityMinMax);


        VisibleRangeMin.onValueChanged.AddListener(value => {
            if (value >= visibilityMinMax.y) {
                value = visibilityMinMax.y - 0.01f;
                VisibleRangeMin.SetValueWithoutNotify(value);
            }
            visibilityMinMax.x = value;
            AppManager.Instance.SelectedVolume.SetVisibilityWindow(visibilityMinMax);
            MinVisibilityInputField.SetTextWithoutNotify("Min: " + Math.Round(value, 2).ToString());
        });

        VisibleRangeMax.onValueChanged.AddListener(value => {
            if (value <= visibilityMinMax.x) {
                value = visibilityMinMax.x + 0.01f;
                VisibleRangeMax.SetValueWithoutNotify(value);
            }
            visibilityMinMax.y = value;
            AppManager.Instance.SelectedVolume.SetVisibilityWindow(visibilityMinMax);
            MaxVisibilityInputField.SetTextWithoutNotify("Max: " + Math.Round(value, 2).ToString());
        });
        MinVisibilityInputField.onValueChanged.AddListener(value => {
            float newValue = 0.0f;
            if (float.TryParse(value, out newValue)) {
                if (newValue >= visibilityMinMax.y) {
                    newValue = visibilityMinMax.y - 0.01f;
                    VisibleRangeMin.SetValueWithoutNotify(newValue);
                }
                visibilityMinMax.x = newValue;
                AppManager.Instance.SelectedVolume.SetVisibilityWindow(visibilityMinMax);
                VisibleRangeMin.SetValueWithoutNotify(newValue);
            }
        });
        MaxVisibilityInputField.onValueChanged.AddListener(value => {
            float newValue = 0.0f;
            if (float.TryParse(value, out newValue)) {
                if (newValue <= visibilityMinMax.x) {
                    newValue = visibilityMinMax.x + 0.01f;
                    VisibleRangeMax.SetValueWithoutNotify(newValue);
                }
                visibilityMinMax.y = newValue;
                AppManager.Instance.SelectedVolume.SetVisibilityWindow(visibilityMinMax);
                VisibleRangeMax.SetValueWithoutNotify(newValue);
            }
        });

    }
    private void Initialise() {
        SetUpSlider(DensitySlider, DensityInputField, "_Density", out densityPropertyIndex, out densityPropertyNameID, true);
        SetUpSlider(LightingIntensitySlider, LightIntensityInputField, "_LightIntensity", out lightIntensityPropertyIndex, out lightIntensityPropertyNameID);
        SetUpSlider(OpacitySlider, OpacityInputField, "_Opacity", out opacityPropertyIndex, out opacityPropertyNameID);
        opacityBasedOnDepthPropertyIndex = AppManager.Instance.SelectedVolumeMaterial.shader.FindPropertyIndex("_DecreaseOpacityBasedOnDepth");
        opacityBasedOnDepthPropertyNameID = AppManager.Instance.SelectedVolumeMaterial.shader.GetPropertyNameId(opacityBasedOnDepthPropertyIndex);
        EnableOpacityBasedOnDepth.onValueChanged.AddListener(value => {
            if (value) AppManager.Instance.SelectedVolumeMaterial.SetInt(opacityBasedOnDepthPropertyNameID, 1);
            else AppManager.Instance.SelectedVolumeMaterial.SetInt(opacityBasedOnDepthPropertyNameID, 0);
        });


        SetMinMaxSliders();

        minDepthPropertyIndex = AppManager.Instance.SelectedVolumeMaterial.shader.FindPropertyIndex("_MinDepth");
        minDepthPropertyNameID = AppManager.Instance.SelectedVolumeMaterial.shader.GetPropertyNameId(minDepthPropertyIndex);
        MinDepthInputField.onEndEdit.AddListener(value => {
            float newValue = 0.0f;
            if (float.TryParse(value, out newValue)) AppManager.Instance.SelectedVolumeMaterial.SetFloat(minDepthPropertyNameID, newValue);
        });

        maxDepthPropertyIndex = AppManager.Instance.SelectedVolumeMaterial.shader.FindPropertyIndex("_MaxDepth");
        maxDepthPropertyNameID = AppManager.Instance.SelectedVolumeMaterial.shader.GetPropertyNameId(maxDepthPropertyIndex);
        MaxDepthInputField.onEndEdit.AddListener(value => {
            float newValue = 0.0f;
            if (float.TryParse(value, out newValue)) AppManager.Instance.SelectedVolumeMaterial.SetFloat(maxDepthPropertyNameID, newValue);
        });



        EnableLightingToggle.onValueChanged.AddListener(value => {
            if (value != LightingIntensityCanvasGroup.interactable)
                LightingIntensityCanvasGroup.interactable = value;
            AppManager.Instance.SelectedVolume.SetLightingEnabled(value);
        });


        EnableBack2FrontRaycasting.onValueChanged.AddListener(value => AppManager.Instance.SelectedVolume.SetDVRBackwardEnabled(value));

        Vector3 currPos = AppManager.Instance.SelectedVolumeTransform.position;
        ZoomSlider.minValue = currPos.z - 1;
        ZoomSlider.maxValue = currPos.z + 5;
        ZoomSlider.onValueChanged.AddListener(value => { currPos.z = value; AppManager.Instance.SelectedVolumeTransform.position = currPos; });

        UIUtilities.SetRotationSliderControl(RotationX, RotationXInputField, Vector3.right, qua => AppManager.Instance.SelectedVolumeTransform.rotation *= qua, false);
        UIUtilities.SetRotationSliderControl(RotationY, RotationYInputField, Vector3.up, qua => AppManager.Instance.SelectedVolumeTransform.rotation *= qua, false);
        UIUtilities.SetRotationSliderControl(RotationZ, RotationZInputField, Vector3.forward, qua => AppManager.Instance.SelectedVolumeTransform.rotation *= qua, false);

        resetRotationButton.onClick.AddListener(() => {
            if (AppManager.Instance.SelectedVolume) {
                AppManager.Instance.SelectedVolumeTransform.rotation = Quaternion.identity * Quaternion.Euler(90, 0, 0);
                RotationX.SetValueWithoutNotify(0);
                RotationXInputField.SetTextWithoutNotify("0");
                RotationY.SetValueWithoutNotify(0);
                RotationYInputField.SetTextWithoutNotify("0");
                RotationZ.SetValueWithoutNotify(0);
                RotationZInputField.SetTextWithoutNotify("0");
            }
        });


        hasInitialised = true;
    }

    public void SetUpUIControlls(Material selectedVolumeMaterial) {
        if (!hasInitialised) Initialise();
        DensitySlider.SetValueWithoutNotify(selectedVolumeMaterial.GetInt(densityPropertyNameID));
        LightingIntensitySlider.SetValueWithoutNotify(selectedVolumeMaterial.GetFloat(lightIntensityPropertyNameID));
        OpacitySlider.SetValueWithoutNotify(selectedVolumeMaterial.GetFloat(opacityPropertyNameID));
        EnableLightingToggle.SetIsOnWithoutNotify(AppManager.Instance.SelectedVolume.GetLightingEnabled());
        MinDepthInputField.text = selectedVolumeMaterial.GetFloat(minDepthPropertyNameID).ToString();
        MaxDepthInputField.text = selectedVolumeMaterial.GetFloat(maxDepthPropertyNameID).ToString();
    }

    public void UpdateRotation() {
        float newX = AppManager.Instance.SelectedVolumeTransform.rotation.eulerAngles.x;
        if (newX > RotationX.maxValue) RotationX.SetValueWithoutNotify(RotationX.maxValue);
        else if (newX < RotationX.minValue) RotationX.SetValueWithoutNotify(RotationX.minValue);
        else RotationX.SetValueWithoutNotify(newX);
        RotationXInputField.SetTextWithoutNotify(newX.ToString());

        float newY = AppManager.Instance.SelectedVolumeTransform.rotation.eulerAngles.y;
        if (newY > RotationY.maxValue) RotationY.SetValueWithoutNotify(RotationY.maxValue);
        else if (newY < RotationY.minValue) RotationY.SetValueWithoutNotify(RotationY.minValue);
        else RotationY.SetValueWithoutNotify(newY);
        RotationYInputField.SetTextWithoutNotify(newY.ToString());

        float newZ = AppManager.Instance.SelectedVolumeTransform.rotation.eulerAngles.z;
        if (newZ > RotationZ.maxValue) RotationZ.SetValueWithoutNotify(RotationZ.maxValue);
        else if (newZ < RotationZ.minValue) RotationZ.SetValueWithoutNotify(RotationZ.minValue);
        else RotationZ.SetValueWithoutNotify(newZ);
        RotationZInputField.SetTextWithoutNotify(newZ.ToString());
    }

    public void UpdatePositionZ() {
        float newZ = AppManager.Instance.SelectedVolumeTransform.position.z;
        ZoomSlider.SetValueWithoutNotify(newZ);
    }


}
