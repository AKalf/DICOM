using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityVolumeRendering;


public class PresetsLibrary : MonoBehaviour {
    [SerializeField]
    public List<VolumePreset> Presets = new List<VolumePreset>();
    private static PresetsLibrary instance = null;
    public static PresetsLibrary Instance => instance;
    private const string fileName = "/presets.json";

    private void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
    }
    private void Start() {
        LoadePresets();
    }
    public void SavePreset(VolumePreset preset) {
        string saveFile = Application.streamingAssetsPath + fileName;
        if (File.Exists(saveFile) == false)
            File.Create(saveFile).Dispose();
        Debug.Log(preset.Name);
        if (Presets.Contains(preset)) return;
        Presets.Add(preset);
        string toJson = "";
        for (int i = 0; i < Presets.Count; i++) {
            toJson += JsonUtility.ToJson(Presets[i]);
            toJson += "\n-NEW_OBJ-\n";
        }
        Debug.Log("toJson: " + toJson);
        File.WriteAllText(saveFile, toJson);
    }
    public void LoadePresets() {
        string saveFile = Application.streamingAssetsPath + fileName;
        Debug.Log(Application.streamingAssetsPath + fileName);
        if (File.Exists(saveFile) == false)
            return;
        string[] lines = File.ReadAllLines(saveFile);
        List<string> jsons = new List<string>();
        int currentJsonIndex = 0;
        jsons.Add("");
        for (int i = 0; i < lines.Length; i++) {
            if (lines[i] == "-NEW_OBJ-") {
                currentJsonIndex++;
                jsons.Add("");
            }
            else if (lines[i] != "\n") {
                jsons[currentJsonIndex] += lines[i];
            }
        }
        if (Presets == null) Presets = new List<VolumePreset>();
        foreach (string json in jsons) {
            VolumePreset p = JsonUtility.FromJson<VolumePreset>(json);
            if (p != null)
                Presets.Add(p);

        }
#if UNITY_EDITOR
        foreach (VolumePreset preset in Presets) {
            Debug.Log("Preset loaded: " + preset.Name);
        }
#endif
    }
}
[System.Serializable]
public class VolumePreset {
    public string Name;
    public bool IsLighted, AreRaysTerminated, IsBack2FrontRaycasted, IsOpacityBasedOnDepth;
    public float Density, LightIntensity, Opacity, VisibleRangeMin, VisibleRangeMax, MinimumDepth, MaximumDepth;
    public Vector3 Position, Scale;
    public Quaternion Rotation;
    public UnityVolumeRendering.RenderMode RenderMode;
    public TransferFunction TransferFunction;
    public byte[] Thumbnail;
    public VolumePreset() { }
    public VolumePreset(
        string name,
        Vector3 position, Quaternion rotation, Vector3 scale,
        bool isLighted, bool areRaysTerminated, bool isBack2FrontRaycast, bool isOpacityBasedOnDepth,
        float density, float lightIntensity, float opacity, float visibleRangeMin, float visibleRangeMax, float minDepth, float maxDepth,
        UnityVolumeRendering.RenderMode renderMode, TransferFunction transferFunction, byte[] thumnail
        ) {
        Name = name;
        Position = position;
        Rotation = rotation;
        Scale = scale;
        IsLighted = isLighted;
        AreRaysTerminated = areRaysTerminated;
        IsBack2FrontRaycasted = isBack2FrontRaycast;
        IsOpacityBasedOnDepth = isOpacityBasedOnDepth;
        Density = density;
        LightIntensity = lightIntensity;
        Opacity = opacity;
        VisibleRangeMin = visibleRangeMin;
        VisibleRangeMax = visibleRangeMax;
        MinimumDepth = minDepth;
        MaximumDepth = maxDepth;
        RenderMode = renderMode;
        TransferFunction = transferFunction;
        Thumbnail = thumnail;
    }

}
