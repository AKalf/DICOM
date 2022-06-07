using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UIWindow : MonoBehaviour {

    [SerializeField] private Button closeButton, minimizeButton;
    [SerializeField] protected string windowName = "";
    public CanvasGroup CanvasGroup { get; private set; }
    public string WindowName => windowName;
    // Start is called before the first frame update
    protected void Start() {
        this.CanvasGroup = GetComponent<CanvasGroup>();
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (minimizeButton != null) minimizeButton.onClick.AddListener(() => MinimizedWindowsPanel.Instance.MinimizeWindow(this));
        OnStart();
    }

    protected virtual void OnStart() {

    }


    private void Close() {
        Destroy(this.gameObject);
    }
}
