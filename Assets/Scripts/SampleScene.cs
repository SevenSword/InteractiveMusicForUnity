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

    enum LoopType
    {
        START,
        LEAD,
        BEHIND,
        LATER,
        LETHAL,
        FINAL
    }

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
        InteractiveLoopManager.SuperLoopData[] loop_one = {
            new InteractiveLoopManager.SuperLoopData(0, 14.4f, 27.2f, 0.0f, 0.0f, 0.0f, false),

            new InteractiveLoopManager.SuperLoopData(1, 24.0f, 27.2f, 30.4f, 32.0f, 92.8f, true),
            new InteractiveLoopManager.SuperLoopData(1, 14.4f, 17.6f, 35.2f, 36.8f, 92.8f, true),
            new InteractiveLoopManager.SuperLoopData(1, 17.6f, 20.8f, 40.0f, 41.6f, 92.8f, true),
            new InteractiveLoopManager.SuperLoopData(1, 20.8f, 24.0f, 44.8f, 46.4f, 92.8f, true),

            new InteractiveLoopManager.SuperLoopData(2, 24.0f, 27.2f, 30.4f, 32.0f, 172.8f, true),
            new InteractiveLoopManager.SuperLoopData(2, 14.4f, 17.6f, 35.2f, 36.8f, 172.8f, true),
            new InteractiveLoopManager.SuperLoopData(2, 17.6f, 20.8f, 40.0f, 41.6f, 172.8f, true),
            new InteractiveLoopManager.SuperLoopData(2, 20.8f, 24.0f, 44.8f, 46.4f, 172.8f, true),

            new InteractiveLoopManager.SuperLoopData(3, 24.0f, 27.2f, 30.4f, 32.0f, 251.2f, true),
            new InteractiveLoopManager.SuperLoopData(3, 14.4f, 17.6f, 35.2f, 36.8f, 251.2f, true),
            new InteractiveLoopManager.SuperLoopData(3, 17.6f, 20.8f, 40.0f, 41.6f, 251.2f, true),
            new InteractiveLoopManager.SuperLoopData(3, 20.8f, 24.0f, 44.8f, 46.4f, 251.2f, true),

            new InteractiveLoopManager.SuperLoopData(4, 24.0f, 27.2f, 30.4f, 32.0f, 331.2f, true),
            new InteractiveLoopManager.SuperLoopData(4, 14.4f, 17.6f, 35.2f, 36.8f, 331.2f, true),
            new InteractiveLoopManager.SuperLoopData(4, 17.6f, 20.8f, 40.0f, 41.6f, 331.2f, true),
            new InteractiveLoopManager.SuperLoopData(4, 20.8f, 24.0f, 44.8f, 46.4f, 331.2f, true),
        };

        InteractiveLoopManager.SuperLoopData[] loop_two = {
            new InteractiveLoopManager.SuperLoopData(1, 94.4f, 107.2f, 0.0f, 0.0f, 0.0f, false),
        };

        InteractiveLoopManager.SuperLoopData[] loop_three = {
            new InteractiveLoopManager.SuperLoopData(2, 174.4f, 187.2f, 0.0f, 0.0f, 0.0f, false),
        };

        InteractiveLoopManager.SuperLoopData[] loop_four = {
            new InteractiveLoopManager.SuperLoopData(3, 252.8f, 294.4f, 0.0f, 0.0f, 0.0f, false),
        };

        InteractiveLoopManager.SuperLoopData[] loop_five = {
            new InteractiveLoopManager.SuperLoopData(4, 332.8f, 345.6f, 0.0f, 0.0f, 0.0f, false),
        };

        InteractiveLoopManager.SuperLoopData[] loop_six = {
            new InteractiveLoopManager.SuperLoopData(5, 380.8f, 419.2f, 0.0f, 0.0f, 0.0f, false),
        };

        InteractiveLoopManager.Instance.SetupSuperLoopData(new InteractiveLoopManager.SuperLoopData[][]{
            loop_one,
            loop_two,
            loop_three,
            loop_four,
            loop_five,
            loop_six
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
