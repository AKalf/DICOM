using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class LoadingWindow : MonoBehaviour {

    private static LoadingWindow instance = null;
    public static LoadingWindow Instance => instance;
    [SerializeField] RectTransform loadingBarParent = null;
    [SerializeField] RectTransform[] loadingSegments = new RectTransform[6];
    CanvasGroup canvasGroup = null;
    [SerializeField]
    private bool isLoading = false;
    private void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (isLoading) {
            for (int i = 0; i < loadingSegments.Length; i++) {
                loadingSegments[i].position = Vector3.Lerp(loadingSegments[i].position, loadingSegments[i].position - Vector3.right * 10, Time.deltaTime);
                float x = loadingSegments[i].position.x + loadingSegments[i].rect.width;
                if (x < loadingBarParent.position.x - loadingBarParent.rect.width / 2) {
                    Vector3 pos = loadingSegments[i].position;
                    pos.x = loadingBarParent.position.x + loadingBarParent.rect.width / 2 + loadingSegments[i].rect.width + 20;
                    loadingSegments[i].position = pos;
                }
            }
        }
    }

    public void StartLoading() {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1;
        isLoading = true;
    }
    public void StopLoading() {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0;
        isLoading = false;
    }
}
