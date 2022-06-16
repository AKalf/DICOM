//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using UnityVolumeRendering;
//using System;

//[CustomEditor(typeof(VolumeRenderedObject))]
//public class VolumeRenderer_Inspector : VolumeRenderedObjectCustomInspector {

//    VolumeRenderedObject context = null;
//    Material contextMaterial = null;
//    int densityPropertyNameID = 0, lightingIntensityPropertyNameID = 0, opacityPropertyNameID, maxDepthProperyNameID, minDepthProperyNameID;
//    Vector2 densityRange, lightingIntensityRange = Vector2.zero, opacityRange, maxDepthRange, minDepthRange;


//    private int SetUpShaderProperty(string name, out Vector2 range) {
//        int id = contextMaterial.shader.FindPropertyIndex(name);
//        range = contextMaterial.shader.GetPropertyRangeLimits(id);
//        return contextMaterial.shader.GetPropertyNameId(id);
//    }
//    private int SetUpShaderProperty(string name) {
//        int id = contextMaterial.shader.FindPropertyIndex(name);
//        return contextMaterial.shader.GetPropertyNameId(id);
//    }
//    private void DrawPropertyIntRange(int propertyNameId, string name, Vector2 limits) {
//        contextMaterial.SetInt(propertyNameId, EditorGUILayout.IntSlider(name, contextMaterial.GetInt(propertyNameId), (int)limits.x, (int)limits.y));
//    }

//    private void DrawPropertyFloatRange(int propertyNameId, string name, Vector2 limits) {
//        contextMaterial.SetFloat(propertyNameId, EditorGUILayout.Slider(name, contextMaterial.GetFloat(propertyNameId), limits.x, limits.y));
//    }
//    private void OnEnable() {
//        context = target as VolumeRenderedObject;
//        contextMaterial = context.GetComponentInChildren<MeshRenderer>().sharedMaterial;

//        densityPropertyNameID = SetUpShaderProperty("_Density", out densityRange);
//        lightingIntensityPropertyNameID = SetUpShaderProperty("_LightIntensity", out lightingIntensityRange);
//        opacityPropertyNameID = SetUpShaderProperty("_Opacity", out opacityRange);
//        maxDepthProperyNameID = SetUpShaderProperty("_MaxDepth", out maxDepthRange);
//        minDepthProperyNameID = SetUpShaderProperty("_MinDepth", out minDepthRange);

//    }
//    public override void OnInspectorGUI() {
//        base.OnInspectorGUI();

//        serializedObject.Update();

//        DrawPropertyIntRange(densityPropertyNameID, "Density:", densityRange);
//        DrawPropertyFloatRange(lightingIntensityPropertyNameID, "Light intensity:", lightingIntensityRange);
//        DrawPropertyFloatRange(opacityPropertyNameID, "Opacity:", opacityRange);
//        DrawPropertyFloatRange(minDepthProperyNameID, "MinDepth:", minDepthRange);
//        DrawPropertyFloatRange(maxDepthProperyNameID, "MaxDepth:", maxDepthRange);

//        if (serializedObject.hasModifiedProperties)
//            serializedObject.ApplyModifiedProperties();
//    }

//}
