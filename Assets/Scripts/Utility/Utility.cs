using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static string GetLanguage(int id, ELanguage language = ELanguage.CN)
    {
        if (Language.list.TryGetValue(id, out var cfg))
        {
            switch (language)
            {
                case ELanguage.CN:
                    return cfg.cn;
                default:
                    Debug.LogError($"[GetLanguage] 未定义的语言类型 [{language}]");
                    return string.Empty;
            }
        }
        else
        {
            Debug.LogError($"[GetLanguage] 不存在的语言ID=[{id}]");
        }
        
        return string.Empty;
    }
}
