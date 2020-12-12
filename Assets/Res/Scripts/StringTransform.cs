using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringTransform : MonoBehaviour
{
    public string text;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Debug.Log(ByteToString(StringToByte(text)));
        }
    }

    byte[] StringToByte(string str)
    {
        byte[] byteArray = System.Text.Encoding.Default.GetBytes(str);
        return byteArray;
    }

    string ByteToString(byte[] byteArray)
    {
        string str = System.Text.Encoding.Default.GetString(byteArray);
        return str;
    }

}
