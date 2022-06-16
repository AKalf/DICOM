using UnityEngine;
using UnityEngine.UI;

namespace UnityVolumeRendering {
    [ExecuteInEditMode]
    public class VolumeRenderedObject : MonoBehaviour {
        [SerializeField, HideInInspector] public TransferFunction transferFunction;

        [SerializeField, HideInInspector] public TransferFunction2D transferFunction2D;

        [SerializeField, HideInInspector] public VolumeDataset dataset;

        [SerializeField, HideInInspector] private MeshRenderer meshRenderer = null;
        public MeshRenderer MeshRenderer => meshRenderer;

        private Material volumeMaterial = null;
        public Material VolumeMaterial => volumeMaterial;

        [SerializeField, HideInInspector] private RenderMode renderMode;
        [SerializeField, HideInInspector] private TFRenderMode tfRenderMode;
        [SerializeField, HideInInspector] private bool lightingEnabled;

        [SerializeField, HideInInspector] private Vector2 visibilityWindow = new Vector2(0.0f, 1.0f);
        [SerializeField, HideInInspector] private bool rayTerminationEnabled = true;
        [SerializeField, HideInInspector] private bool dvrBackward = false;
        [SerializeField, HideInInspector] private bool opacityBasedOnDepthEnabled = false;



        private void Start() {
            meshRenderer = GetComponent<MeshRenderer>();
            volumeMaterial = meshRenderer.material;
            const int noiseDimX = 512, noiseDimY = 512;
            Texture2D noiseTexture = NoiseTextureGenerator.GenerateNoiseTexture(noiseDimX, noiseDimY);
            TransferFunction tf = TransferFunctionDatabase.CreateTransferFunction();
            Texture2D tfTexture = tf.GetTexture();
            transferFunction = tf;

            TransferFunction2D tf2D = TransferFunctionDatabase.CreateTransferFunction2D();
            transferFunction2D = tf2D;

            VolumeMaterial.SetTexture("_MainTex", dataset.GetDataTexture());
            VolumeMaterial.SetTexture("_GradientTex", null);
            VolumeMaterial.SetTexture("_NoiseTex", noiseTexture);
            VolumeMaterial.SetTexture("_TFTex", tfTexture);

            VolumeMaterial.EnableKeyword("MODE_DVR");
            VolumeMaterial.DisableKeyword("MODE_MIP");
            VolumeMaterial.DisableKeyword("MODE_SURF");

            if (dataset.scaleX != 0.0f && dataset.scaleY != 0.0f && dataset.scaleZ != 0.0f) {
                float maxScale = Mathf.Max(dataset.scaleX, dataset.scaleY, dataset.scaleZ);
                transform.localScale = new Vector3(dataset.scaleX / maxScale, dataset.scaleY / maxScale, dataset.scaleZ / maxScale);
            }
            UpdateAllMaterialProperties();
        }

