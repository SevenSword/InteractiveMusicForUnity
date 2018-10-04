using InteractiveMusic;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SampleScene : MonoBehaviour {

    [SerializeField]
    private AudioSource audioSource;

    enum LoopType
    {
        AllLoop = -1,
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

        // ループデータ設定
        InteractiveLoopManager.Instance.SetupLoopData(new [] {
            new InteractiveLoopManager.LoopData(0.0f, 14.45f),
            new InteractiveLoopManager.LoopData(14.45f, 29.073f),
            new InteractiveLoopManager.LoopData(29.073f, 43.524f),
            new InteractiveLoopManager.LoopData(43.524f, 58.1476f),
            new InteractiveLoopManager.LoopData(58.1476f, 72.4046f),
            new InteractiveLoopManager.LoopData(72.6046f, 87.2216f),
            new InteractiveLoopManager.LoopData(87.2216f, 101.691f)
        });
    }

    public void OnDestroy()
    {
        InteractiveLoopManager.Instance.Dispose();
    }

    public void OnClick(int id)
    {
        var type = (LoopType)id;
        Debug.LogFormat("LoopType: {0}", type);

        InteractiveLoopManager.Instance.ChangeLoop(id);

        InteractiveLoopManager.Instance.Play();
    }
}
