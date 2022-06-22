using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
[ExecuteAlways]
public class LoadingWindow : MonoBehaviour {

    private static LoadingWindow instance = null;
    public static LoadingWindow Instance => instance;
    [SerializeField] float animationSpeed = 10.0f;
    [SerializeField] RectTransform loadingBarParent = null;
    [SerializeField] RectTransform[] loadingSegments = new RectTransform[6];
    [SerializeField] Text loadingText = null;
    CanvasGroup canvasGroup = null;
    [SerializeField]
    private IEnumerator loadCoroutine = null;
    private void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
        canvasGroup = GetComponent<CanvasGroup>();
        loadCoroutine = Load();
    }


    private IEnumerator Load() {
        while (true) {
            AppManager.Instance.ChangeCameraStatus(true);
            for (int i = 0; i < loadingSegments.Length; i++) {
                loadingSegments[i].position = Vector3.Lerp(loadingSegments[i].position, loadingSegments[i].position - Vector3.right * 10, animationSpeed);
                float x = loadingSegments[i].position.x + loadingSegments[i].rect.width;
                if (x < loadingBarParent.position.x) {
                    Vector3 pos = loadingSegments[i].localPosition;
                    pos.x = loadingBarParent.position.x + loadingBarParent.rect.width;
                    loadingSegments[i].localPosition = pos;
                }
                yield return new WaitForEndOfFrame();
            }
        }

    }

    public void StartLoading() {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1;
        StartCoroutine(loadCoroutine);
    }
    public void StopLoading() {
        AppManager.Instance.ChangeCameraStatus(true);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0;
        StopCoroutine(loadCoroutine);
        AppManager.Instance.ChangeCameraStatus(false);
    }

    public void SetLoadingPercnetage(float current, int from) {
        loadingText.text = "Loading... " + current + '/' + from;
    }
    public void SetLoadingMessage(string message) {
        loadingText.text = "Loading... " + message;
    }
}
