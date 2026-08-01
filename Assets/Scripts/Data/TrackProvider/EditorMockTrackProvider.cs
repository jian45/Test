using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>Editor 埋点 Mock：Debug.Log 输出，不上传</summary>
public class EditorMockTrackProvider : ITrackProvider
{
    public void Track(string eventName, Dictionary<string, object> properties)
    {
#if UNITY_EDITOR
        Debug.Log("[Track][Mock] " + eventName + " | " + Format(properties));
#endif
    }

    private string Format(Dictionary<string, object> properties)
    {
        if (properties == null || properties.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder();
        bool first = true;
        foreach (KeyValuePair<string, object> property in properties)
        {
            if (!first)
            {
                builder.Append(", ");
            }

            builder.Append(property.Key);
            builder.Append("=");
            builder.Append(property.Value);
            first = false;
        }

        return builder.ToString();
    }
}
