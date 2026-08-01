using System;

namespace ClientFramework.UI
{
    public enum UICacheMode
    {
        DestroyOnClose,
        HideOnClose
    }

    [Serializable]
    public struct UIPanelInfo
    {
        public string Address;
        public UILayer Layer;
        public UICacheMode CacheMode;

        public UIPanelInfo(
            string address,
            UILayer layer,
            UICacheMode cacheMode)
        {
            Address = address;
            Layer = layer;
            CacheMode = cacheMode;
        }
    }
}
