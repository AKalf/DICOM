﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TranslationUIHandlers : UIWindow {

    private static TranslationUIHandlers instance = null;
    public static TranslationUIHandlers Instance => instance;

    [SerializeField] Slider ZoomSlider, RotationX, RotationY, RotationZ;
    [SerializeField] InputField RotationXInputField, RotationYInputField, RotationZInputField;
    [SerializeField] Button resetRotationButton;

    private AppManager.OnSelectVolume onSelectVolumeEvent = null;

    protected override void OnAwake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
        onSelectVolumeEvent = Initialise;
        AppManager.Instance.AddOnSelectVolumeEventListener(onSelectVolumeEvent);
    }

    public void Initialise(UnityVolumeRendering.VolumeRenderedObject volume) {
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
        AppManager.Instance.RemoveOnSelectVolumeEventListener(onSelectVolumeEvent);
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
}