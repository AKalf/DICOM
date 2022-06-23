using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityVolumeRendering;


public class ShaderUIOptionsController : UIWindow {

    private static ShaderUIOptionsController instance;
    public static ShaderUIOptionsController Instance => instance;


    [SerializeField] CanvasGroup LightingIntensityCanvasGroup;
    [SerializeField] Slider DensitySlider, LightingIntensitySlider, OpacitySlider, VisibleRangeMin, VisibleRangeMax;
    [SerializeField] Toggle EnableLightingToggle, EnableRayTermination, EnableBack2FrontRaycasting, EnableOpacityBasedOnDepth;
    [SerializeField]
    InputField
        DensityInputField, LightIntensityInputField, OpacityInputField,
        MinDepthInputField, MaxDepthInputField,
        MinVisibilityInputField, MaxVisibilityInputField;
    [SerializeField] Button savePreset;
    [SerializeField] Dropdown transferFunctionTypeDropdown, renderModeDropdown;

    private VolumeRenderedObject SelectedVolume => AppManager.Instance.SelectedVolume;
    private AppManager.OnSelectVolume onSelectVolumeEvent = null;

    private int
           // Property indexes
           densityPropertyIndex, visibleRangeMinPropertyIndex, visibleRangeMaxPropertyIndex, lightIntensityPropertyIndex, opacityPropertyIndex, minDepthPropertyIndex, maxDepthPropertyIndex,
           // Name IDs
           densityPropertyNameID, visibleRangeMinPropertyNameID, visibleRangeMaxPropertyNameID, lightIntensityPropertyNameID, opacityPropertyNameID, maxDepthPropertyNameID, minDepthPropertyNameID;

    private Vector2 visibilityMinMax = Vector2.zero;
    private UnityVolumeRendering.RenderMode currentRenderMode = UnityVolumeRendering.RenderMode.DirectVolumeRendering;
    private TFRenderMode tfRenderMode = TFRenderMode.TF1D;
    private bool hasInitialised = false;

    protected override void OnAwake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
        onSelectVolumeEvent = newVolume => SetUpUIControlls(AppManager.Instance.SelectedVolumeMaterial);
        AppManager.Instance.AddOnSelectVolumeEventListener(onSelectVolumeEvent);

    }
    protected override void OnStart() {
        savePreset.onClick.AddListener(SavePreset);
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
        EventTrigger trigger = renderModeDropdown.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener(data => AppManager.Instance.ChangeCameraStatus(true));
        trigger.triggers.Add(entry);
    }

    public void SavePreset() {
        VolumePreset settings = new VolumePreset();
        settings.Name = SelectedVolume.dataset.datasetName;
        settings.Position = AppManager.Instance.SelectedVolumeTransform.position;
        settings.Rotation = AppManager.Instance.SelectedVolumeTransform.rotation;
        settings.Scale = AppManager.Instance.SelectedVolumeTransform.localScale;
        settings.IsLighted = EnableLightingToggle.isOn;
        settings.AreRaysTerminated = EnableRayTermination.isOn;
        settings.IsBack2FrontRaycasted = EnableBack2FrontRaycasting.isOn;
        settings.IsOpacityBasedOnDepth = EnableOpacityBasedOnDepth.isOn;
        settings.Density = DensitySlider.value;
        settings.LightIntensity = LightingIntensitySlider.value;
        settings.Opacity = OpacitySlider.value;
        settings.VisibleRangeMin = visibilityMinMax.x;
        settings.VisibleRangeMax = visibilityMinMax.y;
        float minDepth = 0.0f;
        if (float.TryParse(MinDepthInputField.text, out minDepth))
            settings.MinimumDepth = (float)Math.Round(minDepth, 2);
        float maxDepth = 0.0f;
        if (float.TryParse(MaxDepthInputField.text, out maxDepth))
            settings.MaximumDepth = (float)Math.Round(maxDepth, 2);
        settings.RenderMode = currentRenderMode;
        settings.TransferFunction = SelectedVolume.transferFunction;


        AppManager.Instance.ForceEnableCamera();
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas can in canvases)
            can.gameObject.SetActive(false);
        Camera mainCamera = Camera.main;

        var renderTarget = RenderTexture.GetTemporary(512, 512);
        //mainCamera.orthographic = true;
        float oldFOV = mainCamera.fieldOfView;
        mainCamera.fieldOfView = 25;
        mainCamera.targetTexture = renderTarget;
        mainCamera.Render();
        RenderTexture.active = renderTarget;
        Texture2D tex = new Texture2D(renderTarget.width, renderTarget.height);
        tex.ReadPixels(new Rect(0, 0, renderTarget.width, renderTarget.height), 0, 0);
        tex.Apply();
        settings.Thumbnail = tex.EncodeToPNG();
        mainCamera.targetTexture = null;
        mainCamera.orthographic = false;
        PresetsLibrary.Instance.SavePreset(settings);
        AppManager.Instance.ChangeCameraStatus(false);
        mainCamera.fieldOfView = oldFOV;
        foreach (Canvas can in canvases)
            can.gameObject.SetActive(true);
    }
    public void LoadPreset(VolumePreset preset) {
        AppManager.Instance.ChangeCameraStatus(true);
        AppManager.Instance.SelectedVolumeTransform.position = preset.Position;
        AppManager.Instance.SelectedVolumeTransform.rotation = preset.Rotation;
        AppManager.Instance.SelectedVolumeTransform.localScale = preset.Scale;
        EnableLightingToggle.isOn = preset.IsLighted;
        EnableRayTermination.isOn = preset.AreRaysTerminated;
        EnableBack2FrontRaycasting.isOn = preset.IsBack2FrontRaycasted;
        EnableRayTermination.isOn = preset.IsOpacityBasedOnDepth;
        DensitySlider.value = preset.Density;
        LightingIntensitySlider.value = preset.LightIntensity;
        OpacitySlider.value = preset.Opacity;
        VisibleRangeMin.value = preset.VisibleRangeMin;
        VisibleRangeMax.value = preset.VisibleRangeMax;
        MinDepthInputField.text = preset.MinimumDepth.ToString();
        MaxDepthInputField.text = preset.MaximumDepth.ToString();
        renderModeDropdown.value = (int)preset.RenderMode;
        SelectedVolume.transferFunction = preset.TransferFunction;
        SelectedVolume.UpdateTFTextureOnShader();
        AppManager.Instance.ChangeCameraStatus(false);
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
        UIUtilities.SetInputField(MinVisibilityInputField, value => SetNewLayerRangeValues(value, true));
        UIUtilities.SetInputField(MaxVisibilityInputField, value => SetNewLayerRangeValues(value, false));
    }
    private void SetUpUIControlls(Material selectedVolumeMaterial) {
        if (!hasInitialised) Initialise();
        DensitySlider.wholeNumbers = true;
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
        UIUtilities.SetToggle(EnableRayTermination, value => {
            if (value) AppManager.Instance.SelectedVolumeMaterial.EnableKeyword("RAY_TERMINATE_ON");
            else AppManager.Instance.SelectedVolumeMaterial.DisableKeyword("RAY_TERMINATE_ON");
        });
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
