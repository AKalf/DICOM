using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAction = UnityEngine.Events.UnityAction;
public class CameraDrawManager : MonoBehaviour {
    private static CameraDrawManager instance = null;
    public static CameraDrawManager Instance => instance;
    [SerializeField] private Camera volumeCamera = null;
    public Camera VolumeCamera => volumeCamera;
    [SerializeField] private RenderTexture targetTexture = null;

    public UnityAction RenderCall = null;
    public UnityAction StopRendering = null;
    private Coroutine drawCoroutine = null;
    private int drawCalls = 0;

    private void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
        volumeCamera.targetTexture = targetTexture;
    }

    public void Draw() {
        if (AppManager.Instance.SelectedVolumeTransform == null) return;
        if (drawCalls == 0 && drawCoroutine == null) drawCoroutine = StartCoroutine(DrawCoroutine());
        drawCalls++;
    }
    public Texture2D GetScreenShot() {
        volumeCamera.enabled = true;

        var renderTarget = RenderTexture.GetTemporary(1280, 1280);
        volumeCamera.targetTexture = renderTarget;
        volumeCamera.Render(); // this must be called after "rederTarget" is assign as targetTexture
        RenderTexture.active = renderTarget;
        Texture2D tex = new Texture2D(renderTarget.width, renderTarget.height);
        tex.ReadPixels(new Rect(0, 0, renderTarget.width, renderTarget.height), 0, 0);
        tex.Apply();
        volumeCamera.targetTexture = targetTexture;
        volumeCamera.enabled = false;
        return tex;
    }
    private IEnumerator DrawCoroutine() {
        yield return new WaitForEndOfFrame();
        while (drawCalls > 0) {
            yield return new WaitForEndOfFrame();
            volumeCamera.Render();
            drawCalls--;
        }
        drawCoroutine = null;
        yield break;
    }
}
