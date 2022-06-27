using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UISystem {
    public static class UISystemUtilities {

        private static readonly Func<CanvasGroup, bool, IEnumerator> onHover = new Func<CanvasGroup, bool, IEnumerator>((group, show) => {
            return UIUtilities.ToggleCanvasGroup(group, show, 0.15f);
        });
        public static void ToolBoxSetUp(MonoBehaviour thisMono, Transform thisTransform, Action<UnityEngine.Events.UnityAction> addListenerAction) {
            addListenerAction(AppManager.Instance.Render);
            if (thisTransform.childCount > 1) {
                CanvasGroup toolTipBox = thisTransform.GetChild(1).GetComponent<CanvasGroup>();

                EventTrigger eventTrigger = thisTransform.GetComponent<EventTrigger>();
                EventTrigger.Entry onHoverEnter = new EventTrigger.Entry();
                onHoverEnter.eventID = EventTriggerType.PointerEnter;
                RectTransform trans = null;
                if (toolTipBox && toolTipBox.isActiveAndEnabled) trans = toolTipBox.GetComponent<RectTransform>();
                Coroutine onHoverEnterCo = null;
                Coroutine onHoverEndCo = null;
                onHoverEnter.callback.AddListener(data => {
                    if (onHoverEndCo != null) thisMono.StopCoroutine(onHoverEndCo);
                    if (onHoverEnterCo != null) thisMono.StopCoroutine(onHoverEnterCo);
                    if (toolTipBox && toolTipBox.isActiveAndEnabled) onHoverEnterCo = thisMono.StartCoroutine(onHover.Invoke(toolTipBox, true));

                });
                eventTrigger.triggers.Add(onHoverEnter);

                EventTrigger.Entry onHoverExit = new EventTrigger.Entry();
                onHoverExit.eventID = EventTriggerType.PointerExit;
                onHoverExit.callback.AddListener(data => {
                    if (onHoverEnterCo != null) thisMono.StopCoroutine(onHoverEnterCo);
                    if (onHoverEndCo != null) thisMono.StopCoroutine(onHoverEndCo);
                    if (toolTipBox && toolTipBox.isActiveAndEnabled) onHoverEndCo = thisMono.StartCoroutine(onHover.Invoke(toolTipBox, false));
                });
                eventTrigger.triggers.Add(onHoverExit);
            }
        }
    }
}