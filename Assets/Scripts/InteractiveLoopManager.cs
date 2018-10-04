using System;
using System.Collections.Generic;
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

        private bool isPlaying = false;
        private bool isDisposed = false;
        private List<LoopData> loopDataList = new List<LoopData>();
        private int nextLoopId = -1;
        private int currentLoopId = -1;
        private LoopData? currentLoopData = null;

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
                if (!this.currentLoopData.HasValue)
                {
                    this.currentLoopData = new LoopData(0.0f, this.Source.clip.length);
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
        }

        public void ChangeLoop(int loopId)
        {
            Assert.IsTrue(loopId >= -1);
            Assert.IsTrue(loopId <= this.loopDataList.Count);

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
    }
}
