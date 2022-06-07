using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityVolumeRendering;

public class SlicingControlls : UIWindow {
    [SerializeField] InputField inputFieldPosX, inputFieldPosY, inputFieldPosZ, inputFieldRotX, inputFieldRotY, inputFieldRotZ, inputFieldScaleX, inputFieldScaleY;
    [SerializeField] Slider posX, posY, posZ, rotX, rotY, rotZ, scaleX, scaleY;

    [SerializeField] Button openSlicingOptionsPanelButton = null, addSlicingPlaneButton, removeSlicingPlaneButton, selectFirstPlaneButton, selectSecondPlaneButton, selectThirdPlaneButton;

    private SlicingPlane currentPlane => totalPlanes[selectedPlaneIndex];
    private SlicingPlane[] totalPlanes = new SlicingPlane[4];
    public Transform PlaneTransform => totalPlanes[selectedPlaneIndex].transform;

    private int selectedPlaneIndex = 0;
    private int activePlanes = 0;
    private void Awake() {
        openSlicingOptionsPanelButton.onClick.AddListener(() => { UIUtilities.ToggleCanvasGroup(CanvasGroup, true); });
        addSlicingPlaneButton.onClick.AddListener(() => {
            if (activePlanes == 3) return;
            activePlanes++;
            totalPlanes[activePlanes] = AppManager.Instance.SelectedVolume.CreateSlicingPlane();
            SetButtonStatusByIndex(activePlanes, true);
            SetSlicingPlaneColor(activePlanes);
            selectedPlaneIndex = activePlanes;
            if (activePlanes == 1) {
                UIUtilities.SetPositionSliderControl(posX, inputFieldPosX, Vector3.right, vec => PlaneTransform.position += vec, -2, 2, false);
                UIUtilities.SetPositionSliderControl(posY, inputFieldPosY, Vector3.up, vec => PlaneTransform.position += vec, -2, 2, false);
                UIUtilities.SetPositionSliderControl(posZ, inputFieldPosZ, Vector3.forward, vec => PlaneTransform.position += vec, -2, 2, false);
                UIUtilities.SetRotationSliderControl(rotX, inputFieldRotX, Vector3.right, qua => PlaneTransform.rotation *= qua, false);
                UIUtilities.SetRotationSliderControl(rotY, inputFieldRotY, Vector3.up, qua => PlaneTransform.rotation *= qua, false);
                UIUtilities.SetRotationSliderControl(rotZ, inputFieldRotZ, Vector3.forward, qua => PlaneTransform.rotation *= qua, false);
                UIUtilities.SetScaleSliderControl(scaleX, inputFieldScaleX, Vector3.right, vec => PlaneTransform.localScale += vec, 0.1f, 3, false);
                UIUtilities.SetScaleSliderControl(scaleY, inputFieldScaleY, Vector3.up, vec => PlaneTransform.localScale += vec, 0.1f, 3, false);
            }

        });
        removeSlicingPlaneButton.onClick.AddListener(() => {
            if (activePlanes == 0) return;
            if (currentPlane != null) {
                Destroy(currentPlane.gameObject);
                totalPlanes[activePlanes] = null;
                SetButtonStatusByIndex(activePlanes, false);
                if (selectedPlaneIndex == activePlanes)
                    selectedPlaneIndex--;
                activePlanes--;

            }
        });
        selectFirstPlaneButton.onClick.AddListener(() => selectedPlaneIndex = 1);
        selectSecondPlaneButton.onClick.AddListener(() => selectedPlaneIndex = 2);
        selectThirdPlaneButton.onClick.AddListener(() => selectedPlaneIndex = 3);
    }
    private void SetSlicingPlaneColor(int index) {
        Transform trans = totalPlanes[index].transform;
        Color color = index == 1 ? Color.red : index == 2 ? Color.blue : Color.green;
        for (int i = 0; i < trans.childCount; i++) {
            trans.GetChild(i).GetComponent<MeshRenderer>().material.color = color;
        }
    }
    private void SetButtonStatusByIndex(int index, bool enable) {
        switch (index) {
            case 1:
                if (enable) selectFirstPlaneButton.interactable = true;
                else selectFirstPlaneButton.interactable = false;
                break;
            case 2:
                if (enable) selectSecondPlaneButton.interactable = true;
                else selectSecondPlaneButton.interactable = false;
                break;
            case 3:
                if (enable) selectThirdPlaneButton.interactable = true;
                else selectThirdPlaneButton.interactable = false;
                break;

        }
    }
}
