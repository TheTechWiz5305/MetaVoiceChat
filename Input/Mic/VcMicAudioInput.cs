using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Metater.MetaVoiceChat.Input.Mic
{
    public class VcMicAudioInput : VcAudioInput
    {
        public event Action<string> OnActiveDeviceChanged;

        public string ActiveDevice => mic?.ActiveDevice ?? null;

        private VcMic mic;

        public bool IsInitialized => mic != null;

        public override void StartLocalPlayer()
        {
            int samplesPerFrame = metaVc.config.samplesPerFrame;

            mic = new(this, samplesPerFrame);
            mic.OnFrameReady += SendAndFilterFrame;
            mic.OnActiveDeviceChanged += Mic_OnActiveDeviceChanged;

            if (mic.Devices.Length > 0)
            {
                mic.StartRecording();
            }

            StartCoroutine(CoReconnect());
        }

        private void OnDestroy()
        {
            if (mic == null)
            {
                return;
            }

            mic.OnFrameReady -= SendAndFilterFrame;
            mic.OnActiveDeviceChanged -= Mic_OnActiveDeviceChanged;
            mic.Dispose();
            mic = null;

            StopAllCoroutines();
        }

        private IEnumerator CoReconnect()
        {
            yield return new WaitForSecondsRealtime(1f);

            while (mic != null)
            {
                while (!ShouldReconnect())
                {
                    yield return null;
                }

                if (mic == null)
                {
                    yield break;
                }

                mic.StopRecording();

                yield return null;
                yield return null;

                if (mic == null)
                {
                    yield break;
                }

                if (mic.Devices.Length > 0)
                {
                    mic.StartRecording();
                }
                else
                {
                    yield return new WaitForSecondsRealtime(1f);
                }

                yield return null;
                yield return null;
            }
        }

        private void Mic_OnActiveDeviceChanged(string device)
        {
            OnActiveDeviceChanged?.Invoke(device);
        }

        public void SetSelectedDevice(string device)
        {
            if (mic == null)
            {
                return;
            }

            mic.SetSelectedDevice(device);
        }

        private bool ShouldReconnect()
        {
            if (mic == null)
            {
                return true;
            }

            if (!mic.IsRecording || !mic.Devices.Contains(mic.ActiveDevice))
            {
                return true;
            }

            if (mic.SelectedDevice != mic.ActiveDevice && mic.Devices.Contains(mic.SelectedDevice))
            {
                return true;
            }

            return false;
        }
    }
}