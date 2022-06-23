using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimizedWindowsPanel : MonoBehaviour {

    private static MinimizedWindowsPanel instance = null;
    public static MinimizedWindowsPanel Instance => instance;

    [SerializeField] GameObject minimizedPanel = null, minimizedWindowPrefab = null;

    private Dictionary<UIWindow, GameObject> windows = new Dictionary<UIWindow, GameObject>();

    private void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
    }

    public void MinimizeWindow(UIWindow window) {
        AppManager.Instance.ChangeCameraStatus(true);
        if (windows.ContainsKey(window) == false) {
            GameObject newMinPanel = Instantiate(minimizedWindowPrefab, minimizedPanel.transform);
            newMinPanel.GetComponent<Button>().onClick.AddListener(() => MaximizeWindow(window));
            newMinPanel.GetComponentInChildren<Text>().text = window.WindowName;
            windows.Add(window, newMinPanel);
        }
        HideWindow(window);
        AppManager.Instance.ChangeCameraStatus(false);
    }
    private void MaximizeWindow(UIWindow window) {
        if (windows.ContainsKey(window)) {
            AppManager.Instance.ChangeCameraStatus(true);
            UIUtilities.ToggleCanvasGroup(window.CanvasGroup, true);
            window.gameObject.SetActive(true);
            windows[window].SetActive(false);
            window.OnMaximize();
            if (window.ShouldMinimzeOtherOnExpand) {
                foreach (UIWindow otherWindow in windows.Keys) {
                    if (otherWindow.Anchor == window.Anchor && otherWindow.Equals(window) == false) {
                        HideWindow(otherWindow);
                    }
                }
            }
            AppManager.Instance.ChangeCameraStatus(false);
        }
    }

    private void HideWindow(UIWindow window) {
        window.gameObject.SetActive(false);
        windows[window].SetActive(true);
        UIUtilities.ToggleCanvasGroup(window.CanvasGroup, false);
        window.OnMinimize();
    }

}
