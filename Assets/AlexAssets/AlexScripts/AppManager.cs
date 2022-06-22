using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityVolumeRendering;

public class AppManager : MonoBehaviour {

    private static AppManager instance = null;
    public static AppManager Instance => instance;


    public delegate void OnSelectVolume(VolumeRenderedObject volume);
    private OnSelectVolume OnSelectVolumeEvent = null;
    private Camera mainCamera = null;

    private List<VolumeRenderedObject> volumeObjects = new List<VolumeRenderedObject>();
    [SerializeField] private VolumeRenderedObject selectedVolume = null; public VolumeRenderedObject SelectedVolume => selectedVolume;
    public Material SelectedVolumeMaterial => SelectedVolume.VolumeMaterial;
    private Transform selectedVolumeTransform = null; public Transform SelectedVolumeTransform => selectedVolume.transform;


    private void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) {
            Destroy(this);
            return;
        }
        mainCamera = Camera.main;

    }
    // Start is called before the first frame update
    void Start() {
        if (selectedVolume != null) SelectVolume(selectedVolume);
        else {
            selectedVolume = FindObjectOfType<VolumeRenderedObject>();
            if (selectedVolume != null) SelectVolume(selectedVolume);
        }
    }

    public void ChangeCameraStatus(bool status) {
        if (status) mainCamera.enabled = true;
        else StartCoroutine(DisableCameraWithDelay());
    }
    private IEnumerator DisableCameraWithDelay() {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        mainCamera.enabled = false;
    }
    private void SelectVolume(VolumeRenderedObject obj) {
        selectedVolume = obj;
        selectedVolumeTransform = selectedVolume.transform;
        OnSelectVolumeEvent.Invoke(selectedVolume);
    }
    public void OnOpenPARDatasetResult(RuntimeFileBrowser.DialogResult result) {
        if (!result.cancelled) {
            DespawnAllDatasets();
            string filePath = result.path;
            IImageFileImporter parimporter = ImporterFactory.CreateImageFileImporter(ImageFileFormat.VASP);
            VolumeDataset dataset = parimporter.Import(filePath);
            if (dataset != null) SelectVolume(VolumeObjectFactory.CreateObject(dataset));
        }
    }
    public void OnOpenRAWDatasetResult(RuntimeFileBrowser.DialogResult result) {
        if (!result.cancelled) {

            // We'll only allow one dataset at a time in the runtime GUI (for simplicity)
            DespawnAllDatasets();

            // Did the user try to import an .ini-file? Open the corresponding .raw file instead
            string filePath = result.path;
            if (System.IO.Path.GetExtension(filePath) == ".ini")
                filePath = filePath.Replace(".ini", ".raw");

            // Parse .ini file
            DatasetIniData initData = DatasetIniReader.ParseIniFile(filePath + ".ini");
            if (initData != null) {
                // Import the dataset
                RawDatasetImporter importer = new RawDatasetImporter(filePath, initData.dimX, initData.dimY, initData.dimZ, initData.format, initData.endianness, initData.bytesToSkip);
                VolumeDataset dataset = importer.Import();
                // Spawn the object
                if (dataset != null) SelectVolume(VolumeObjectFactory.CreateObject(dataset));

            }
        }
    }
    public IEnumerator OnOpenDICOMDatasetResult(RuntimeFileBrowser.DialogResult result) {
        if (!result.cancelled) {
            LoadingWindow.Instance.StartLoading();
            ChangeCameraStatus(true);
            // We'll only allow one dataset at a time in the runtime GUI (for simplicity)
            DespawnAllDatasets();

            bool recursive = false;

            // Read all files
            IEnumerable<string> fileCandidates = Directory.EnumerateFiles(result.path, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase));
            // Import the dataset
            IImageSequenceImporter importer = ImporterFactory.CreateImageSequenceImporter(ImageSequenceFormat.DICOM);
            IEnumerable<IImageSequenceSeries> seriesList = importer.LoadSeries(fileCandidates);
            float numVolumesCreated = 0;
            int loopIndex = 0;
            const int loopsPerFrame = 100;
            foreach (IImageSequenceSeries series in seriesList) {
                VolumeDataset dataset = null;
                yield return StartCoroutine(importer.ImportSeriesAsynch(series));
                dataset = importer.LoadedDataset;
                // Spawn the object
                if (dataset != null) {
                    VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);
                    obj.transform.position = new Vector3(numVolumesCreated, 0, 0);
                    numVolumesCreated++;
                    yield return new WaitUntil(() => obj != null);
                    yield return new WaitForEndOfFrame();
                    SelectVolume(obj);
                    LoadingWindow.Instance.StopLoading();
                }
                loopIndex++;
                if (loopIndex > loopsPerFrame) {
                    loopIndex = 0;
                    yield return new WaitForEndOfFrame();
                }
            }
            LoadingWindow.Instance.StopLoading();
            Destroy(FindObjectOfType<RuntimeFileBrowser.RuntimeFileBrowserComponent>().gameObject);
            ChangeCameraStatus(false);
        }
        else {
            LoadingWindow.Instance.StopLoading();
            Destroy(FindObjectOfType<RuntimeFileBrowser.RuntimeFileBrowserComponent>().gameObject);
        }
    }
    public void AddOnSelectVolumeEventListener(OnSelectVolume delegateFunction) {
        OnSelectVolumeEvent += delegateFunction;
    }
    public void RemoveOnSelectVolumeEventListener(OnSelectVolume delegateFunction) {
        OnSelectVolumeEvent -= delegateFunction;
    }


    private void DespawnAllDatasets() {
        VolumeRenderedObject[] volobjs = GameObject.FindObjectsOfType<VolumeRenderedObject>();
        foreach (VolumeRenderedObject volobj in volobjs) {
            GameObject.Destroy(volobj.gameObject);
        }
    }
}