        public bool IsLightingEnabled {
            get => lightingEnabled;
            set {
                if (value != lightingEnabled) {
                    AppManager.Instance.ChangeCameraStatus(true);
                    lightingEnabled = value;
                    if (lightingEnabled) volumeMaterial.EnableKeyword("LIGHTING_ON");
                    else volumeMaterial.DisableKeyword("LIGHTING_ON");
                    bool useGradientTexture = tfRenderMode == TFRenderMode.TF2D || renderMode == RenderMode.IsosurfaceRendering || lightingEnabled;
                    volumeMaterial.SetTexture("_GradientTex", useGradientTexture ? dataset.GetGradientTexture() : null);
                    AppManager.Instance.ChangeCameraStatus(false);
                }
            }
        }
        public bool IsRayTerminationEnabled {
            get => rayTerminationEnabled;
            set {
                if (value != rayTerminationEnabled) {
                    rayTerminationEnabled = value;
                    AppManager.Instance.ChangeCameraStatus(true);
                    if (rayTerminationEnabled) volumeMaterial.EnableKeyword("RAY_TERMINATE_ON");
                    else volumeMaterial.DisableKeyword("RAY_TERMINATE_ON");
                    AppManager.Instance.ChangeCameraStatus(false);
                }
            }
        }
        public bool IsDvrBackwardEnabled {
            get => dvrBackward;
            set {
                if (value != dvrBackward) {
                    dvrBackward = value;
                    AppManager.Instance.ChangeCameraStatus(true);
                    if (dvrBackward) volumeMaterial.EnableKeyword("DVR_BACKWARD_ON");
                    else volumeMaterial.DisableKeyword("DVR_BACKWARD_ON");
                    AppManager.Instance.ChangeCameraStatus(false);
                }
            }
        }
        public bool IsOpacityBasedOnDepthEnabled {
            get => opacityBasedOnDepthEnabled;
            set {
                if (value != opacityBasedOnDepthEnabled) {
                    opacityBasedOnDepthEnabled = value;
                    AppManager.Instance.ChangeCameraStatus(true);
                    if (opacityBasedOnDepthEnabled) volumeMaterial.EnableKeyword("OPACITY_BASED_ON_DEPTH");
                    else volumeMaterial.DisableKeyword("OPACITY_BASED_ON_DEPTH");
                    AppManager.Instance.ChangeCameraStatus(false);
                }
            }
        }
        public RenderMode RenderMode {
            get => renderMode;
            set {
                if (renderMode != value) {
                    AppManager.Instance.ChangeCameraStatus(true);
                    renderMode = value;
                    switch (renderMode) {
                        case RenderMode.DirectVolumeRendering: {
                                meshRenderer.material.EnableKeyword("MODE_DVR");
                                meshRenderer.material.DisableKeyword("MODE_MIP");
                                meshRenderer.material.DisableKeyword("MODE_SURF");
                                break;
                            }
                        case RenderMode.MaximumIntensityProjectipon: {
                                meshRenderer.material.DisableKeyword("MODE_DVR");
                                meshRenderer.material.EnableKeyword("MODE_MIP");
                                meshRenderer.material.DisableKeyword("MODE_SURF");
                                break;
                            }
                        case RenderMode.IsosurfaceRendering: {
                                meshRenderer.material.DisableKeyword("MODE_DVR");
                                meshRenderer.material.DisableKeyword("MODE_MIP");
                                meshRenderer.material.EnableKeyword("MODE_SURF");
                                break;
                            }
                    }
                    VisibilityWindow = new Vector2(0, 1); // reset visibility window 
                    bool useGradientTexture = tfRenderMode == TFRenderMode.TF2D || renderMode == RenderMode.IsosurfaceRendering || lightingEnabled;
                    meshRenderer.material.SetTexture("_GradientTex", useGradientTexture ? dataset.GetGradientTexture() : null);
                    AppManager.Instance.ChangeCameraStatus(false);
                }
            }
        }
        public TFRenderMode TFRenderMode {
            get => tfRenderMode;
            set {
                if (tfRenderMode != value) {
                    tfRenderMode = value;
                    if (tfRenderMode == TFRenderMode.TF1D && transferFunction != null)
                        transferFunction.GenerateTexture();
                    else if (transferFunction2D != null)
                        transferFunction2D.GenerateTexture();
                    if (tfRenderMode == TFRenderMode.TF2D) {
                        meshRenderer.material.SetTexture("_TFTex", transferFunction2D.GetTexture());
                        meshRenderer.material.EnableKeyword("TF2D_ON");
                    }
                    else {
                        meshRenderer.material.SetTexture("_TFTex", transferFunction.GetTexture());
                        meshRenderer.material.DisableKeyword("TF2D_ON");
                    }
                    bool useGradientTexture = tfRenderMode == TFRenderMode.TF2D || renderMode == RenderMode.IsosurfaceRendering || lightingEnabled;
                    meshRenderer.material.SetTexture("_GradientTex", useGradientTexture ? dataset.GetGradientTexture() : null);
                }
            }
        }
        public Vector2 VisibilityWindow {
            get => visibilityWindow;
            set {
                if (value != visibilityWindow) {
                    AppManager.Instance.ChangeCameraStatus(true);
                    visibilityWindow = value;
                    meshRenderer.material.SetFloat("_MinVal", visibilityWindow.x);
                    meshRenderer.material.SetFloat("_MaxVal", visibilityWindow.y);
                    AppManager.Instance.ChangeCameraStatus(false);
                }
            }
        }
        public SlicingPlane CreateSlicingPlane() {
            GameObject sliceRenderingPlane = GameObject.Instantiate(Resources.Load<GameObject>("SlicingPlane"));
            sliceRenderingPlane.transform.parent = transform;
            sliceRenderingPlane.transform.localPosition = Vector3.zero;
            sliceRenderingPlane.transform.localRotation = Quaternion.identity;
            sliceRenderingPlane.transform.localScale = Vector3.one * 0.1f; // TODO: Change the plane mesh instead and use Vector3.one
            MeshRenderer sliceMeshRend = sliceRenderingPlane.GetComponent<MeshRenderer>();
            sliceMeshRend.material = new Material(sliceMeshRend.sharedMaterial);
            Material sliceMat = sliceRenderingPlane.GetComponent<MeshRenderer>().sharedMaterial;
            sliceMat.SetTexture("_MainTex", dataset.GetDataTexture());
            sliceMat.SetTexture("_TFTex", transferFunction.GetTexture());
            sliceMat.SetMatrix("_parentInverseMat", transform.worldToLocalMatrix);
            sliceMat.SetMatrix("_planeMat", Matrix4x4.TRS(sliceRenderingPlane.transform.position, sliceRenderingPlane.transform.rotation, transform.lossyScale)); // TODO: allow changing scale

            return sliceRenderingPlane.GetComponent<SlicingPlane>();
        }

