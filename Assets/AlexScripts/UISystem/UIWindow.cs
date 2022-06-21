using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UIWindow : MonoBehaviour {

    [SerializeField] private Button closeButton, minimizeButton;
    [SerializeField] protected string windowName = "";
    [SerializeField] protected bool isClosedOnStart = false;
    public CanvasGroup CanvasGroup { get; private set; }
    public string WindowName => windowName;

    private void Awake() {
        OnAwake();
    }
    // Start is called before the first frame update
    private void Start() {
        this.CanvasGroup = GetComponent<CanvasGroup>();
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (minimizeButton != null) minimizeButton.onClick.AddListener(() => MinimizedWindowsPanel.Instance.MinimizeWindow(this));
        if (isClosedOnStart) minimizeButton.onClick.Invoke();
        OnStart();

    }
    protected virtual void OnAwake() { }
    protected virtual void OnStart() {

    }
    public virtual void OnMinimize() { }
    public virtual void OnMaximize() { }
    private void Close() {
        Destroy(this.gameObject);
    }
}
