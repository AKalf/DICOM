using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VolumeContainerGizmoManager : MonoBehaviour {
    public enum VectorDirection { Forward, Up, Right }

    private static Dictionary<Transform, VolumeContainerGizmo> gizmos = new Dictionary<Transform, VolumeContainerGizmo>();
    private static Transform activeGizmo = null;

    private const int HANDLE_VALUE_DEVIDER = 2500;
    private static void ChangeActiveGizmo(Transform target) {
        if (activeGizmo != null) {
            gizmos[activeGizmo].gameObject.SetActive(false);
            activeGizmo = null;
        }
        if (gizmos.ContainsKey(target)) {
            gizmos[target].gameObject.SetActive(true);
            activeGizmo = target;
        }
    }
    public static GameObject GetGizmo(Transform target) {
        if (gizmos.ContainsKey(target)) return gizmos[target].gameObject;

        GameObject gizmoGO = GameObject.Instantiate((GameObject)Resources.Load("VolumeContainerVectors_Holder"));
        VolumeContainerGizmo gizmoComponent = gizmoGO.AddComponent<VolumeContainerGizmo>();
        gizmoGO.transform.position = target.transform.position;
        gizmoGO.transform.position += new Vector3(1.5f, -0.5f, 1);
        gizmoComponent.Target = target;
        gizmos.Add(target, gizmoComponent);
        ChangeActiveGizmo(target);
        return gizmoGO;
    }

    public static void DestroyGizmo(Transform target) {
        if (gizmos.ContainsKey(target) == false) return;

        GameObject g = gizmos[target].gameObject;
        gizmos.Remove(target);
        Destroy(g);
    }
    private class VolumeContainerGizmo : MonoBehaviour {
        [SerializeField] private GameObject axisX, axisY, axisZ;
        [SerializeField] public Transform Target = null;
        private Vector2 initialDragPosition = Vector2.zero;
        private void Start() {
            axisX = transform.GetChild(0).gameObject;
            axisY = transform.GetChild(1).gameObject;
            axisZ = transform.GetChild(2).gameObject;
            if (axisX) SetTrigger(axisX, Vector3.right);
            if (axisY) SetTrigger(axisY, Vector3.up);
            if (axisZ) SetTrigger(axisZ, Vector3.forward);
        }
        private void SetTrigger(GameObject targetAxis, Vector3 direction) {
            EventTrigger trigger = targetAxis.AddComponent<EventTrigger>();
            EventTrigger.Entry beginDragEntry = new EventTrigger.Entry();
            beginDragEntry.eventID = EventTriggerType.BeginDrag;
            beginDragEntry.callback.AddListener(data => {
                AppManager.Instance.ChangeCameraStatus(true);
                initialDragPosition = ((PointerEventData)data).position;
                Debug.Log(initialDragPosition);
            });
            trigger.triggers.Add(beginDragEntry);


            EventTrigger.Entry dragEntry = new EventTrigger.Entry();
            dragEntry.eventID = EventTriggerType.Drag;
            dragEntry.callback.AddListener(data => {
                AppManager.Instance.ChangeCameraStatus(true);
                Vector3 result = Vector3.zero;
                if (direction == Vector3.up)
                    result = direction * (((PointerEventData)data).position.y - initialDragPosition.y);
                else
                    result = direction * (((PointerEventData)data).position.x - initialDragPosition.x);

                Debug.Log(result);
                result = result / HANDLE_VALUE_DEVIDER;
                Vector3 scale = targetAxis.transform.localScale;
                scale.z += (result.x + result.y);
                if (scale.z > 0.1f)
                    targetAxis.transform.localScale = scale;
                Target.position += result;
                initialDragPosition = ((PointerEventData)data).position;
            });
            trigger.triggers.Add(dragEntry);


            EventTrigger.Entry endDragEntry = new EventTrigger.Entry();
            endDragEntry.eventID = EventTriggerType.PointerUp;
            endDragEntry.callback.AddListener(data => {
                AppManager.Instance.ChangeCameraStatus(false);
            });
            trigger.triggers.Add(endDragEntry);
        }
    }
}
