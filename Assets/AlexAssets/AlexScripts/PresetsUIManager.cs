using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PresetsUIManager : UIWindow {
    [SerializeField] Transform parentPanel = null;
    [SerializeField] GameObject presetThumbnailPrefab = null;

    bool hasInit = false;
    public override void OnMaximize() {
        Initialise();
    }

    private void Initialise() {
        foreach (VolumePreset preset in PresetsLibrary.Instance.Presets) {
            GameObject presetInstance = Instantiate(presetThumbnailPrefab, parentPanel);
            Texture2D texture = new Texture2D(512, 512);
            texture.LoadImage(preset.Thumbnail);
            presetInstance.GetComponentInChildren<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2);
            presetInstance.GetComponentInChildren<Text>().text = preset.Name;
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
        hasInit = true;
    }
}
