using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityVolumeRendering;


public class PresetsLibrary : MonoBehaviour {

    private static PresetsLibrary instance = null;
    public static PresetsLibrary Instance => instance;
    [SerializeField][ReadOnly] private List<VolumePreset> presetsLibrary = new List<VolumePreset>();
    public VolumePreset[] PresetsCopy => presetsLibrary.ToArray();
    private readonly string pathToFile = Application.streamingAssetsPath + "/presets.json";

    private void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
    }
    private void Start() {
        LoadePresets();
    }

    public void SavePreset(VolumePreset preset) {
        if (presetsLibrary.Contains(preset)) return;
        presetsLibrary.Add(preset);
        SerializeLibrary();
        PresetsUIManager.Instance.SpawnThumbnails();
    }
    public void LoadePresets() {
        if (File.Exists(pathToFile) == false)
            return;
        string[] lines = File.ReadAllLines(pathToFile);
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
        if (presetsLibrary == null) presetsLibrary = new List<VolumePreset>();
        else presetsLibrary.Clear();
        foreach (string json in jsons) {
            VolumePreset p = JsonUtility.FromJson<VolumePreset>(json);
            if (p != null)
                presetsLibrary.Add(p);

        }
        PresetsUIManager.Instance.SpawnThumbnails();
    }
    public void RemovePresetFromLibrary(VolumePreset preset) {
        if (presetsLibrary.Contains(preset)) {
            presetsLibrary.Remove(preset);
            SerializeLibrary();
            PresetsUIManager.Instance.SpawnThumbnails();
        }
    }
    private void SerializeLibrary() {
        if (File.Exists(pathToFile) == false)
            File.Create(pathToFile).Dispose();
        string toJson = "";
        for (int i = 0; i < presetsLibrary.Count; i++) {
            toJson += JsonUtility.ToJson(presetsLibrary[i]);
            toJson += "\n-NEW_OBJ-\n";
        }
        Debug.Log("toJson: " + toJson);
        File.WriteAllText(pathToFile, toJson);
    }
}
[System.Serializable]
public class VolumePreset {
    public string Name;
    public bool IsLighted, AreRaysTerminated, IsBack2FrontRaycasted, IsOpacityBasedOnDepth;
    public float Density, LightIntensity, Opacity, VisibleRangeMin, VisibleRangeMax, MinimumDepth, MaximumDepth;
    public Vector3 Position, Scale;
    public Quaternion Rotation;
    public UnityVolumeRendering.VolumeRenderMode RenderMode;
    public TransferFunction TransferFunction;
    public byte[] Thumbnail;
    public VolumePreset() { }
    public VolumePreset(
        string name,
        Vector3 position, Quaternion rotation, Vector3 scale,
        bool isLighted, bool areRaysTerminated, bool isBack2FrontRaycast, bool isOpacityBasedOnDepth,
        float density, float lightIntensity, float opacity, float visibleRangeMin, float visibleRangeMax, float minDepth, float maxDepth,
        UnityVolumeRendering.VolumeRenderMode renderMode, TransferFunction transferFunction, byte[] thumnail
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
