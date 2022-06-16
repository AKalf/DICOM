//using UnityEngine;
//using UnityEditor;

//namespace UnityVolumeRendering {
//    [CustomEditor(typeof(VolumeRenderedObject))]
//    public class VolumeRenderedObjectCustomInspector : Editor {
//        bool otherSettings = false;
//        public override void OnInspectorGUI() {
//            VolumeRenderedObject volrendObj = (VolumeRenderedObject)target;

//            // Render mode
//            RenderMode oldRenderMode = volrendObj.RenderMode;
//            RenderMode newRenderMode = (RenderMode)EditorGUILayout.EnumPopup("Render mode", oldRenderMode);
//            if (newRenderMode != oldRenderMode)
//                volrendObj.RenderMode = newRenderMode;

//            // Lighting settings
//            if (volrendObj.RenderMode == RenderMode.DirectVolumeRendering)
//                volrendObj.IsLightingEnabled = GUILayout.Toggle(volrendObj.IsLightingEnabled, "Enable lighting");
//            else
//                volrendObj.IsLightingEnabled = false;

//            // Visibility window
//            Vector2 visibilityWindow = volrendObj.VisibilityWindow;
//            float oldX = visibilityWindow.x;
//            float oldY = visibilityWindow.y;
//            EditorGUILayout.MinMaxSlider("Visible value range", ref visibilityWindow.x, ref visibilityWindow.y, 0.0f, 1.0f);
//            EditorGUILayout.Space();
//            volrendObj.VisibilityWindow = visibilityWindow;

//            // Transfer function type
//            TFRenderMode tfMode = (TFRenderMode)EditorGUILayout.EnumPopup("Transfer function type", volrendObj.TFRenderMode);
//            if (tfMode != volrendObj.TFRenderMode)
//                volrendObj.TFRenderMode = tfMode;

//            // Show TF button
//            if (GUILayout.Button("Edit transfer function")) {
//                if (tfMode == TFRenderMode.TF1D)
//                    TransferFunctionEditorWindow.ShowWindow();
//                else
//                    TransferFunction2DEditorWindow.ShowWindow();
//            }

//            // Other settings for direct volume rendering
//            if (volrendObj.RenderMode == RenderMode.DirectVolumeRendering) {
//                GUILayout.Space(10);
//                otherSettings = EditorGUILayout.Foldout(otherSettings, "Other Settings");
//                if (otherSettings) {
//                    // Temporary back-to-front rendering option
//                    volrendObj.IsDvrBackwardEnabled = GUILayout.Toggle(volrendObj.IsDvrBackwardEnabled, "Enable Back-to-Front Direct Volume Rendering");

//                    // Early ray termination for Front-to-back DVR
//                    if (!volrendObj.IsDvrBackwardEnabled) {
//                        volrendObj.IsRayTerminationEnabled = GUILayout.Toggle(volrendObj.IsRayTerminationEnabled, "Enable early ray termination");
//                    }
//                }
//            }
//        }
//    }
//}
