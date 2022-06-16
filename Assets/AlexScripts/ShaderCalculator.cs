using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderCalculator : MonoBehaviour {

    [SerializeField] private ComputeShader compute;
    int kernel_renderStuff = -1;
    int propEffectTexture = -1;

    // Start is called before the first frame update
    void Start() {
        kernel_renderStuff = compute.FindKernel("DirectVolumeRenderingComputeShader");
        propEffectTexture = Shader.PropertyToID("Result");
    }

    // Update is called once per frame
    void Update() {

    }


    public void DoWork(Texture effectTexture) {
        int xDim = Mathf.Max(1, effectTexture.width / 8);
        int yDim = Mathf.Max(1, effectTexture.height / 8);
        int zDim = 1;
        if (effectTexture is Texture3D)
            zDim = Mathf.Max(1, ((Texture3D)effectTexture).depth / 8);
        compute.Dispatch(kernel_renderStuff, xDim, yDim, zDim);
    }
}
