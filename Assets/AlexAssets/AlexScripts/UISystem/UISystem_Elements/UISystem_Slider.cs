using UnityEngine;
using Slider = UnityEngine.UI.Slider;
using UnityEngine.Events;
namespace UISystem.Elements {
    [RequireComponent(typeof(UnityEngine.EventSystems.EventTrigger))]
    public class UISystem_Slider : Slider {
        private CanvasGroup toolTipBox = null;
        private new SliderEvent onValueChanged = new SliderEvent();
        protected override void Start() {
            base.Start();
            UISystemUtilities.ToolBoxSetUp(this, toolTipBox, transform, uAction => base.onValueChanged.AddListener(s => uAction()));
        }
        public void AddListener(UnityAction<float> uAction) {
            base.onValueChanged.AddListener(uAction);
        }

        public void RemoveListener(UnityAction<float> uAction) {
            base.onValueChanged.RemoveListener(uAction);
        }
    }

}