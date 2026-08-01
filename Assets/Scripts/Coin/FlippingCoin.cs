using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class FlippingCoin : MonoBehaviour
{
    public float forceMagnitude = 10f;
    private Rigidbody2D rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        Flipping();
    }
  
    private void OnMouseDown()
    {
        this.gameObject.SetActive(false);
    }
    private void Flipping()
    {
        if (rb == null) return;
       //取一定区域
        float angle = Random.Range(-45f, 45f) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
        dir.y = Mathf.Abs(dir.y);

   //添加一个向上一定区域的力
        rb.AddForce(dir * forceMagnitude, ForceMode2D.Impulse);
    }
}
