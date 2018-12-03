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
        public static InteractiveLoopManager Instance { get; } = new InteractiveLoopManager();

        private InteractiveLoopManager()
        {
            // nop
        }

        public AudioSource Source { private get; set; }

        public float NowTime
        {
            get
            {
                if (this.Source != null)
                {
                    return this.Source.time;
                }
                else
                {
                    return 0.0f;
                }
            }
        }

        private bool isPlaying = false;
        private int nextLoopId = -1;
        private int currentLoopId = -1;

        private List<SuperLoopData[]> SuperLoopDataList = new List<SuperLoopData[]>();

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
        private bool needOneLoop = false;

        public void Dispose()
        {
            this.Stop();
        }

        public void Play()
        {
            this.CheckRegisterSource();

            bool beforeIsPlaying = this.isPlaying;

            this.isPlaying = true;

            if (!beforeIsPlaying)
            {
                this.currentLoopId = 0;
                this.nextLoopId = 0;

                this.CheckLoop();
            }
        }

        public void Stop()
        {
            this.isPlaying = false;
            this.currentLoopId = 0;
            this.nextLoopId = 0;

            if (this.Source != null)
            {
                this.Source.Stop();
                this.Source.time = 0.0f;
            }
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
            Debug.LogFormat("STFL: {0} ETFL: {1}", this.normalLoopData.StartTimeFromLoop, this.normalLoopData.EndTimeFromLoop);
        }

        public void ChangeLoop(int loopId)
        {
            Assert.IsTrue(loopId >= -1);
            if (!this.duringTrans && !this.isTransPart)
            {
                this.nextLoopId = loopId;
            }
            else
            {
                Debug.LogWarning("Duraing!!");
            }
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
                        if (superLoopDatas.Any())
                        {
                            Assert.IsFalse(superLoopDatas.Count() != 1, superLoopDatas.Count().ToString());
                            var superLoopData = superLoopDatas.FirstOrDefault();
                            this.tempSuperLoopData = superLoopData;
                            this.duringTrans = true;
                            this.currentLoopId = this.nextLoopId;
                        }
                        else
                        {
                            // 見つからなかったので、データの抜け。その場合は近傍捜索
                            // WIP: STFL〜ETFLのどちらに近いかを見て、近い方の値を利用→データの近傍を取得していないため抜けを参照する可能性がまだある
                            float vicinityTime1 = 0.0f;
                            float vicinityTime2 = 0.0f;
                            if ((this.normalLoopData.EndTimeFromLoop - this.normalLoopData.StartTimeFromLoop / 2) - this.Source.time >= 0.0f)
                            {
                                vicinityTime1 = this.normalLoopData.StartTimeFromLoop;
                                vicinityTime2 = this.normalLoopData.EndTimeFromLoop;
                            }
                            else
                            {
                                vicinityTime1 = this.normalLoopData.EndTimeFromLoop;
                                vicinityTime2 = this.normalLoopData.StartTimeFromLoop;
                                this.needOneLoop = true;
                            }

                            IEnumerable<SuperLoopData> vicinityLoopDatas = this.SuperLoopDataList[this.currentLoopId].Where(x => x.NextLoopId == this.nextLoopId)
                                                                                                                     .Where(x => x.StartTimeFromLoop <= vicinityTime1 && vicinityTime1 <= x.EndTimeFromLoop);
                            if (vicinityLoopDatas.Any())
                            {
                                Assert.IsFalse(vicinityLoopDatas.Count() != 1, vicinityLoopDatas.Count().ToString());
                                var superLoopData = vicinityLoopDatas.FirstOrDefault();
                                this.tempSuperLoopData = superLoopData;
                                this.needOneLoop = false;
                                this.duringTrans = true;
                                this.currentLoopId = this.nextLoopId;
                            }
                            else
                            {
                                // 第一近傍に見つからなかったので、第二
                                vicinityLoopDatas = this.SuperLoopDataList[this.currentLoopId].Where(x => x.NextLoopId == this.nextLoopId)
                                                                                              .Where(x => x.StartTimeFromLoop <= vicinityTime2 && vicinityTime2 <= x.EndTimeFromLoop);
                                if (vicinityLoopDatas.Any())
                                {
                                    Assert.IsFalse(vicinityLoopDatas.Count() != 1, vicinityLoopDatas.Count().ToString());
                                    var superLoopData = vicinityLoopDatas.FirstOrDefault();
                                    this.tempSuperLoopData = superLoopData;
                                    this.duringTrans = true;
                                    this.currentLoopId = this.nextLoopId;
                                }
                                else
                                {
                                    // 近傍データ見つからず
                                    Debug.LogWarning("NotFound LoopData!!");
                                    this.nextLoopId = this.currentLoopId;
                                }
                            }
                        }
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
                        if (!this.needOneLoop)
                        {
                            this.duringTrans = false;
                            this.isTransPart = true;
                            this.Source.time = this.tempSuperLoopData.Value.StartTimeAtTarnsPart;
                        }
                        else
                        {
                            if (this.Source.time >= this.normalLoopData.EndTimeFromLoop)
                            {
                                this.Source.time = this.normalLoopData.StartTimeFromLoop;
                                this.needOneLoop = false;
                            }
                        }
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

                await Task.Yield();
            }
        }

        private void CheckRegisterSource()
        {
            Assert.IsNotNull(this.Source);
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
