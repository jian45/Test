using System.Collections.Generic;

/// <summary>埋点上报接口：可替换实现（Debug.Log / 微信埋点 / 第三方 SDK）</summary>
public interface ITrackProvider
{
    void Track(string eventName, Dictionary<string, object> properties);
}
