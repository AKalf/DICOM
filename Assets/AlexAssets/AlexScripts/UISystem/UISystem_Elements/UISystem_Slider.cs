using UnityEngine;
using Slider = UnityEngine.UI.Slider;
using UnityEngine.Events;
namespace UISystem.Elements {
    [RequireComponent(typeof(UnityEngine.EventSystems.EventTrigger))]
    public class UISystem_Slider : Slider {
        protected override void Start() {
            base.Start();
#if UNITY_EDITOR
            if (!Application.isPlaying) return; // pops errors when reloading otherwise
#endif
            UISystemUtilities.ToolBoxSetUp(this, transform, uAction => base.onValueChanged.AddListener(s => uAction()));
        }
        public void AddListener(UnityAction<float> uAction) {
            base.onValueChanged.AddListener(uAction);
        }

        public void RemoveListener(UnityAction<float> uAction) {
            base.onValueChanged.RemoveListener(uAction);
        }
    }

}