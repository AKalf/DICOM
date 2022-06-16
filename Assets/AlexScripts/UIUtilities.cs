﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class UIUtilities {
    public static void SetRotationSliderControl(Slider slider, InputField inputField, Vector3 dir, Action<Quaternion> target, bool wholeNumbers, float minValue = -180, float maxValue = 180) {
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.wholeNumbers = wholeNumbers;
        inputField.contentType = InputField.ContentType.DecimalNumber;

        float lastValue = 0.0f;
        slider.onValueChanged.AddListener(value => {
            if (target == null) return;
            AppManager.Instance.ChangeCameraStatus(true);
            float diff = lastValue - value;
            target.Invoke(Quaternion.AngleAxis(diff, dir));
            inputField.SetTextWithoutNotify(value.ToString());
            slider.SetValueWithoutNotify(value);
            lastValue = value;
            AppManager.Instance.ChangeCameraStatus(false);
        });

        inputField.onEndEdit.AddListener(value => {
            float f = 0.0f;
            if (float.TryParse(value, out f)) {
                AppManager.Instance.ChangeCameraStatus(true);
                f = (float)Math.Round(f, 1);
                inputField.text = f.ToString();
                slider.value = f;
                AppManager.Instance.ChangeCameraStatus(false);
            }
        });
    }

    public static void SetPositionSliderControl(Slider slider, InputField inputField, Vector3 dir, Action<Vector3> target, float minValue, float maxValue, bool wholeNumbers) {
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.wholeNumbers = wholeNumbers;
        inputField.contentType = InputField.ContentType.DecimalNumber;

        float lastValue = 0.0f;
        slider.onValueChanged.AddListener(value => {
            if (target == null) return;
            AppManager.Instance.ChangeCameraStatus(true);
            float diff = lastValue - value;
            target.Invoke(dir * diff);
            inputField.SetTextWithoutNotify(value.ToString());
            slider.SetValueWithoutNotify(value);
            lastValue = value;
            AppManager.Instance.ChangeCameraStatus(false);
        });

        inputField.onEndEdit.AddListener(value => {
            float f = 0.0f;
            if (float.TryParse(value, out f)) {
                AppManager.Instance.ChangeCameraStatus(true);
                f = (float)Math.Round(f, 1);
                inputField.text = f.ToString();
                slider.value = f;
                AppManager.Instance.ChangeCameraStatus(false);
            }
        });
    }
    public static void SetScaleSliderControl(Slider slider, InputField inputField, Vector3 dir, Action<Vector3> target, float minValue, float maxValue, bool wholeNumbers) {
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.wholeNumbers = wholeNumbers;
        inputField.contentType = InputField.ContentType.DecimalNumber;

        float lastValue = 0.0f;
        slider.onValueChanged.AddListener(value => {
            AppManager.Instance.ChangeCameraStatus(true);
            if (target == null) return;
            float diff = lastValue - value;
            target.Invoke(dir * diff);
            inputField.SetTextWithoutNotify(value.ToString());
            slider.SetValueWithoutNotify(value);
            lastValue = value;
            AppManager.Instance.ChangeCameraStatus(false);
        });

        inputField.onEndEdit.AddListener(value => {
            float f = 0.0f;
            if (float.TryParse(value, out f)) {
                AppManager.Instance.ChangeCameraStatus(true);
                f = (float)Math.Round(f, 1);
                inputField.text = f.ToString();
                slider.value = f;
                AppManager.Instance.ChangeCameraStatus(false);
            }
        });
    }
    public static void ToggleCanvasGroup(CanvasGroup group, bool open) {
        if (open) {
            group.interactable = true;
            group.alpha = 1;
            group.blocksRaycasts = true;
        }
        else {
            group.interactable = false;
            group.alpha = 0;
            group.blocksRaycasts = false;
        }
    }

    public static void SetUpSliderControl(Slider slider, InputField inputField, string propertyName, out int propertyIndex, out int nameID, bool onlyIntValues = false) {
        propertyIndex = AppManager.Instance.SelectedVolumeMaterial.shader.FindPropertyIndex(propertyName);
        int temp = AppManager.Instance.SelectedVolumeMaterial.shader.GetPropertyNameId(propertyIndex);
        nameID = temp;
        Vector2 minMax = AppManager.Instance.SelectedVolumeMaterial.shader.GetPropertyRangeLimits(propertyIndex);
        slider.minValue = minMax.x;
        slider.maxValue = minMax.y;
        slider.onValueChanged.AddListener(value => {
            AppManager.Instance.ChangeCameraStatus(true);
            inputField.SetTextWithoutNotify(value.ToString());
            if (onlyIntValues) AppManager.Instance.SelectedVolumeMaterial.SetInt(temp, (int)value);
            else AppManager.Instance.SelectedVolumeMaterial.SetFloat(temp, value);
            AppManager.Instance.ChangeCameraStatus(false);
        });
        inputField.onEndEdit.AddListener(value => {
            int newIntValue;
            float newFloatValue;
            if (int.TryParse(value, out newIntValue)) {
                AppManager.Instance.ChangeCameraStatus(true);
                AppManager.Instance.SelectedVolumeMaterial.SetInt(temp, newIntValue);
                if (newIntValue > slider.maxValue) slider.SetValueWithoutNotify(slider.maxValue);
                else if (newIntValue < slider.minValue) slider.SetValueWithoutNotify(slider.minValue);
                else slider.SetValueWithoutNotify(newIntValue);
                AppManager.Instance.ChangeCameraStatus(false);
            }
            else if (float.TryParse(value, out newFloatValue)) {
                AppManager.Instance.ChangeCameraStatus(true);
                AppManager.Instance.SelectedVolumeMaterial.SetFloat(temp, newFloatValue);
                if (newFloatValue > slider.maxValue) slider.SetValueWithoutNotify(slider.maxValue);
                else if (newFloatValue < slider.minValue) slider.SetValueWithoutNotify(slider.minValue);
                else slider.SetValueWithoutNotify(newFloatValue);
                AppManager.Instance.ChangeCameraStatus(false);
            }
        });


    }
}
