using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityVolumeRendering;
using System.IO;


public class DropDownTexturePicker : MonoBehaviour {

    private Dropdown dropdown;
    private List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
    //private Dropdown.OptionData option_data = new Dropdown.OptionData();
    private static DirectoryInfo directory = new DirectoryInfo(Application.streamingAssetsPath);    //need to implement the saved ones too
    private FileInfo[] t_functions = directory.GetFiles("*.*");
    public List<string> filepaths = new List<string>();

    [SerializeField]
    public VolumeRenderedObject volume;

    private TransferFunction tf;
    // Start is called before the first frame update
    void Start() {
        //Loads the transfer function presets
        foreach (FileInfo t_function in t_functions) {
            if (t_function.Name.Contains(".tf") && !t_function.Name.Contains("meta")) {
                Dropdown.OptionData option_data = new Dropdown.OptionData();
                filepaths.Add(t_function.FullName);
                option_data.text = t_function.Name;
                //option_data.image = *Placeholder*
                options.Add(option_data);
            }
        }

        dropdown = this.GetComponent<Dropdown>();
        //Change Dropdown Menu on Start depending on the number of options
        dropdown.AddOptions(options);
        
    }

    public void Update_TF() {
        if (volume == null)
        {
            volume = TF_Utilities.Instance.GetVolume();
        }
        
        tf = TransferFunctionDatabase.LoadTransferFunction(filepaths[dropdown.value]);
        volume.transferFunction = tf;
        volume.UpdateTFTextureOnShader();
    }


}
