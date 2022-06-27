using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityVolumeRendering {
    public class VolumeObjectFactory {
        public static VolumeRenderedObject CreateObject(VolumeDataset dataset) {


            GameObject meshContainer = GameObject.Instantiate((GameObject)Resources.Load("VolumeContainer"));
            //GameObject gizmo = VolumeContainerGizmoManager.GetGizmo(meshContainer.transform);
            //gizmo.transform.rotation = Quaternion.Euler(0, 35, 0);
            meshContainer.name = "VolumeObject_" + dataset.datasetName;
            VolumeRenderedObject volObj = meshContainer.AddComponent<VolumeRenderedObject>();
            meshContainer.transform.localScale = Vector3.one;
            meshContainer.transform.localPosition = Vector3.zero;

            volObj.dataset = dataset;


            return volObj;
        }

        public static CrossSectionPlane SpawnCrossSectionPlane(VolumeRenderedObject volobj) {
            GameObject quad = GameObject.Instantiate((GameObject)Resources.Load("CrossSectionPlane"));
            quad.transform.rotation = Quaternion.Euler(270.0f, 0.0f, 0.0f);
            CrossSectionPlane csplane = quad.gameObject.GetComponent<CrossSectionPlane>();
            csplane.targetObject = volobj;
            quad.transform.position = volobj.transform.position;

#if UNITY_EDITOR
            UnityEditor.Selection.objects = new UnityEngine.Object[] { quad };
#endif
            return csplane;
        }

        public static CutoutBox SpawnCutoutBox(VolumeRenderedObject volobj) {
            GameObject obj = GameObject.Instantiate((GameObject)Resources.Load("CutoutBox"));
            obj.transform.rotation = Quaternion.Euler(270.0f, 0.0f, 0.0f);
            CutoutBox cbox = obj.gameObject.GetComponent<CutoutBox>();
            cbox.targetObject = volobj;
            obj.transform.position = volobj.transform.position;

#if UNITY_EDITOR
            UnityEditor.Selection.objects = new UnityEngine.Object[] { obj };
#endif
            return cbox;
        }
    }
}
