using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityVolumeRendering;

public class CutoutControls : MonoBehaviour {
    [SerializeField] private CanvasGroup panelGroup = null;
    [SerializeField] private Button createPlaneCutoutButton, createVolumeCutoutButton, deleteCutout, openPanel;
    [SerializeField] private Slider posX, posY, posZ, rotX, rotY, rotZ, scaleX, scaleY, scaleZ;
    [SerializeField] private InputField posInputFieldX, posInputFieldY, posInputFieldZ, rotInputFieldX, rotInputFieldY, rotInputFieldZ, scaleInputFieldX, scaleInputFieldY, scaleInputFieldZ;
    [SerializeField] private Dropdown cutOutMode;
    private CrossSectionPlane sectionPlane = null;
    private CutoutBox sectionVolume = null;
    // Start is called before the first frame update
    void Start() {
        ToggleOptionsInteractivity(false);

        UIUtilities.SetUpButtonListener(createPlaneCutoutButton, () => {
            if (AppManager.Instance.SelectedVolume) {
                if (sectionVolume != null) Destroy(sectionVolume.gameObject);
                ToggleOptionsInteractivity(true);
                cutOutMode.interactable = false;
                sectionPlane = VolumeObjectFactory.SpawnCrossSectionPlane(AppManager.Instance.SelectedVolume);
            }
        });
        UIUtilities.SetUpButtonListener(createVolumeCutoutButton, () => {
            if (AppManager.Instance.SelectedVolume) {
                if (sectionPlane != null) Destroy(sectionPlane.gameObject);
                ToggleOptionsInteractivity(true);
                sectionVolume = VolumeObjectFactory.SpawnCutoutBox(AppManager.Instance.SelectedVolume);
            }
        });
        UIUtilities.SetUpButtonListener(deleteCutout, () => {
            if (sectionVolume != null) Destroy(sectionVolume.gameObject);
            if (sectionPlane != null) Destroy(sectionPlane.gameObject);
            ToggleOptionsInteractivity(false);
        });
        UIUtilities.SetDropdown(cutOutMode, index => {
            AppManager.Instance.ChangeCameraStatus(true);
            sectionVolume.cutoutType = (CutoutType)index;
            AppManager.Instance.ChangeCameraStatus(false);
        });
        UIUtilities.SetUpButtonListener(openPanel, () => UIUtilities.ToggleCanvasGroup(panelGroup, true));

        SetPositionSliders(-3, 7, false);
        SetRotationSliders(-180, 180, false);
        SetScaleSliders(0.1f, 5, false);

    }
    private void SetPositionSliders(int minValue = -3, int maxValue = 7, bool wholeNumbers = false) {
        Action<Vector3> action = vec => {
            if (sectionPlane != null) sectionPlane.transform.position += vec;
            else if (sectionVolume != null) sectionVolume.transform.position += vec;
        };
        UIUtilities.SetPositionSliderControl(posX, posInputFieldX, Vector3.right, action, minValue, maxValue, wholeNumbers);
        UIUtilities.SetPositionSliderControl(posY, posInputFieldY, Vector3.up, action, minValue, maxValue, wholeNumbers);
        UIUtilities.SetPositionSliderControl(posZ, posInputFieldZ, Vector3.forward, action, minValue, maxValue, wholeNumbers);
    }
    private void SetRotationSliders(int minValue = -180, int maxValue = 180, bool wholeNumbers = false) {
        Action<Quaternion> action = quat => {
            if (sectionPlane != null) sectionPlane.transform.rotation *= quat;
            else if (sectionVolume != null) sectionVolume.transform.rotation *= quat;
        };
        UIUtilities.SetRotationSliderControl(rotX, rotInputFieldX, Vector3.right, action, wholeNumbers, minValue, maxValue);
        UIUtilities.SetRotationSliderControl(rotY, rotInputFieldY, Vector3.up, action, wholeNumbers, minValue, maxValue);
        UIUtilities.SetRotationSliderControl(rotZ, rotInputFieldZ, Vector3.forward, action, wholeNumbers, minValue, maxValue);
    }
    private void SetScaleSliders(float minValue = 0.1f, float maxValue = 5, bool wholeNumbers = false) {
        Action<Vector3> action = vec => {
            if (sectionPlane != null) sectionPlane.transform.localScale += vec;
            else if (sectionVolume != null) sectionVolume.transform.localScale += vec;
        };
        UIUtilities.SetScaleSliderControl(scaleX, scaleInputFieldX, Vector3.right, action, minValue, maxValue, wholeNumbers);
        UIUtilities.SetScaleSliderControl(scaleY, scaleInputFieldY, Vector3.up, action, minValue, maxValue, wholeNumbers);
        UIUtilities.SetScaleSliderControl(scaleZ, scaleInputFieldZ, Vector3.forward, action, minValue, maxValue, wholeNumbers);
    }

    private void ToggleOptionsInteractivity(bool enabled) {
        posX.interactable = enabled; posY.interactable = enabled; posZ.interactable = enabled;
        rotX.interactable = enabled; rotY.interactable = enabled; rotZ.interactable = enabled;
        scaleX.interactable = enabled; scaleY.interactable = enabled; scaleZ.interactable = enabled;
        posInputFieldX.interactable = enabled; posInputFieldY.interactable = enabled; posInputFieldZ.interactable = enabled;
        rotInputFieldX.interactable = enabled; rotInputFieldY.interactable = enabled; rotInputFieldZ.interactable = enabled;
        scaleInputFieldX.interactable = enabled; scaleInputFieldY.interactable = enabled; scaleInputFieldZ.interactable = enabled;
        cutOutMode.interactable = enabled;
    }
}
