using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace InteractiveMusic
{
    public sealed class InteractiveLoopManager
    {
        public static InteractiveLoopManager Instance { get; } = new InteractiveLoopManager();

        private InteractiveLoopManager()
        {
            // nop
        }

        public AudioSource Source { private get; set; }

        public void Play()
        {
            this.CheckRegisterSource();

            this.Source.Play();
        }

        private void CheckRegisterSource()
        {
            Assert.IsNotNull(this.Source);
        }
    }
}
