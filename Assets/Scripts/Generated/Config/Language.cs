using System;
using System.Collections.Generic;
public class Class_Language
{
    /// <summary>
    /// id
    /// </summary>
    public double id;
    /// <summary>
    /// CN文本
    /// </summary>
    public string cn;
}

public class Language
{
    public static  Dictionary<double,Class_Language> list = new Dictionary<double, Class_Language>();
    static void Language0()
    {
        Class_Language tem2 = new Class_Language();
        tem2.id = 10001;
        tem2.cn = "《猫咖经营》";
        list.Add(tem2.id,tem2);

    }
    static Language()
    {
        Language0();
    }
}
