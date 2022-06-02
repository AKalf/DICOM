using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityVolumeRendering;

[CustomEditor(typeof(VolumeRenderedObject))]
public class VolumeRenderer_Inspector : VolumeRenderedObjectCustomInspector {

    VolumeRenderedObject context = null;
    Material contextMaterial = null;
    int numOfSamplesPropertyNameID = 0, lightingIntensityPropertyNameID = 0, opacityPropertyNameID, maxDepthProperyNameID;
    Vector2Int samplesRange = Vector2Int.zero;
    Vector2 lightingIntensityRange = Vector2.zero, opacityRange;
    private void OnEnable() {
        context = target as VolumeRenderedObject;
        contextMaterial = context.GetComponentInChildren<MeshRenderer>().sharedMaterial;
        int samplesPropertyID = contextMaterial.shader.FindPropertyIndex("_Density");
        int lightingPropertyID = contextMaterial.shader.FindPropertyIndex("_LightIntensity");
        int opacityPropertyID = contextMaterial.shader.FindPropertyIndex("_Opacity");
        int maxDepthPropertyID = contextMaterial.shader.FindPropertyIndex("_MaxDepth");
        numOfSamplesPropertyNameID = contextMaterial.shader.GetPropertyNameId(samplesPropertyID);
        lightingIntensityPropertyNameID = contextMaterial.shader.GetPropertyNameId(lightingPropertyID);
        opacityPropertyNameID = contextMaterial.shader.GetPropertyNameId(opacityPropertyID);
        maxDepthProperyNameID = contextMaterial.shader.GetPropertyNameId(maxDepthPropertyID);
        Vector2 vec = contextMaterial.shader.GetPropertyRangeLimits(samplesPropertyID);
        samplesRange.x = (int)vec.x;
        samplesRange.y = (int)vec.y;
        vec = contextMaterial.shader.GetPropertyRangeLimits(lightingPropertyID);
        lightingIntensityRange.x = vec.x;
        lightingIntensityRange.y = vec.y;
        vec = contextMaterial.shader.GetPropertyRangeLimits(opacityPropertyID);
        opacityRange.x = vec.x;
        opacityRange.y = vec.y;

    }
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        serializedObject.Update();


        contextMaterial.SetInt(numOfSamplesPropertyNameID, EditorGUILayout.IntSlider("Density: ", contextMaterial.GetInt(numOfSamplesPropertyNameID), samplesRange.x, samplesRange.y));
        contextMaterial.SetFloat(lightingIntensityPropertyNameID, EditorGUILayout.Slider("Light intensity: ", contextMaterial.GetFloat(lightingIntensityPropertyNameID), lightingIntensityRange.x, lightingIntensityRange.y));
        contextMaterial.SetFloat(opacityPropertyNameID, EditorGUILayout.Slider("Opacity: ", contextMaterial.GetFloat(opacityPropertyNameID), opacityRange.x, opacityRange.y));
        contextMaterial.SetFloat(maxDepthProperyNameID, EditorGUILayout.FloatField("MaxDepth: ", contextMaterial.GetFloat(maxDepthProperyNameID)));
        if (serializedObject.hasModifiedProperties)
            serializedObject.ApplyModifiedProperties();
    }

}
