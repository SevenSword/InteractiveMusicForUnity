using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace InteractiveMusic
{
    public sealed class InteractiveLoopManager : IDisposable
    {
        public static readonly int NormalLoop = -1;
        public static InteractiveLoopManager Instance { get; } = new InteractiveLoopManager();

        private InteractiveLoopManager()
        {
            // nop
        }

        public AudioSource Source { private get; set; }

        public float NowTime { get { return this.Source.time; } }

        private bool isPlaying = false;
        private bool isDisposed = false;
        private List<LoopData> loopDataList = new List<LoopData>();
        private int nextLoopId = -1;
        private int currentLoopId = -1;
        private LoopData? currentLoopData = null;

        private List<SuperLoopData[]> SuperLoopDataList = new List<SuperLoopData[]>();

        private bool isSuper = true;

        /// <summary>
        /// 遷移中フラグ
        /// </summary>
        private bool duringTrans = false;

        /// <summary>
        /// 遷移パート中かどうか
        /// </summary>
        private bool isTransPart = false;

        private SuperLoopData? tempSuperLoopData = null;
        private SuperLoopData normalLoopData;
        private List<SuperLoopData> normalLoopList = new List<SuperLoopData>();

        public void Dispose()
        {
            this.Stop();
            this.isDisposed = true;
        }

        public void Play()
        {
            this.CheckRegisterSource();

            bool beforeIsPlaying = this.isPlaying;

            this.isPlaying = true;

            if (!beforeIsPlaying)
            {
                if (isSuper)
                {
                    this.currentLoopId = 0;
                    this.nextLoopId = 0;
                }
                else
                {
                    if (!this.currentLoopData.HasValue)
                    {
                        this.currentLoopData = new LoopData(0.0f, this.Source.clip.length);
                    }
                }

                this.CheckLoop();
            }
        }

        public void Stop()
        {
            this.isPlaying = false;
        }

        public void SetupLoopData(LoopData[] loopDataArray)
        {
            this.loopDataList = new List<LoopData>(loopDataArray);
            this.isSuper = false;
        }

        public void SetupSuperLoopData(SuperLoopData[][] superLoopDataArray)
        {
            this.SuperLoopDataList.Clear();
            foreach (var array in superLoopDataArray)
            {
                this.SuperLoopDataList.Add(array);
            }
            this.normalLoopData = this.SuperLoopDataList[0][0];
            this.normalLoopList = this.SuperLoopDataList.SelectMany(v => v)
                                                        .Where(v => !v.hasTransPart)
                                                        .ToList();
            Debug.Log(this.normalLoopList.Count());
            this.isSuper = true;
        }

        public void ChangeLoop(int loopId)
        {
            Assert.IsTrue(loopId >= -1);
            if (!isSuper)
            {
                Assert.IsTrue(loopId <= this.loopDataList.Count);
            }

            this.nextLoopId = loopId;
        }

        private async Task CheckLoop()
        {
            while (this.isPlaying)
            {
                this.CheckRegisterSource();

                if (!this.Source.isPlaying)
                {
                    this.Source.Play();
                }

                if (this.isSuper)
                {
                    // 遷移中でも遷移パート中でも
                    if (!this.duringTrans && !this.isTransPart)
                    {
                        // 遷移処理
                        if (this.currentLoopId != this.nextLoopId)
                        {
                            Debug.LogFormat("Change: {0} -> {1}", this.currentLoopId, this.nextLoopId);
                            Debug.LogFormat("NowTime: {0}", this.Source.time);
                            var superLoopDatas = this.SuperLoopDataList[this.currentLoopId].Where(x => x.NextLoopId == this.nextLoopId)
                                                                                           .Where(x => x.StartTimeFromLoop <= this.Source.time && this.Source.time <= x.EndTimeFromLoop);
                            Assert.IsTrue(superLoopDatas.Any());
                            Assert.IsFalse(superLoopDatas.Count() != 1, superLoopDatas.Count().ToString());
                            var superLoopData = superLoopDatas.FirstOrDefault();
                            this.tempSuperLoopData = superLoopData;
                            this.duringTrans = true;
                            this.currentLoopId = this.nextLoopId;
                        }
                        else
                        {
                            // 通常ループ
                            if (this.Source.time >= this.normalLoopData.EndTimeFromLoop)
                            {
                                this.Source.time = this.normalLoopData.StartTimeFromLoop;
                            }
                        }
                    }

                    // 遷移中（遷移パートへの遷移待ち）
                    if (this.duringTrans)
                    {
                        Assert.IsTrue(this.tempSuperLoopData.HasValue);
                        if (this.Source.time >= this.tempSuperLoopData.Value.EndTimeFromLoop)
                        {
                            this.duringTrans = false;
                            this.isTransPart = true;
                            this.Source.time = this.tempSuperLoopData.Value.StartTimeAtTarnsPart;
                        }
                    }

                    // 遷移パート中（遷移パートの完了待ち）
                    if (this.isTransPart)
                    {
                        Assert.IsTrue(this.tempSuperLoopData.HasValue);
                        if (this.Source.time >= this.tempSuperLoopData.Value.EndTimeAtTransPart)
                        {
                            this.isTransPart = false;
                            this.normalLoopData = this.normalLoopList[this.currentLoopId];
                            this.Source.time = this.tempSuperLoopData.Value.DestinationTime;
                            Debug.LogFormat("STFL: {0} ETFL: {1}", this.normalLoopData.StartTimeFromLoop, this.normalLoopData.EndTimeFromLoop);
                        }
                    }
                }
                else
                {
                    if (this.currentLoopId != this.nextLoopId)
                    {
                        Debug.LogFormat("Change: {0} -> {1}", this.currentLoopId, this.nextLoopId);
                        if (this.nextLoopId == NormalLoop)
                        {
                            this.currentLoopId = NormalLoop;
                            this.currentLoopData = new LoopData(0.0f, this.Source.clip.length);
                        }
                        else
                        {
                            var buffer = this.Source.time - this.currentLoopData.Value.StartTime;

                            this.currentLoopData = this.loopDataList[this.nextLoopId];
                            this.Source.time = this.currentLoopData.Value.StartTime + buffer;

                            this.currentLoopId = this.nextLoopId;
                        }
                    }

                    if (this.currentLoopData.HasValue)
                    {
                        if (this.Source.time >= this.currentLoopData.Value.EndTime)
                        {
                            this.Source.time = this.currentLoopData.Value.StartTime;
                        }
                    }
                }

                await Task.Yield();
            }

            this.Source.Stop();
            if (this.isDisposed)
            {
                this.Source = null;
            }
        }

        private void CheckRegisterSource()
        {
            Assert.IsNotNull(this.Source);
        }

        public struct LoopData
        {
            public float StartTime;
            public float EndTime;

            public LoopData(float st, float et)
            {
                this.StartTime = st;
                this.EndTime = et;
            }
        }

        public struct SuperLoopData
        {
            public int NextLoopId;
            public float StartTimeFromLoop;
            public float EndTimeFromLoop;
            public float StartTimeAtTarnsPart;
            public float EndTimeAtTransPart;
            public float DestinationTime;
            public bool hasTransPart;

            public SuperLoopData(int nli, float stfl, float etfl, float statp, float etatp, float dt, bool htp)
            {
                this.NextLoopId = nli;
                this.StartTimeFromLoop = stfl;
                this.EndTimeFromLoop = etfl;
                this.StartTimeAtTarnsPart = statp;
                this.EndTimeAtTransPart = etatp;
                this.DestinationTime = dt;
                this.hasTransPart = htp;
            }
        }
    }
}
