using System;
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
            value = (float)Math.Round(value, 1);
            float diff = lastValue - value;
            target.Invoke(Quaternion.AngleAxis(diff, dir));
            inputField.SetTextWithoutNotify(value.ToString());
            slider.SetValueWithoutNotify(value);
            lastValue = value;
            AppManager.Instance.Render();
        });
        if (inputField != null) {
            SetInputField(inputField, value => {
                float f = 0.0f;
                if (float.TryParse(value, out f)) {
                    f = (float)Math.Round(f, 1);
                    inputField.text = f.ToString();
                    slider.value = f;
                    AppManager.Instance.Render();
                }
            });
        }
    }

    public static void SetPositionSliderControl(Slider slider, InputField inputField, Vector3 dir, Action<Vector3> target, float minValue, float maxValue, bool wholeNumbers) {
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.wholeNumbers = wholeNumbers;


        slider.onValueChanged.AddListener(value => {
            if (target == null) return;
            value = (float)Math.Round(value, 2);
            target.Invoke(dir * value);
            if (inputField != null) inputField.SetTextWithoutNotify(value.ToString());
            slider.SetValueWithoutNotify(value);
            AppManager.Instance.Render();
        });
        if (inputField != null) {
            inputField.contentType = InputField.ContentType.DecimalNumber;
            SetInputField(inputField, value => {
                float f = 0.0f;
                if (float.TryParse(value, out f)) {
                    f = (float)Math.Round(f, 1);
                    inputField.text = f.ToString();
                    slider.value = f;
                    AppManager.Instance.Render();
                }
            });
        }

    }
    public static void SetScaleSliderControl(Slider slider, InputField inputField, Vector3 dir, Action<Vector3> target, float minValue, float maxValue, bool wholeNumbers) {
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.wholeNumbers = wholeNumbers;
        inputField.contentType = InputField.ContentType.DecimalNumber;

        float lastValue = 0.0f;
        slider.onValueChanged.AddListener(value => {
            value = (float)Math.Round(value, 1);
            if (target == null) return;
            float diff = lastValue - value;
            target.Invoke(dir * diff);
            inputField.SetTextWithoutNotify(value.ToString());
            slider.SetValueWithoutNotify(value);
            lastValue = value;
            AppManager.Instance.Render();
        });
        if (inputField != null) {
            SetInputField(inputField, value => {
                float f = 0.0f;
                if (float.TryParse(value, out f)) {
                    f = (float)Math.Round(f, 1);
                    inputField.text = f.ToString();
                    slider.value = f;
                    AppManager.Instance.Render();
                }
            });
        }
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
    public static System.Collections.IEnumerator ToggleCanvasGroup(CanvasGroup group, bool open, float speed) {
        if (open) {
            group.interactable = true;
            group.blocksRaycasts = true;
            while (group.alpha < 1) {
                group.alpha += speed;
                AppManager.Instance.Render();
                yield return new WaitForSeconds(0.05f);
            }
            group.alpha = 1;
        }
        else {
            group.interactable = false;
            group.blocksRaycasts = false;
            while (group.alpha > 0) {
                group.alpha -= speed;
                AppManager.Instance.Render();
                yield return new WaitForSeconds(0.05f);
            }
            group.alpha = 0;
        }
        yield return new WaitForEndOfFrame();
        AppManager.Instance.Render();
    }

    public static void SetUpSliderControl(Slider slider, InputField inputField, string propertyName, out int propertyIndex, out int nameID, bool onlyIntValues = false, Action<float> extraActionToInvoke = null) {
        propertyIndex = AppManager.Instance.SelectedVolumeMaterial.shader.FindPropertyIndex(propertyName);
        int temp = AppManager.Instance.SelectedVolumeMaterial.shader.GetPropertyNameId(propertyIndex);
        nameID = temp;
        Vector2 minMax = AppManager.Instance.SelectedVolumeMaterial.shader.GetPropertyRangeLimits(propertyIndex);
        slider.minValue = minMax.x;
        slider.maxValue = minMax.y;
        slider.onValueChanged.AddListener(value => {
            value = (float)Math.Round(value, 1);
            if (inputField != null) inputField.SetTextWithoutNotify(value.ToString());
            if (onlyIntValues) AppManager.Instance.SelectedVolumeMaterial.SetInt(temp, (int)value);
            else AppManager.Instance.SelectedVolumeMaterial.SetFloat(temp, value);
            AppManager.Instance.Render();
        });
        if (inputField != null) {
            SetInputField(inputField, value => {
                int newIntValue;
                float newFloatValue;
                if (int.TryParse(value, out newIntValue)) {
                    AppManager.Instance.SelectedVolumeMaterial.SetInt(temp, newIntValue);
                    if (newIntValue > slider.maxValue) slider.SetValueWithoutNotify(slider.maxValue);
                    else if (newIntValue < slider.minValue) slider.SetValueWithoutNotify(slider.minValue);
                    else slider.SetValueWithoutNotify(newIntValue);
                    AppManager.Instance.Render();
                }
                else if (float.TryParse(value, out newFloatValue)) {
                    newFloatValue = (float)Math.Round(newFloatValue, 1);
                    AppManager.Instance.SelectedVolumeMaterial.SetFloat(temp, newFloatValue);
                    if (newFloatValue > slider.maxValue) slider.SetValueWithoutNotify(slider.maxValue);
                    else if (newFloatValue < slider.minValue) slider.SetValueWithoutNotify(slider.minValue);
                    else slider.SetValueWithoutNotify(newFloatValue);
                    AppManager.Instance.Render();
                }
            });
        }
    }
    public static void SetUpButtonListener(Button button, Action actionToInvoke) {
        button.onClick.AddListener(() => {
            actionToInvoke.Invoke();
            AppManager.Instance.Render();
        });
    }
    public static void SetInputField(InputField field, Action<string> actionToInvoke, float waitFrames = 1) {
        field.onValueChanged.AddListener(value => AppManager.Instance.Render());
        field.onEndEdit.AddListener(value => {
            actionToInvoke.Invoke(value);
            AppManager.Instance.Render();
        });
    }
    public static void SetDropdown(Dropdown dropdown, Action<int> actionToInvoke) {
        dropdown.onValueChanged.AddListener(index => {
            actionToInvoke.Invoke(index);
            AppManager.Instance.Render();
        });
    }
    public static void SetToggle(Toggle toggle, Action<bool> actionToInvoke) {
        toggle.onValueChanged.AddListener(value => {
            actionToInvoke.Invoke(value);
            AppManager.Instance.Render();
        });
    }
}
