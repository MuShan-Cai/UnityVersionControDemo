using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecodeMusic : MonoBehaviour
{
    public AudioClip Music;
    private AudioSource audioPlayer;
    public bool isLoop;
    public float[] Samples;


    // Start is called before the first frame update
    void Start()
    {
        if(audioPlayer == null)
        {
            audioPlayer = this.gameObject.AddComponent<AudioSource>();
            audioPlayer.clip = Music;
            audioPlayer.Play();
            if(isLoop)
            {
                audioPlayer.loop = true;
            }
            return;
        }
        audioPlayer = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        audioPlayer.GetSpectrumData(Samples, 0, FFTWindow.BlackmanHarris);
    }

    public int GetSamplesCount()
    {
        return Samples.Length;
    }

    public float GetSample(int num)
    {
        return Samples[num];
    }
}
