using System;
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
    [SerializeField] Toggle EnableLightingToggle, EnableRayTermination, EnableBack2FrontRaycasting, EnableOpacityBasedOnDepth;
    [SerializeField]
    InputField DensityInputField, LightIntensityInputField, OpacityInputField,
        MinDepthInputField, MaxDepthInputField,
        MinVisibilityInputField, MaxVisibilityInputField,
        RotationXInputField, RotationYInputField, RotationZInputField;
    [SerializeField] Button importRawButton = null, importPARCHG = null, importDICOM = null, resetRotationButton;
    [SerializeField] Dropdown transferFunctionTypeDropdown, renderModeDropdown;

    private VolumeRenderedObject SelectedVolume => AppManager.Instance.SelectedVolume;

    private int
           // Property indexes
           densityPropertyIndex, visibleRangeMinPropertyIndex, visibleRangeMaxPropertyIndex, lightIntensityPropertyIndex, opacityPropertyIndex, minDepthPropertyIndex, maxDepthPropertyIndex,
           // Name IDs
           densityPropertyNameID, visibleRangeMinPropertyNameID, visibleRangeMaxPropertyNameID, lightIntensityPropertyNameID, opacityPropertyNameID, maxDepthPropertyNameID, minDepthPropertyNameID;

    private Vector2 visibilityMinMax = Vector2.zero;
    private UnityVolumeRendering.RenderMode currentRenderMode = UnityVolumeRendering.RenderMode.DirectVolumeRendering;
    private TFRenderMode tfRenderMode = TFRenderMode.TF1D;
    private bool hasInitialised = false;



    private void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
        DensitySlider.wholeNumbers = true;

        UIUtilities.SetUpButtonListener(importRawButton, () => RuntimeFileBrowser.ShowOpenFileDialog(AppManager.Instance.OnOpenRAWDatasetResult, "DataFiles"));
        UIUtilities.SetUpButtonListener(importPARCHG, () => RuntimeFileBrowser.ShowOpenFileDialog(AppManager.Instance.OnOpenPARDatasetResult, "DataFiles"));
        UIUtilities.SetUpButtonListener(importDICOM, () => RuntimeFileBrowser.ShowOpenDirectoryDialog(AppManager.Instance.OnOpenDICOMDatasetResult));
        UIUtilities.SetDropdown(renderModeDropdown, index => {
            if (AppManager.Instance.SelectedVolume == null)
                return;
            UnityVolumeRendering.RenderMode newRenderMode = (UnityVolumeRendering.RenderMode)index;
            if (newRenderMode != currentRenderMode) {
                currentRenderMode = newRenderMode;
                switch (newRenderMode) {
                    case UnityVolumeRendering.RenderMode.DirectVolumeRendering: {
                            AppManager.Instance.SelectedVolumeMaterial.EnableKeyword("MODE_DVR");
                            AppManager.Instance.SelectedVolumeMaterial.DisableKeyword("MODE_MIP");
                            AppManager.Instance.SelectedVolumeMaterial.DisableKeyword("MODE_SURF");
                            break;
                        }
                    case UnityVolumeRendering.RenderMode.MaximumIntensityProjectipon: {
                            AppManager.Instance.SelectedVolumeMaterial.DisableKeyword("MODE_DVR");
                            AppManager.Instance.SelectedVolumeMaterial.EnableKeyword("MODE_MIP");
                            AppManager.Instance.SelectedVolumeMaterial.DisableKeyword("MODE_SURF");
                            break;
                        }
                    case UnityVolumeRendering.RenderMode.IsosurfaceRendering: {
                            AppManager.Instance.SelectedVolumeMaterial.DisableKeyword("MODE_DVR");
                            AppManager.Instance.SelectedVolumeMaterial.DisableKeyword("MODE_MIP");
                            AppManager.Instance.SelectedVolumeMaterial.EnableKeyword("MODE_SURF");
                            break;
                        }
                }
                visibilityMinMax = new Vector2(0, 1); // reset visibility window 
                bool useGradientTexture = tfRenderMode == TFRenderMode.TF2D || newRenderMode == UnityVolumeRendering.RenderMode.IsosurfaceRendering || EnableLightingToggle.isOn;
                AppManager.Instance.SelectedVolumeMaterial.SetTexture("_GradientTex", useGradientTexture ? SelectedVolume.dataset.GetGradientTexture() : null);
            }
        });

    }

    private void SetNewLayerRangeValues(float value, bool isMin) {
        AppManager.Instance.ChangeCameraStatus(true);
        value = (float)Math.Round(value, 2);
        if (isMin) {
            if (value >= visibilityMinMax.y) {
                value = visibilityMinMax.y - 0.01f;
                VisibleRangeMin.SetValueWithoutNotify(value);
            }
            visibilityMinMax.x = value;
            MinVisibilityInputField.SetTextWithoutNotify("Min: " + value);
        }
        else {
            if (value <= visibilityMinMax.x) {
                value = visibilityMinMax.x + 0.01f;
                VisibleRangeMax.SetValueWithoutNotify(value);
            }
            visibilityMinMax.y = value;
            MaxVisibilityInputField.SetTextWithoutNotify("Max: " + value);
        }
        AppManager.Instance.SelectedVolumeMaterial.SetFloat(visibleRangeMinPropertyNameID, visibilityMinMax.x);
        AppManager.Instance.SelectedVolumeMaterial.SetFloat(visibleRangeMaxPropertyNameID, visibilityMinMax.y);
        AppManager.Instance.ChangeCameraStatus(false);
    }
    private void SetNewLayerRangeValues(string stringValue, bool isMin) {
        float value = 0.0f;
        if (float.TryParse(stringValue, out value))
            SetNewLayerRangeValues(value, isMin);
    }
    private void SetLayerRangeSliders() {

        visibleRangeMinPropertyIndex = AppManager.Instance.SelectedVolumeMaterial.shader.FindPropertyIndex("_MinVal");
        visibleRangeMaxPropertyIndex = AppManager.Instance.SelectedVolumeMaterial.shader.FindPropertyIndex("_MaxVal");

        visibleRangeMinPropertyNameID = AppManager.Instance.SelectedVolumeMaterial.shader.GetPropertyNameId(visibleRangeMinPropertyIndex);
        visibleRangeMaxPropertyNameID = AppManager.Instance.SelectedVolumeMaterial.shader.GetPropertyNameId(visibleRangeMaxPropertyIndex);

        VisibleRangeMin.minValue = 0;
        VisibleRangeMin.value = 0;
        MinVisibilityInputField.text = VisibleRangeMin.value.ToString();
        VisibleRangeMin.maxValue = 0.9f;

        VisibleRangeMax.minValue = 0.1f;
        VisibleRangeMax.value = 1;
        MaxVisibilityInputField.text = VisibleRangeMax.value.ToString();
        VisibleRangeMax.maxValue = 1;

        visibilityMinMax = new Vector2(VisibleRangeMin.value, VisibleRangeMax.value);

        VisibleRangeMin.onValueChanged.AddListener(value => SetNewLayerRangeValues(value, true));
        VisibleRangeMax.onValueChanged.AddListener(value => SetNewLayerRangeValues(value, false));
        MinVisibilityInputField.onValueChanged.AddListener(value => SetNewLayerRangeValues(value, true));
        MaxVisibilityInputField.onValueChanged.AddListener(value => SetNewLayerRangeValues(value, false));
    }
    public void UpdateRotation() {
        AppManager.Instance.ChangeCameraStatus(true);
        float newX = (float)Math.Round(AppManager.Instance.SelectedVolumeTransform.rotation.eulerAngles.x, 1);
        if (newX > RotationX.maxValue) RotationX.SetValueWithoutNotify(RotationX.maxValue);
        else if (newX < RotationX.minValue) RotationX.SetValueWithoutNotify(RotationX.minValue);
        else RotationX.SetValueWithoutNotify(newX);
        RotationXInputField.SetTextWithoutNotify(newX.ToString());

        float newY = (float)Math.Round(AppManager.Instance.SelectedVolumeTransform.rotation.eulerAngles.y, 1);
        if (newY > RotationY.maxValue) RotationY.SetValueWithoutNotify(RotationY.maxValue);
        else if (newY < RotationY.minValue) RotationY.SetValueWithoutNotify(RotationY.minValue);
        else RotationY.SetValueWithoutNotify(newY);
        RotationYInputField.SetTextWithoutNotify(newY.ToString());

        float newZ = (float)Math.Round(AppManager.Instance.SelectedVolumeTransform.rotation.eulerAngles.z, 1);
        if (newZ > RotationZ.maxValue) RotationZ.SetValueWithoutNotify(RotationZ.maxValue);
        else if (newZ < RotationZ.minValue) RotationZ.SetValueWithoutNotify(RotationZ.minValue);
        else RotationZ.SetValueWithoutNotify(newZ);
        RotationZInputField.SetTextWithoutNotify(newZ.ToString());
        AppManager.Instance.ChangeCameraStatus(false);
    }
    public void UpdatePositionZ() {
        AppManager.Instance.ChangeCameraStatus(true);
        float newZ = AppManager.Instance.SelectedVolumeTransform.position.z;
        ZoomSlider.SetValueWithoutNotify((float)Math.Round(newZ, 1));
        AppManager.Instance.ChangeCameraStatus(false);

    }
    public void SetUpUIControlls(Material selectedVolumeMaterial) {
        if (!hasInitialised) Initialise();
        DensitySlider.SetValueWithoutNotify(selectedVolumeMaterial.GetInt(densityPropertyNameID));
        LightingIntensitySlider.SetValueWithoutNotify(selectedVolumeMaterial.GetFloat(lightIntensityPropertyNameID));
        OpacitySlider.SetValueWithoutNotify(selectedVolumeMaterial.GetFloat(opacityPropertyNameID));
        EnableLightingToggle.SetIsOnWithoutNotify(AppManager.Instance.SelectedVolumeMaterial.IsKeywordEnabled("LIGHTING_ON"));
        MinDepthInputField.text = selectedVolumeMaterial.GetFloat(minDepthPropertyNameID).ToString();
        MaxDepthInputField.text = selectedVolumeMaterial.GetFloat(maxDepthPropertyNameID).ToString();
    }
    private void Initialise() {

        // Density slider
        UIUtilities.SetUpSliderControl(DensitySlider, DensityInputField, "_Density", out densityPropertyIndex, out densityPropertyNameID, true);
        // Light intensity slider
        UIUtilities.SetUpSliderControl(LightingIntensitySlider, LightIntensityInputField, "_LightIntensity", out lightIntensityPropertyIndex, out lightIntensityPropertyNameID);
        // Opacity slider
        UIUtilities.SetUpSliderControl(OpacitySlider, OpacityInputField, "_Opacity", out opacityPropertyIndex, out opacityPropertyNameID);
        // Min-Max Layers
        SetLayerRangeSliders();
        // Min Depth
        minDepthPropertyIndex = AppManager.Instance.SelectedVolumeMaterial.shader.FindPropertyIndex("_MinDepth");
        minDepthPropertyNameID = AppManager.Instance.SelectedVolumeMaterial.shader.GetPropertyNameId(minDepthPropertyIndex);
        UIUtilities.SetInputField(MinDepthInputField, value => {
            float newValue = 0.0f;
            if (float.TryParse(value, out newValue))
                AppManager.Instance.SelectedVolumeMaterial.SetFloat(minDepthPropertyNameID, (float)Math.Round(newValue, 2));
        });
        // Max Depth
        maxDepthPropertyIndex = AppManager.Instance.SelectedVolumeMaterial.shader.FindPropertyIndex("_MaxDepth");
        maxDepthPropertyNameID = AppManager.Instance.SelectedVolumeMaterial.shader.GetPropertyNameId(maxDepthPropertyIndex);
        UIUtilities.SetInputField(MaxDepthInputField, value => {
            float newValue = 0.0f;
            if (float.TryParse(value, out newValue))
                AppManager.Instance.SelectedVolumeMaterial.SetFloat(maxDepthPropertyNameID, (float)Math.Round(newValue, 2));
        });

        // Toggles
        #region TOGGLES
        // Lighting
        UIUtilities.SetToggle(EnableLightingToggle, value => {
            if (value) AppManager.Instance.SelectedVolumeMaterial.EnableKeyword("LIGHTING_ON");
            else AppManager.Instance.SelectedVolumeMaterial.DisableKeyword("LIGHTING_ON");
            bool useGradientTexture = tfRenderMode == TFRenderMode.TF2D || currentRenderMode == UnityVolumeRendering.RenderMode.IsosurfaceRendering || value;
            AppManager.Instance.SelectedVolumeMaterial.SetTexture("_GradientTex", useGradientTexture ? AppManager.Instance.SelectedVolume.dataset.GetGradientTexture() : null);
            if (value != LightingIntensityCanvasGroup.interactable)
                LightingIntensityCanvasGroup.interactable = value;
        });
        // Ray termination
        //UIUtilities.SetToggle(EnableRayTermination, value => {
        //    if (value) AppManager.Instance.SelectedVolumeMaterial.EnableKeyword("RAY_TERMINATE_ON");
        //    else AppManager.Instance.SelectedVolumeMaterial.DisableKeyword("RAY_TERMINATE_ON");
        //});
        // Back-to-fron Raycasting
        UIUtilities.SetToggle(EnableBack2FrontRaycasting, value => {
            if (value) AppManager.Instance.SelectedVolumeMaterial.EnableKeyword("DVR_BACKWARD_ON");
            else AppManager.Instance.SelectedVolumeMaterial.DisableKeyword("DVR_BACKWARD_ON");
        });
        // Opacity based on Depth
        UIUtilities.SetToggle(EnableOpacityBasedOnDepth, value => {
            if (value) AppManager.Instance.SelectedVolumeMaterial.EnableKeyword("OPACITY_BASED_ON_DEPTH");
            else AppManager.Instance.SelectedVolumeMaterial.DisableKeyword("OPACITY_BASED_ON_DEPTH");
        });
        #endregion

        Vector3 currPos = AppManager.Instance.SelectedVolumeTransform.position;
        UIUtilities.SetPositionSliderControl(ZoomSlider, null, Vector3.forward, vec => AppManager.Instance.SelectedVolumeTransform.position = vec, currPos.z - 1, currPos.z + 5, false);

        UIUtilities.SetRotationSliderControl(RotationX, RotationXInputField, Vector3.right, qua => AppManager.Instance.SelectedVolumeTransform.rotation *= qua, false);
        UIUtilities.SetRotationSliderControl(RotationY, RotationYInputField, Vector3.up, qua => AppManager.Instance.SelectedVolumeTransform.rotation *= qua, false);
        UIUtilities.SetRotationSliderControl(RotationZ, RotationZInputField, Vector3.forward, qua => AppManager.Instance.SelectedVolumeTransform.rotation *= qua, false);

        UIUtilities.SetUpButtonListener(resetRotationButton, () => {
            AppManager.Instance.SelectedVolumeTransform.rotation = Quaternion.identity * Quaternion.Euler(90, 0, 0);
            RotationX.SetValueWithoutNotify(0);
            RotationXInputField.SetTextWithoutNotify("0");
            RotationY.SetValueWithoutNotify(0);
            RotationYInputField.SetTextWithoutNotify("0");
            RotationZ.SetValueWithoutNotify(0);
            RotationZInputField.SetTextWithoutNotify("0");
        });

        //UIUtilities.SetDropdown(transferFunctionTypeDropdown, index => {
        //    TFRenderMode value = (TFRenderMode)index;
        //    if (tfRenderMode != value) {
        //        tfRenderMode = value;
        //        if (tfRenderMode == TFRenderMode.TF1D && SelectedVolume.transferFunction != null)
        //            SelectedVolume.transferFunction.GenerateTexture();
        //        else if (SelectedVolume.transferFunction2D != null)
        //            SelectedVolume.transferFunction2D.GenerateTexture();
        //        if (tfRenderMode == TFRenderMode.TF2D) {
        //            AppManager.Instance.SelectedVolumeMaterial.SetTexture("_TFTex", SelectedVolume.transferFunction2D.GetTexture());
        //            AppManager.Instance.SelectedVolumeMaterial.EnableKeyword("TF2D_ON");
        //        }
        //        else {
        //            AppManager.Instance.SelectedVolumeMaterial.SetTexture("_TFTex", SelectedVolume.transferFunction.GetTexture());
        //            AppManager.Instance.SelectedVolumeMaterial.DisableKeyword("TF2D_ON");
        //        }
        //        bool useGradientTexture = tfRenderMode == TFRenderMode.TF2D || currentRenderMode == UnityVolumeRendering.RenderMode.IsosurfaceRendering || EnableLightingToggle.isOn;
        //        AppManager.Instance.SelectedVolumeMaterial.SetTexture("_GradientTex", useGradientTexture ? SelectedVolume.dataset.GetGradientTexture() : null);
        //    }
        //});
        hasInitialised = true;
    }
}
