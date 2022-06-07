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
        createPlaneCutoutButton.onClick.AddListener(() => {
            if (AppManager.Instance.SelectedVolume) {
                if (sectionVolume != null) Destroy(sectionVolume.gameObject);
                cutOutMode.interactable = false;
                scaleZ.interactable = false;
                sectionPlane = VolumeObjectFactory.SpawnCrossSectionPlane(AppManager.Instance.SelectedVolume);
            }
        });
        createVolumeCutoutButton.onClick.AddListener(() => {
            if (AppManager.Instance.SelectedVolume) {
                if (sectionPlane != null) Destroy(sectionPlane.gameObject);
                cutOutMode.interactable = true;
                scaleZ.interactable = true;
                sectionVolume = VolumeObjectFactory.SpawnCutoutBox(AppManager.Instance.SelectedVolume);
            }
        });
        deleteCutout.onClick.AddListener(() => {
            if (sectionVolume != null) Destroy(sectionVolume.gameObject);
            if (sectionPlane != null) Destroy(sectionPlane.gameObject);
        });
        cutOutMode.onValueChanged.AddListener(index => {
            sectionVolume.cutoutType = (CutoutType)index;
        });
        UIUtilities.SetPositionSliderControl(posX, posInputFieldX, Vector3.right,
            vec => { if (sectionPlane != null) sectionPlane.transform.position += vec; else if (sectionVolume != null) sectionVolume.transform.position += vec; },
            minValue: -3, maxValue: 7, wholeNumbers: false);
        UIUtilities.SetPositionSliderControl(posY, posInputFieldY, Vector3.up,
            vec => { if (sectionPlane != null) sectionPlane.transform.position += vec; else if (sectionVolume != null) sectionVolume.transform.position += vec; },
            minValue: -3, maxValue: 7, wholeNumbers: false);
        UIUtilities.SetPositionSliderControl(posZ, posInputFieldZ, Vector3.forward,
            vec => { if (sectionPlane != null) sectionPlane.transform.position += vec; else if (sectionVolume != null) sectionVolume.transform.position += vec; },
            minValue: -3, maxValue: 7, wholeNumbers: false);

        UIUtilities.SetRotationSliderControl(rotX, rotInputFieldX, Vector3.right,
            quat => { if (sectionPlane != null) sectionPlane.transform.rotation *= quat; else if (sectionVolume != null) sectionVolume.transform.rotation *= quat; },
            minValue: -180, maxValue: 180, wholeNumbers: false);
        UIUtilities.SetRotationSliderControl(rotY, rotInputFieldY, Vector3.up,
            quat => { if (sectionPlane != null) sectionPlane.transform.rotation *= quat; else if (sectionVolume != null) sectionVolume.transform.rotation *= quat; },
            minValue: -180, maxValue: 180, wholeNumbers: false);
        UIUtilities.SetRotationSliderControl(rotZ, rotInputFieldZ, Vector3.forward,
            quat => { if (sectionPlane != null) sectionPlane.transform.rotation *= quat; else if (sectionVolume != null) sectionVolume.transform.rotation *= quat; },
            minValue: -180, maxValue: 180, wholeNumbers: false);

        UIUtilities.SetScaleSliderControl(scaleX, scaleInputFieldX, Vector3.right,
            vec => { if (sectionPlane != null) sectionPlane.transform.localScale += vec; else if (sectionVolume != null) sectionVolume.transform.localScale += vec; },
            minValue: 0.1f, maxValue: 5, wholeNumbers: false);
        UIUtilities.SetScaleSliderControl(scaleY, scaleInputFieldY, Vector3.up,
            vec => { if (sectionPlane != null) sectionPlane.transform.localScale += vec; else if (sectionVolume != null) sectionVolume.transform.localScale += vec; },
            minValue: 0.1f, maxValue: 5, wholeNumbers: false);
        UIUtilities.SetScaleSliderControl(scaleZ, scaleInputFieldZ, Vector3.forward,
            vec => { if (sectionPlane != null) sectionPlane.transform.localScale += vec; else if (sectionVolume != null) sectionVolume.transform.localScale += vec; },
            minValue: 0.1f, maxValue: 5, wholeNumbers: false);
        openPanel.onClick.AddListener(() => {
            UIUtilities.ToggleCanvasGroup(panelGroup, true);
        });

    }

}
