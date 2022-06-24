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
        // Start is called before the first frame update
        protected override void Start() {
            base.Start();
            UISystemUtilities.ToolBoxSetUp(this, transform, AddListener);
        }
        public void AddListener(UnityAction uAction) {
            base.onClick.AddListener(uAction);
        }
        public void RemoveListener(UnityAction uAction) {
            base.onClick.RemoveListener(uAction);
        }
    }
}
