﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimizedWindowsPanel : MonoBehaviour {

    private static MinimizedWindowsPanel instance = null;
    public static MinimizedWindowsPanel Instance => instance;

    [SerializeField] GameObject minimizedPanel = null, minimizedWindowPrefab = null;

    public List<UIWindow> windows = new List<UIWindow>();

    private Dictionary<GameObject, UIWindow> minimizedWindows = new Dictionary<GameObject, UIWindow>();

    private void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
    }

    public void MinimizeWindow(UIWindow window) {
        if (minimizedWindows.ContainsValue(window) == false) {
            AppManager.Instance.ChangeCameraStatus(true);
            UIUtilities.ToggleCanvasGroup(window.CanvasGroup, false);
            window.gameObject.SetActive(false);
            GameObject newMinPanel = Instantiate(minimizedWindowPrefab, minimizedPanel.transform);
            newMinPanel.GetComponent<Button>().onClick.AddListener(() => MaximizeWindow(newMinPanel));
            newMinPanel.GetComponentInChildren<Text>().text = window.WindowName;
            minimizedWindows.Add(newMinPanel, window);
            window.OnMinimize();
            AppManager.Instance.ChangeCameraStatus(false);
        }
    }

    private void MaximizeWindow(GameObject minimizedWindow) {
        if (minimizedWindows.ContainsKey(minimizedWindow)) {
            AppManager.Instance.ChangeCameraStatus(true);
            UIWindow window = minimizedWindows[minimizedWindow];
            UIUtilities.ToggleCanvasGroup(window.CanvasGroup, true);
            window.gameObject.SetActive(true);
            minimizedWindows.Remove(minimizedWindow);
            Destroy(minimizedWindow);
            window.OnMaximize();
            AppManager.Instance.ChangeCameraStatus(false);
        }
    }


}