        private void UpdateAllMaterialProperties() {
            bool useGradientTexture = tfRenderMode == TFRenderMode.TF2D || renderMode == RenderMode.IsosurfaceRendering || lightingEnabled;
            meshRenderer.material.SetTexture("_GradientTex", useGradientTexture ? dataset.GetGradientTexture() : null);

            if (tfRenderMode == TFRenderMode.TF2D) {
                meshRenderer.material.SetTexture("_TFTex", transferFunction2D.GetTexture());
                meshRenderer.material.EnableKeyword("TF2D_ON");
            }
            else {
                meshRenderer.material.SetTexture("_TFTex", transferFunction.GetTexture());
                meshRenderer.material.DisableKeyword("TF2D_ON");
            }

            if (lightingEnabled)
                meshRenderer.material.EnableKeyword("LIGHTING_ON");
            else
                meshRenderer.material.DisableKeyword("LIGHTING_ON");

            switch (renderMode) {
                case RenderMode.DirectVolumeRendering: {
                        meshRenderer.material.EnableKeyword("MODE_DVR");
                        meshRenderer.material.DisableKeyword("MODE_MIP");
                        meshRenderer.material.DisableKeyword("MODE_SURF");
                        break;
                    }
                case RenderMode.MaximumIntensityProjectipon: {
                        meshRenderer.material.DisableKeyword("MODE_DVR");
                        meshRenderer.material.EnableKeyword("MODE_MIP");
                        meshRenderer.material.DisableKeyword("MODE_SURF");
                        break;
                    }
                case RenderMode.IsosurfaceRendering: {
                        meshRenderer.material.DisableKeyword("MODE_DVR");
                        meshRenderer.material.DisableKeyword("MODE_MIP");
                        meshRenderer.material.EnableKeyword("MODE_SURF");
                        break;
                    }
            }

            meshRenderer.material.SetFloat("_MinVal", visibilityWindow.x);
            meshRenderer.material.SetFloat("_MaxVal", visibilityWindow.y);

            if (rayTerminationEnabled) {
                meshRenderer.material.EnableKeyword("RAY_TERMINATE_ON");
            }
            else {
                meshRenderer.material.DisableKeyword("RAY_TERMINATE_ON");
            }

            if (dvrBackward) {
                meshRenderer.material.EnableKeyword("DVR_BACKWARD_ON");
            }
            else {
                meshRenderer.material.DisableKeyword("DVR_BACKWARD_ON");
            }

        }


    }
}
