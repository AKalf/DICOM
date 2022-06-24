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
        private new ButtonClickedEvent onClick = new ButtonClickedEvent();
        // Start is called before the first frame update
        protected override void Start() {
            base.Start();
            UISystemUtilities.ToolBoxSetUp(this, toolTipBox, transform, AddListener);
        }
        public void AddListener(UnityAction uAction) {
            base.onClick.AddListener(uAction);
        }
        public void RemoveListener(UnityAction uAction) {
            base.onClick.RemoveListener(uAction);
        }
    }
}
