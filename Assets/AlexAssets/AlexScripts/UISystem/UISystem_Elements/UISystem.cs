using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UISystem {
    public static class UISystemUtilities {

        private static readonly Func<CanvasGroup, bool, IEnumerator> onHover = new Func<CanvasGroup, bool, IEnumerator>((group, show) => {
            return UIUtilities.ToggleCanvasGroup(group, show, 0.05f);
        });
        public static void ToolBoxSetUp(MonoBehaviour thisMono, CanvasGroup toolTipBox, Transform thisTransform, Action<UnityEngine.Events.UnityAction> addListenerAction) {
            if (toolTipBox == null) toolTipBox = thisTransform.GetChild(1).GetComponent<CanvasGroup>();
            addListenerAction(AppManager.Instance.Render);
            EventTrigger eventTrigger = thisTransform.GetComponent<EventTrigger>();
            EventTrigger.Entry onHoverEnter = new EventTrigger.Entry();
            onHoverEnter.eventID = EventTriggerType.PointerEnter;
            RectTransform trans = toolTipBox.GetComponent<RectTransform>();
            Coroutine onHoverEnterCo = null;
            Coroutine onHoverEndCo = null;
            onHoverEnter.callback.AddListener(data => {
                if (onHoverEndCo != null) thisMono.StopCoroutine(onHoverEndCo);
                if (onHoverEnterCo != null) thisMono.StopCoroutine(onHoverEnterCo);
                onHoverEnterCo = thisMono.StartCoroutine(onHover.Invoke(toolTipBox, true));

            });
            eventTrigger.triggers.Add(onHoverEnter);

            EventTrigger.Entry onHoverExit = new EventTrigger.Entry();
            onHoverExit.eventID = EventTriggerType.PointerExit;
            onHoverExit.callback.AddListener(data => {
                if (onHoverEnterCo != null) thisMono.StopCoroutine(onHoverEnterCo);
                if (onHoverEndCo != null) thisMono.StopCoroutine(onHoverEndCo);
                onHoverEndCo = thisMono.StartCoroutine(onHover.Invoke(toolTipBox, false));
            });
            eventTrigger.triggers.Add(onHoverExit);
        }
    }
}