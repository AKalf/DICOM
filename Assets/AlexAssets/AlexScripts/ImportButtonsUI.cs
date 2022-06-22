using UnityEngine;

public class ImportButtonsUI : MonoBehaviour {
    [SerializeField] UnityEngine.UI.Button importRawButton = null, importPARCHG = null, importDICOM = null;
    // Start is called before the first frame update
    void Start() {
        UnityVolumeRendering.RuntimeFileBrowser.RuntimeFileBrowserComponent fileBrowser = null;
        UIUtilities.SetUpButtonListener(importRawButton, () => {
            fileBrowser = UnityVolumeRendering.RuntimeFileBrowser.ShowOpenFileDialog(AppManager.Instance.OnOpenRAWDatasetResult, "DataFiles");
            fileBrowser.WindowName = "Select Raw File";
        });
        UIUtilities.SetUpButtonListener(importPARCHG, () => {
            fileBrowser = UnityVolumeRendering.RuntimeFileBrowser.ShowOpenFileDialog(AppManager.Instance.OnOpenPARDatasetResult, "DataFiles");
            fileBrowser.WindowName = "Select PARCHG File";
        });
        UIUtilities.SetUpButtonListener(importDICOM, () => {
            fileBrowser = UnityVolumeRendering.RuntimeFileBrowser.ShowOpenDirectoryDialog(AppManager.Instance.OnOpenDICOMDatasetResult);
            fileBrowser.WindowName = "Select DICOM Folder";
        });
    }

}
