using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Controller : MonoBehaviour
{
    public float Step = 10;
    public float LocalStep = 1;
    public Color HColor;
    public float HColorStep;
    public Color MColor;
    public Color LColor;
    public float LColorStep;
    public Image[] img;
    public RectTransform[] rectTransforms;
    private DecodeMusic dMusic;
    bool isProcessEnd = false;
    // Start is called before the first frame update
    void Start()
    {
        img = this.GetComponentsInChildren<Image>();
        rectTransforms = this.GetComponentsInChildren<RectTransform>();
        dMusic = FindObjectOfType<DecodeMusic>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!isProcessEnd)
        {
            StartCoroutine("Process");
        }
    }

    IEnumerator Process()
    {
        float imgy = rectTransforms[0].rect.height;
        for (int i = 0; i < img.Length; i++)
        {
            if (dMusic.GetSample(i) * 10 >= HColorStep)
                img[i].color = HColor;
            if (dMusic.GetSample(i) * 10 >= LColorStep && dMusic.GetSample(i) * 10 < HColorStep)
                img[i].color = MColor;
            if (dMusic.GetSample(i) < LColorStep)
                img[i].color = LColor;
            img[i].transform.localScale = new Vector3(img[i].transform.localScale.x, LocalStep + dMusic.GetSample(i) * Step, img[i].transform.localScale.x);
        }
        isProcessEnd = true;
        yield return new WaitForSeconds(Time.deltaTime * 2);
        isProcessEnd = false;
    }

}
