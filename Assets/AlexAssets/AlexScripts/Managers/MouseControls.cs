using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityVolumeRendering;
public class MouseControls : MonoBehaviour {
    [SerializeField] UnityEngine.UI.RawImage volumeTexture = null;
    private void Awake() {
        OnScroll();
    }
    void Update() {
        if (AppManager.Instance.SelectedVolume == null) return;

        if (Input.GetMouseButtonDown(0) && Input.GetKeyDown(KeyCode.LeftControl) || Input.GetMouseButton(1)) {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            if (mouseX != 0 || mouseY != 0) {
                if (Input.GetMouseButton(1)) {
                    AppManager.Instance.SelectedVolumeTransform.rotation *= Quaternion.Euler(mouseY, mouseX, 0);
                    TranslationUIHandlers.Instance.UpdateRotation();
                }
                else {
                    AppManager.Instance.SelectedVolumeTransform.position += new Vector3(mouseY, mouseX, 0);
                    TranslationUIHandlers.Instance.UpdatePosition();

                }
            }
        }


    }

    private void OnScroll() {
        EventTrigger trigger = volumeTexture.GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Scroll;
        entry.callback.AddListener(data => {
            float scroll = Input.mouseScrollDelta.y;
            if (scroll != 0) {
                TranslationUIHandlers.Instance.UpdateZoomSlider(scroll);
            }
        });
        trigger.triggers.Add(entry);
    }
}
