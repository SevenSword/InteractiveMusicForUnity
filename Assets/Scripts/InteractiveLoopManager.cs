using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace InteractiveMusic
{
    /// <summary>
    /// インタラクティブルームマネージャー
    /// </summary>
    public sealed class InteractiveLoopManager : IDisposable
    {
        /// <summary>
        /// 再生中フラグ
        /// </summary>
        private bool isPlaying = false;

        /// <summary>
        /// 次のループID
        /// </summary>
        private int nextLoopId = -1;

        /// <summary>
        /// 現在のループID
        /// </summary>
        private int currentLoopId = -1;

        /// <summary>
        /// ループデータの二次元配列
        /// </summary>
        /// <typeparam name="SuperLoopData[]">ループデータの配列</typeparam>
        private List<SuperLoopData[]> superLoopDataList = new List<SuperLoopData[]>();

        /// <summary>
        /// 遷移中フラグ
        /// </summary>
        private bool duringTrans = false;

        /// <summary>
        /// 遷移パート中かどうか
        /// </summary>
        private bool isTransPart = false;

        /// <summary>
        /// 遷移に利用しているループデータの一時保存
        /// </summary>
        private SuperLoopData? tempSuperLoopData = null;

        /// <summary>
        /// 通常ループ用データ
        /// </summary>
        private SuperLoopData normalLoopData;

        /// <summary>
        /// 通常ループ用データリスト
        /// </summary>
        /// <typeparam name="SuperLoopData">ループデータ</typeparam>
        private List<SuperLoopData> normalLoopList = new List<SuperLoopData>();

        /// <summary>
        /// 1ループ必要かどうか
        /// </summary>
        private bool needOneLoop = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private InteractiveLoopManager()
        {
            // nop
        }

        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static InteractiveLoopManager Instance { get; } = new InteractiveLoopManager();

        /// <summary>
        /// オーディオソース
        /// </summary>
        public AudioSource Source { private get; set; }

        /// <summary>
        /// 現在の再生時間
        /// </summary>
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

        /// <summary>
        /// デストラクタ
        /// </summary>
        public void Dispose()
        {
            this.Stop();
        }

        /// <summary>
        /// 状態を取得
        /// </summary>
        /// <param name="duringTrans">遷移中かどうか</param>
        /// <param name="isTransPart">遷移パートかどうか</param>
        /// <param name="nowLoopData">現在のループデータ</param>
        /// <returns>再生中かどうか</returns>
        public bool GetStatus(out bool duringTrans, out bool isTransPart, out SuperLoopData nowLoopData)
        {
            if (!this.isPlaying)
            {
                duringTrans = false;
                isTransPart = false;
                nowLoopData = default(SuperLoopData);
                return false;
            }
            else
            {
                duringTrans = this.duringTrans;
                isTransPart = this.isTransPart;
                if (!this.duringTrans && !isTransPart)
                {
                    nowLoopData = this.normalLoopData;
                }
                else
                {
                    if (this.tempSuperLoopData.HasValue)
                    {
                        nowLoopData = this.tempSuperLoopData.Value;
                    }
                    else
                    {
                        nowLoopData = this.normalLoopData;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// 再生
        /// </summary>
        public void Play()
        {
            this.CheckRegisterSource();

            bool beforeIsPlaying = this.isPlaying;

            this.isPlaying = true;

            if (!beforeIsPlaying)
            {
                this.ResetParam();
                this.normalLoopData = this.normalLoopList[0];

                this.CheckLoop().ContinueWith(_ => { });
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <remarks>
        /// 再生位置など全てリセットされます。
        /// </remarks>
        public void Stop()
        {
            // 再生フラグを下ろす
            this.isPlaying = false;

            // ソース停止処理
            if (this.Source != null)
            {
                this.Source.Stop();
                this.Source.time = 0.0f;
            }

            this.ResetParam();
        }

        /// <summary>
        /// ループデータを設定
        /// </summary>
        /// <param name="superLoopDataArray">ループデータの二次元配列</param>
        public void SetupSuperLoopData(SuperLoopData[][] superLoopDataArray)
        {
            this.superLoopDataList.Clear();
            foreach (var array in superLoopDataArray)
            {
                this.superLoopDataList.Add(array);
            }

            this.normalLoopData = this.superLoopDataList[0][0];
            this.normalLoopList = this.superLoopDataList.SelectMany(v => v)
                                                        .Where(v => !v.HasTransPart)
                                                        .ToList();
            Debug.Log(this.normalLoopList.Count());
            Debug.LogFormat("STFL: {0} ETFL: {1}", this.normalLoopData.StartTimeFromLoop, this.normalLoopData.EndTimeFromLoop);
        }

        /// <summary>
        /// ループ切り替え
        /// </summary>
        /// <param name="loopId">切り替え先のループID</param>
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

        /// <summary>
        /// ループチェック処理
        /// </summary>
        /// <returns>処理</returns>
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
                        var superLoopDatas = this.superLoopDataList[this.currentLoopId].Where(x => x.NextLoopId == this.nextLoopId)
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
                            if ((this.normalLoopData.EndTimeFromLoop - (this.normalLoopData.StartTimeFromLoop / 2)) - this.Source.time >= 0.0f)
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

                            IEnumerable<SuperLoopData> vicinityLoopDatas = this.superLoopDataList[this.currentLoopId].Where(x => x.NextLoopId == this.nextLoopId)
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
                                vicinityLoopDatas = this.superLoopDataList[this.currentLoopId].Where(x => x.NextLoopId == this.nextLoopId)
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
                        this.tempSuperLoopData = null;
                        Debug.LogFormat("STFL: {0} ETFL: {1}", this.normalLoopData.StartTimeFromLoop, this.normalLoopData.EndTimeFromLoop);
                    }
                }

                await Task.Yield();
            }
        }

        /// <summary>
        /// オーディオソースヌルチェック
        /// </summary>
        private void CheckRegisterSource()
        {
            Assert.IsNotNull(this.Source);
        }

        /// <summary>
        /// パラメーターリセット
        /// </summary>
        private void ResetParam()
        {
            // 各パラメーターリセット
            this.currentLoopId = 0;
            this.nextLoopId = 0;
            this.needOneLoop = false;
            this.duringTrans = false;
            this.isTransPart = false;
        }

        /// <summary>
        /// ループデータ
        /// </summary>
        public struct SuperLoopData
        {
            /// <summary>
            /// 次のループID
            /// </summary>
            public int NextLoopId;

            /// <summary>
            /// ループ開始時間（STFL）
            /// </summary>
            public float StartTimeFromLoop;

            /// <summary>
            /// ループ終了時間（ETFL）
            /// </summary>
            public float EndTimeFromLoop;

            /// <summary>
            /// 遷移パート開始時間（STATP）
            /// </summary>
            public float StartTimeAtTarnsPart;

            /// <summary>
            /// 遷移パート終了時間（ETATP）
            /// </summary>
            public float EndTimeAtTransPart;

            /// <summary>
            /// 遷移先時間
            /// </summary>
            public float DestinationTime;

            /// <summary>
            /// 遷移パートがあるかどうか
            /// </summary>
            public bool HasTransPart;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="nli">次のループID</param>
            /// <param name="stfl">STFL</param>
            /// <param name="etfl">ETFL</param>
            /// <param name="statp">STATP</param>
            /// <param name="etatp">ETATP</param>
            /// <param name="dt">遷移先時間</param>
            /// <param name="htp">遷移パートがあるかどうか</param>
            public SuperLoopData(int nli, float stfl, float etfl, float statp, float etatp, float dt, bool htp)
            {
                this.NextLoopId = nli;
                this.StartTimeFromLoop = stfl;
                this.EndTimeFromLoop = etfl;
                this.StartTimeAtTarnsPart = statp;
                this.EndTimeAtTransPart = etatp;
                this.DestinationTime = dt;
                this.HasTransPart = htp;
            }

            /// <summary>
            /// 文字列化
            /// </summary>
            /// <returns>文字列</returns>
            public override string ToString()
            {
                if (!HasTransPart)
                {
                    return string.Format(
                        "ループ開始: {0} 〜 ループ終了: {1}",
                        this.StartTimeFromLoop,
                        this.EndTimeFromLoop);
                }
                else
                {
                    return string.Format(
                        "ここから: {0} ここまで: {1} ならば {1} になったら\n {2} 〜 {3} を再生して {4} へ",
                        this.StartTimeFromLoop,
                        this.EndTimeFromLoop,
                        this.StartTimeAtTarnsPart,
                        this.EndTimeAtTransPart,
                        this.DestinationTime);
                }
            }
        }
    }
}
