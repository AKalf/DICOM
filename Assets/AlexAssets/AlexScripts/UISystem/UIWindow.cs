using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWindow : MonoBehaviour {

    [SerializeField] CanvasGroup windowToOpen = null;
    [SerializeField] Toggle windowToggle = null;
    [SerializeField] List<UIWindow> windowsToCloseWhenOpening = new List<UIWindow>();
    [SerializeField] List<UIWindow> windowsToCloseWhenClosing = new List<UIWindow>();
    [SerializeField] protected string windowName = "";
    [SerializeField] protected bool isClosedOnStart = false;
    private bool isOpen = false;
    public bool IsOpen {
        get => isOpen;
        set {
            isOpen = value;
            ToggleWindow(isOpen);
        }
    }

    public CanvasGroup CanvasGroup { get => windowToOpen; private set => windowToOpen = value; }
    public string WindowName => windowName;

    private void Awake() {
        if (windowToggle != null) windowToggle.onValueChanged.AddListener(value => IsOpen = value);
        else Debug.LogWarning("You have assigne a toggle-to-open-window");
        OnAwake();
    }
    // Start is called before the first frame update
    private void Start() {
        OnStart();
    }
    protected virtual void OnAwake() { }
    protected virtual void OnStart() {

    }
    private void ToggleWindow(bool show) {
        if (show) _OnMaximize();
        else _OnMinimize();
    }

    private void _OnMinimize() {
        UIUtilities.ToggleCanvasGroup(windowToOpen, false);
        foreach (UIWindow otherWindow in windowsToCloseWhenClosing)
            otherWindow.IsOpen = false;
        OnMinimize();
    }
    private void _OnMaximize() {
        UIUtilities.ToggleCanvasGroup(windowToOpen, true);
        foreach (UIWindow otherWindow in windowsToCloseWhenOpening)
            otherWindow.IsOpen = false;
        OnMaximize();
    }
    /// <summary>Extend functionality </summary>
    public virtual void OnMinimize() { }
    /// <summary>Extend functionality </summary>
    public virtual void OnMaximize() {

    }

}
