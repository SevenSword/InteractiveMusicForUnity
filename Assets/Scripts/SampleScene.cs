using InteractiveMusic;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class SampleScene : MonoBehaviour {

    [SerializeField]
    private AudioSource[] audioSource;

    [SerializeField]
    private Text time;

    [SerializeField]
    private Text status;

    public void Awake()
    {
        // 30fps固定でもループ問題ないか検証する目的
        Application.targetFrameRate = 30;
    }

    public void Update()
    {
        this.time.text = InteractiveLoopManager.Instance.NowTime.ToString();

        bool duringTrans, isTransPart;
        InteractiveLoopManager.SuperLoopData superLoopData;
        if (InteractiveLoopManager.Instance.GetStatus(out duringTrans, out isTransPart, out superLoopData))
        {
            string formatText = "{0}:\n{1}";
            string text = string.Empty;

            if (!duringTrans && !isTransPart)
            {
                text = string.Format(formatText, "ループパート", superLoopData.ToString());
            }
            else
            {
                if (duringTrans)
                {
                    text = string.Format(formatText, "ループパート;遷移待機中", superLoopData.ToString());
                }
                if (isTransPart)
                {
                    text = string.Format(formatText, "遷移パート", superLoopData.ToString());
                }
            }

            this.status.text = text;
        }
        else
        {
            this.status.text = "停止中";
        }
    }

    public void OnDestroy()
    {
        InteractiveLoopManager.Instance.Dispose();
    }

    public void OnChangeBGM(int id)
    {
        InteractiveLoopManager.Instance.Source = this.audioSource[id];
        Debug.Log(this.audioSource[id].clip.length);
        InteractiveLoopManager.Instance.SetupSuperLoopData(SampleLoopData.SuperSample(id));
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

    public void OnStop()
    {
        InteractiveLoopManager.Instance.Stop();
    }
}
