using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MouseControls : MonoBehaviour {

    [SerializeField]
    private Vector2 zoomRange = new Vector2(-4, 5);

    void Update() {
        if (AppManager.Instance.SelectedVolume == null) return;
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0) {
            float newZ = AppManager.Instance.SelectedVolumeTransform.position.z + scroll;
            if (newZ > zoomRange.x && newZ < zoomRange.y) {
                AppManager.Instance.SelectedVolumeTransform.position += Vector3.forward * Input.mouseScrollDelta.y;
                TranslationUIHandlers.Instance.UpdatePositionZ();
            }
        }
        if (Input.GetMouseButton(1)) {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            if (mouseX != 0 || mouseY != 0) {
                AppManager.Instance.SelectedVolumeTransform.rotation *= Quaternion.Euler(mouseY, mouseX, 0);
                TranslationUIHandlers.Instance.UpdateRotation();
            }
        }


    }
}
