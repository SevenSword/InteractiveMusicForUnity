using InteractiveMusic;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class SampleScene : MonoBehaviour {

    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private Text time;

    public void Update()
    {
        this.time.text = InteractiveLoopManager.Instance.NowTime.ToString();
    }

    public void Start()
    {
        InteractiveLoopManager.Instance.Source = this.audioSource;

        // ループデータ設定
        // InteractiveLoopManager.Instance.SetupLoopData(new [] {
        //     new InteractiveLoopManager.LoopData(0.0f, 14.45f),
        //     new InteractiveLoopManager.LoopData(14.45f, 29.073f),
        //     new InteractiveLoopManager.LoopData(29.073f, 43.524f),
        //     new InteractiveLoopManager.LoopData(43.524f, 58.1476f),
        //     new InteractiveLoopManager.LoopData(58.1476f, 72.4046f),
        //     new InteractiveLoopManager.LoopData(72.6046f, 87.2216f),
        //     new InteractiveLoopManager.LoopData(87.2216f, 101.691f)
        // });
        // スーパーループデータ設定

        InteractiveLoopManager.Instance.SetupSuperLoopData(SampleLoopData.SuperSampleOne());
    }

    public void OnDestroy()
    {
        InteractiveLoopManager.Instance.Dispose();
    }

    public void OnClick(int id)
    {
        // var type = (LoopType)id;
        // Debug.LogFormat("LoopType: {0}", type);

        // InteractiveLoopManager.Instance.ChangeLoop(id);

        // InteractiveLoopManager.Instance.Play();
    }

    public void OnChangeBGM(int id)
    {

    }

    public void OnChangeLoop(int id)
    {
        if (id == 0)
        {
            InteractiveLoopManager.Instance.Play();
        }
        else
        {
            InteractiveLoopManager.Instance.ChangeLoop(id);
        }
    }
}
