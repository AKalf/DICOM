using System;
using UnityEngine;
using UnityEngine.Events;

namespace UISystem.Elements {
    [RequireComponent(typeof(UnityEngine.EventSystems.EventTrigger))]
    public class UISystem_Toggle : UnityEngine.UI.Toggle {
        protected override void Start() {
            base.Start();
#if UNITY_EDITOR
            if (!Application.isPlaying) return; // pops errors when reloading otherwise
#endif
            UISystemUtilities.ToolBoxSetUp(this, transform, uAction => base.onValueChanged.AddListener(s => uAction()));
        }
        public void AddListener(UnityAction<bool> uAction) {
            base.onValueChanged.AddListener(uAction);
        }

        public void RemoveListener(UnityAction<bool> uAction) {
            base.onValueChanged.RemoveListener(uAction);
        }
    }

}
