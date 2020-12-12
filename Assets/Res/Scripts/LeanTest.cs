using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LeanTest : MonoBehaviour
{

    public ScrollCircle scrollCircle;
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 movement = scrollCircle.content.anchoredPosition;

        if(movement.magnitude > 0.2f)
        {
            transform.Translate(movement * speed * Time.deltaTime);
        }
    }

    public void OnFingerTap()
    {
        Debug.Log(transform.name);
    }
}
