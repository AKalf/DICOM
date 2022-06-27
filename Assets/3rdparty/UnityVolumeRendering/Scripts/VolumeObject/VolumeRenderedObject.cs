using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UnityVolumeRendering {
    [ExecuteInEditMode]
    public class VolumeRenderedObject : MonoBehaviour {

        //Variables
        public List<TFColourControlPoint> color_points = new List<TFColourControlPoint>();
        public List<TFAlphaControlPoint> alpha_points = new List<TFAlphaControlPoint>();

        [SerializeField, HideInInspector] public TransferFunction transferFunction;

        [SerializeField, HideInInspector] public TransferFunction2D transferFunction2D;

        [SerializeField, HideInInspector] public VolumeDataset dataset;

        [SerializeField, HideInInspector] private MeshRenderer meshRenderer = null;
        public MeshRenderer MeshRenderer => meshRenderer;

        private Material volumeMaterial = null;
        public Material VolumeMaterial => volumeMaterial;


        private void Awake() {
            meshRenderer = GetComponent<MeshRenderer>();
            volumeMaterial = meshRenderer.sharedMaterial;
        }
        private void Start() {

            const int noiseDimX = 512, noiseDimY = 512;
            Texture2D noiseTexture = NoiseTextureGenerator.GenerateNoiseTexture(noiseDimX, noiseDimY);
            TransferFunction tf = TransferFunctionDatabase.CreateTransferFunction();
            Texture2D tfTexture = tf.GetTexture();
            transferFunction = tf;

            //TransferFunction2D tf2D = TransferFunctionDatabase.CreateTransferFunction2D();
            //transferFunction2D = tf2D;

            VolumeMaterial.SetTexture("_DataTex", dataset.GetDataTexture());
            VolumeMaterial.SetTexture("_GradientTex", null);
            VolumeMaterial.SetTexture("_NoiseTex", noiseTexture);
            VolumeMaterial.SetTexture("_TFTex", tfTexture);

            VolumeMaterial.EnableKeyword("MODE_DVR");
            VolumeMaterial.DisableKeyword("MODE_MIP");
            VolumeMaterial.DisableKeyword("MODE_SURF");

            VolumeMaterial.DisableKeyword("TF2D_ON");
            VolumeMaterial.DisableKeyword("LIGHTING_ON");

            VolumeMaterial.SetFloat("_MinVal", 0);
            VolumeMaterial.SetFloat("_MaxVal", 1);
            VolumeMaterial.DisableKeyword("RAY_TERMINATE_ON");
            VolumeMaterial.EnableKeyword("DVR_BACKWARD_ON");
            VolumeMaterial.DisableKeyword("CUTOUT_ON");
            if (dataset.scaleX != 0.0f && dataset.scaleY != 0.0f && dataset.scaleZ != 0.0f) {
                float maxScale = Mathf.Max(dataset.scaleX, dataset.scaleY, dataset.scaleZ);
                transform.localScale = new Vector3(dataset.scaleX / maxScale, dataset.scaleY / maxScale, dataset.scaleZ / maxScale);
            }
        }

        public SlicingPlane CreateSlicingPlane() {
            GameObject sliceRenderingPlane = GameObject.Instantiate(Resources.Load<GameObject>("SlicingPlane"));
            //sliceRenderingPlane.transform.parent = transform;
            sliceRenderingPlane.transform.localPosition = Vector3.zero;
            //sliceRenderingPlane.transform.localRotation = Quaternion.Euler(0, 0, 90);
            sliceRenderingPlane.transform.localScale = Vector3.one * 0.1f; // TODO: Change the plane mesh instead and use Vector3.one
            MeshRenderer sliceMeshRend = sliceRenderingPlane.GetComponent<MeshRenderer>();
            sliceMeshRend.material = new Material(sliceMeshRend.sharedMaterial);
            Material sliceMat = sliceRenderingPlane.GetComponent<MeshRenderer>().sharedMaterial;
            sliceMat.SetTexture("_DataTex", dataset.GetDataTexture());
            sliceMat.SetTexture("_TFTex", transferFunction.GetTexture());
            sliceMat.SetMatrix("_parentInverseMat", transform.worldToLocalMatrix);
            sliceMat.SetMatrix("_planeMat", Matrix4x4.TRS(sliceRenderingPlane.transform.position, sliceRenderingPlane.transform.rotation, transform.lossyScale)); // TODO: allow changing scale
            VolumeMaterial.EnableKeyword("CUTOUT_ON");
            SlicingPlane slicingPlane = sliceRenderingPlane.GetComponent<SlicingPlane>();
            slicingPlane.VolumeTranform = this.transform;


            return slicingPlane;
        }


        public void NewTF() {
            TransferFunction tf = new TransferFunction();
            tf.alphaControlPoints = alpha_points;
            tf.colourControlPoints = color_points;
            transferFunction = tf;
            transferFunction.colourControlPoints.OrderBy(x => x.dataValue).ToList();
            VolumeMaterial.SetTexture("_TFTex", transferFunction.GetTexture());
        }


        public void ExposePointValues(Slider slider) {
            TFAlphaControlPoint new_alpha = new TFAlphaControlPoint();
            new_alpha.alphaValue = slider.value;
            alpha_points[0] = new_alpha;
            VolumeMaterial.SetTexture("_TFTex", transferFunction.GetTexture());

        }

        public void SaveTF(InputField filename) {
            TransferFunctionDatabase.SaveTransferFunction(transferFunction, Path.Combine(Application.streamingAssetsPath, filename.text + ".tf"));  //maybe change it to Persistent path because of admin access
        }

        public void ResetTF() {
            Debug.Log(transferFunction);
            transferFunction.ClearTF();
            VolumeMaterial.SetTexture("_TFTex", transferFunction.GetTexture());
        }


        public void SetTF(Texture2D texture) {
            transferFunction.SetTexture(texture);
            VolumeMaterial.SetTexture("_TFTex", transferFunction.GetTexture());
        }

        public void UpdateTFTextureOnShader() {
            VolumeMaterial.SetTexture("_TFTex", transferFunction.GetTexture());
        }
    }
}
