using InteractiveMusic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleScene : MonoBehaviour {

    [SerializeField]
    private AudioSource audioSource;

    enum LoopType
    {
        AllLoop,
        Intro1,
        Intro2,
        Verse1_1,
        Verse1_2,
        Verse2_1,
        Verse2_2,
        Chorus
    }

    public void Start()
    {
        InteractiveLoopManager.Instance.Source = this.audioSource;
    }

    public void OnClick(int id)
    {
        var type = (LoopType)id;
        Debug.LogFormat("LoopType: {0}", type);

        if (type == LoopType.AllLoop)
        {

        }
        else
        {
        }

        InteractiveLoopManager.Instance.Play();
    }
}
