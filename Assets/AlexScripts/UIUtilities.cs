using System;
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
            float diff = lastValue - value;
            target.Invoke(Quaternion.AngleAxis(diff, dir));
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

    public static void SetPositionSliderControl(Slider slider, InputField inputField, Vector3 dir, Action<Vector3> target, float minValue, float maxValue, bool wholeNumbers) {
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.wholeNumbers = wholeNumbers;
        inputField.contentType = InputField.ContentType.DecimalNumber;

        float lastValue = 0.0f;
        slider.onValueChanged.AddListener(value => {
            if (target == null) return;
            float diff = lastValue - value;
            target.Invoke(dir * diff);
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
    public static void SetScaleSliderControl(Slider slider, InputField inputField, Vector3 dir, Action<Vector3> target, float minValue, float maxValue, bool wholeNumbers) {
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.wholeNumbers = wholeNumbers;
        inputField.contentType = InputField.ContentType.DecimalNumber;

        float lastValue = 0.0f;
        slider.onValueChanged.AddListener(value => {
            if (target == null) return;
            float diff = lastValue - value;
            target.Invoke(dir * diff);
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
}
