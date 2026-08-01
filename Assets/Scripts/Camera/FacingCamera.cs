using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacingCamera : MonoBehaviour
{
    Transform[] children;

    void Start()
    {
        children = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i);
        }
    }

    void Update()
    {
        if (Camera.main == null)
        {
            return;
        }

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null)
            {
                children[i].rotation = Camera.main.transform.rotation;
            }
        }
    }
}
