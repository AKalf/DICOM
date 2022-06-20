using UnityEngine;

namespace UnityVolumeRendering {
    [ExecuteInEditMode]
    public class SlicingPlane : MonoBehaviour {
        public Transform VolumeTranform = null;
        private MeshRenderer meshRenderer;

        private void Start() {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        private void Update() {
            meshRenderer.sharedMaterial.SetMatrix("_parentInverseMat", VolumeTranform.worldToLocalMatrix);
            meshRenderer.sharedMaterial.SetMatrix("_planeMat", Matrix4x4.TRS(transform.position, transform.rotation, transform.parent.lossyScale)); // TODO: allow changing scale
        }
    }
}
