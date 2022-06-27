using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace UISystem.Elements {
    [RequireComponent(typeof(EventTrigger))]
    public class UISystem_InputField : UnityEngine.UI.InputField {
        // Start is called before the first frame update
        protected override void Start() {
            base.Start();
#if UNITY_EDITOR
            if (!Application.isPlaying) return; // pops errors when reloading otherwise
#endif
            UISystemUtilities.ToolBoxSetUp(this, transform, uAction => {
                base.onValueChanged.AddListener(s => uAction());
                base.onEndEdit.AddListener(s => uAction());
            });
        }
        public void AddListenerOnValueChange(UnityAction<string> uAction) {
            base.onValueChanged.AddListener(uAction);
        }
        public void AddListenerOnEditEnd(UnityAction<string> uAction) {
            base.onEndEdit.AddListener(uAction);
        }
        public void RemoveListenerOnValueChange(UnityAction<string> uAction) {
            base.onValueChanged.RemoveListener(uAction);
        }
        public void RemoveListenerOnEditEnd(UnityAction<string> uAction) {
            base.onEndEdit.RemoveListener(uAction);
        }
    }
}
