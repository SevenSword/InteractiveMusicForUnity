using System;
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

        private bool isPlaying = false;
        private bool isDisposed = false;

        public void Dispose()
        {
            this.Stop();
            this.isDisposed = true;
        }

        public void Play()
        {
            this.CheckRegisterSource();

            this.isPlaying = true;

            this.CheckLoop();
        }

        public void Stop()
        {
            this.isPlaying = false;
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
    }
}
