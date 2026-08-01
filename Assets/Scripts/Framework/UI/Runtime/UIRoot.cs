using UnityEngine;

namespace ClientFramework.UI
{
    public sealed class UIRoot : MonoBehaviour
    {
        [SerializeField] private Transform normalRoot;
        [SerializeField] private Transform popupRoot;
        [SerializeField] private Transform tipsRoot;
        [SerializeField] private Transform topRoot;

        public Transform GetLayer(UILayer layer)
        {
            switch (layer)
            {
                case UILayer.Popup:
                    return popupRoot;
                case UILayer.Tips:
                    return tipsRoot;
                case UILayer.Top:
                    return topRoot;
                default:
                    return normalRoot;
            }
        }
    }
}
