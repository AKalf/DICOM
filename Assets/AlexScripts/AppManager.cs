using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityVolumeRendering;
using static UnityVolumeRendering.DICOMImporter;

public class AppManager : MonoBehaviour {

    private static AppManager instance = null;
    public static AppManager Instance => instance;

    private List<VolumeRenderedObject> volumeObjects = new List<VolumeRenderedObject>();
    [SerializeField] private VolumeRenderedObject selectedVolume = null; public VolumeRenderedObject SelectedVolume => selectedVolume;
    private Material selectedVolumeMaterial = null; public Material SelectedVolumeMaterial => selectedVolume.GetComponentInChildren<Renderer>().material;
    private Transform selectedVolumeTransform = null; public Transform SelectedVolumeTransform => selectedVolume.transform;


    private void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
    }
    // Start is called before the first frame update
    void Start() {
        if (selectedVolume != null) OnSelectVolume(selectedVolume);
        else {
            selectedVolume = FindObjectOfType<VolumeRenderedObject>();
            if (selectedVolume != null) OnSelectVolume(selectedVolume);
        }

    }



    private void OnSelectVolume(VolumeRenderedObject obj) {

        selectedVolume = obj;
        ShaderUIOptionsController.Instance.SetUpUIControlls(SelectedVolumeMaterial);
        selectedVolumeMaterial = selectedVolume.GetComponentInChildren<Renderer>().material;
        selectedVolumeTransform = selectedVolume.transform.GetChild(0).transform;

    }
    public void OnOpenPARDatasetResult(RuntimeFileBrowser.DialogResult result) {
        if (!result.cancelled) {
            DespawnAllDatasets();
            string filePath = result.path;
            IImageFileImporter parimporter = ImporterFactory.CreateImageFileImporter(ImageFileFormat.VASP);
            VolumeDataset dataset = parimporter.Import(filePath);
            if (dataset != null) OnSelectVolume(VolumeObjectFactory.CreateObject(dataset));
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
                if (dataset != null) OnSelectVolume(VolumeObjectFactory.CreateObject(dataset));

            }
        }
    }

    public IEnumerator OnOpenDICOMDatasetResult(RuntimeFileBrowser.DialogResult result) {
        Debug.Log("Result is canceled: " + result.cancelled);
        if (!result.cancelled) {
            // We'll only allow one dataset at a time in the runtime GUI (for simplicity)
            DespawnAllDatasets();

            bool recursive = true;

            // Read all files
            IEnumerable<string> fileCandidates = Directory.EnumerateFiles(result.path, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase));
            yield return new WaitForEndOfFrame();
            // Import the dataset
            IImageSequenceImporter importer = ImporterFactory.CreateImageSequenceImporter(ImageSequenceFormat.DICOM);
            IEnumerable<IImageSequenceSeries> seriesList = importer.LoadSeries(fileCandidates);
            float numVolumesCreated = 0;
            foreach (IImageSequenceSeries series in seriesList) {
                VolumeDataset dataset = null;
                Debug.Log("Stating importing coroutine");
                yield return StartCoroutine(importer.ImportSeries(series));

                Debug.Log("Loaded dataseted is not null");
                dataset = importer.LoadedDataset;
                Debug.Log("Importing coroutine finished: " + dataset.datasetName);
                // Spawn the object
                if (dataset != null) {
                    VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);
                    obj.transform.position = new Vector3(numVolumesCreated, 0, 0);
                    numVolumesCreated++;
                    OnSelectVolume(obj);
                }
            }
        }
    }



    private void DespawnAllDatasets() {
        VolumeRenderedObject[] volobjs = GameObject.FindObjectsOfType<VolumeRenderedObject>();
        foreach (VolumeRenderedObject volobj in volobjs) {
            GameObject.Destroy(volobj.gameObject);
        }
    }
}
