using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommSetting : MonoBehaviour
{
	public Vector2 Resolution;
	public float TimeScale = 1;
    // Start is called before the first frame update
    void Start()
    {       
        Screen.SetResolution((int)Resolution.x, (int)Resolution.y, true);
		#if UNITY_EDITOR

		#else
        Cursor.visible = false;
	#endif
    }

    // Update is called once per frame
    void Update()
    {
		Time.timeScale = TimeScale;
		
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
		
        if(Input.GetKeyDown(KeyCode.M))
        {
            if(Cursor.visible)
            {
                Cursor.visible = false;
            }
            else
            {
                Cursor.visible = true;
            }
        }		
    }
}
