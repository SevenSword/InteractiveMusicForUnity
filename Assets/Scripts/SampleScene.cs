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

    public void Update()
    {
        this.time.text = InteractiveLoopManager.Instance.NowTime.ToString();
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
