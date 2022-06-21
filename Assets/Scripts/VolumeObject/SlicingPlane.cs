using UnityEngine;

namespace UnityVolumeRendering {
    [ExecuteInEditMode]
    public class SlicingPlane : MonoBehaviour {
        public Transform VolumeTranform = null;
        private Material thisMaterial = null;

        //private MeshRenderer meshRenderer;

        private void Start() {
            thisMaterial = GetComponent<MeshRenderer>().sharedMaterial;
        }

        private void Update() {
            thisMaterial.SetMatrix("_parentInverseMat", VolumeTranform.worldToLocalMatrix);
            thisMaterial.SetMatrix("_planeMat", Matrix4x4.TRS(transform.position, transform.rotation, VolumeTranform.lossyScale)); // TODO: allow changing scale        
        }
    }
}
