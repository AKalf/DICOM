using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityVolumeRendering;
public class VolumesManager : MonoBehaviour {

    private static VolumesManager instance = null;
    public static VolumesManager Instance => instance;

    [SerializeField]
    VolumeRenderedObject currentVolume = null;

    private void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
