using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Button = UnityEngine.UI.Button;
using Text = UnityEngine.UI.Text;
using System.Collections;

namespace UISystem.Elements {
    [RequireComponent(typeof(EventTrigger))]
    public class UISystem_Button : Button {
        private CanvasGroup toolTipBox = null;
        private EventTrigger eventTrigger = null;
        private new ButtonClickedEvent onClick = new ButtonClickedEvent();
        private UnityAction renderCamera = new UnityAction(() => AppManager.Instance.ChangeCameraStatus(true));
        private UnityAction stopRenderCamera = new UnityAction(() => AppManager.Instance.ChangeCameraStatus(false));
        private Coroutine onHover = null;
        // Start is called before the first frame update
        protected override void Start() {
            base.Start();
            toolTipBox = transform.GetChild(1).GetComponent<CanvasGroup>();
            AddListener(renderCamera);
            AddListener(stopRenderCamera);
            eventTrigger = GetComponent<EventTrigger>();
            EventTrigger.Entry onHoverEnter = new EventTrigger.Entry();
            onHoverEnter.eventID = EventTriggerType.PointerEnter;
            RectTransform trans = toolTipBox.GetComponent<RectTransform>();

            onHoverEnter.callback.AddListener(data => Tooltip(true, 0.05f));

            eventTrigger.triggers.Add(onHoverEnter);
            EventTrigger.Entry onHoverExit = new EventTrigger.Entry();
            onHoverExit.eventID = EventTriggerType.PointerExit;
            onHoverExit.callback.AddListener(data => Tooltip(false, 0.05f));
            eventTrigger.triggers.Add(onHoverExit);
        }
        public void AddListener(UnityAction uAction) {
            base.onClick.AddListener(uAction);
        }
        public void RemoveListener(UnityAction uAction) {
            base.onClick.RemoveListener(uAction);
        }
        private void Tooltip(bool show, float speed) {
            if (onHover != null)
                StopCoroutine(onHover);
            onHover = StartCoroutine(UIUtilities.ToggleCanvasGroup(toolTipBox, show, speed));
        }

    }
}
