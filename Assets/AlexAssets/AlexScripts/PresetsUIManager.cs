using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PresetsUIManager : UIWindow {
    private static PresetsUIManager instance = null;
    public static PresetsUIManager Instance => instance;
    [SerializeField] Transform parentPanel = null;
    [SerializeField] GameObject presetThumbnailPrefab = null;
    [SerializeField] ScrollRect scrollRect = null;
    private Dictionary<VolumePreset, GameObject> spawnedPresets = new Dictionary<VolumePreset, GameObject>();

    protected override void OnAwake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
    }
    protected override void OnStart() {
        scrollRect.onValueChanged.AddListener(value => {
            AppManager.Instance.ChangeCameraStatus(true);
            AppManager.Instance.ChangeCameraStatus(false);
        });
    }


    public void SpawnThumbnails() {
        AppManager.Instance.ChangeCameraStatus(true);
        foreach (VolumePreset preset in PresetsLibrary.Instance.PresetsCopy) {
            if (spawnedPresets.ContainsKey(preset)) {
                Debug.Log("Already containes: " + preset.Name);
                continue;
            }
            GameObject presetInstance = Instantiate(presetThumbnailPrefab, parentPanel);
            spawnedPresets.Add(preset, presetInstance);
            Texture2D texture = new Texture2D(512, 512);
            texture.LoadImage(preset.Thumbnail);
            presetInstance.GetComponentInChildren<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2);
            presetInstance.GetComponentInChildren<Text>().text = preset.Name;
            Button deleteButton = presetInstance.GetComponentInChildren<Button>();
            deleteButton.onClick.AddListener(() => {
                AppManager.Instance.ChangeCameraStatus(true);
                DeleteInstance(preset);
                AppManager.Instance.ChangeCameraStatus(false);
            });

            EventTrigger trigger = presetInstance.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener(data => {
                AppManager.Instance.ChangeCameraStatus(true);
                ShaderUIOptionsController.Instance.LoadPreset(preset);
                AppManager.Instance.ChangeCameraStatus(false);
            });
            trigger.triggers.Add(entry);
        }
        AppManager.Instance.ChangeCameraStatus(false);
    }

    private void DeleteInstance(VolumePreset preset) {
        if (spawnedPresets.ContainsKey(preset) == false) return;
        PresetsLibrary.Instance.RemovePresetFromLibrary(preset);
        Destroy(spawnedPresets[preset]);
        spawnedPresets.Remove(preset);
    }
}
