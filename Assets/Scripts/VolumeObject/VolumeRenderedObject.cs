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

            VolumeMaterial.SetTexture("_DataTex", dataset.GetDataTexture());
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


        public SlicingPlane CreateSlicingPlane() {
            GameObject sliceRenderingPlane = GameObject.Instantiate(Resources.Load<GameObject>("SlicingPlane"));
            sliceRenderingPlane.transform.parent = transform;
            sliceRenderingPlane.transform.localPosition = Vector3.zero;
            sliceRenderingPlane.transform.localRotation = Quaternion.Euler(0, 0, 90);
            sliceRenderingPlane.transform.localScale = Vector3.one * 0.1f; // TODO: Change the plane mesh instead and use Vector3.one
            MeshRenderer sliceMeshRend = sliceRenderingPlane.GetComponent<MeshRenderer>();
            sliceMeshRend.material = new Material(sliceMeshRend.sharedMaterial);
            Material sliceMat = sliceRenderingPlane.GetComponent<MeshRenderer>().sharedMaterial;
            sliceMat.SetTexture("_DataTex", dataset.GetDataTexture());
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
