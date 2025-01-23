// Influenced by: https://github.com/adrenak/univoice-unimic-input/blob/master/Assets/Adrenak.UniVoice.UniMicInput/Runtime/UniVoiceUniMicInput.cs

namespace Assets.Metater.MetaVoiceChat.Input.Mic
{
    public class VcMicAudioInput : VcAudioInput
    {
        public VcMic Mic { get; private set; } = null;

        public override void StartLocalPlayer()
        {
            int samplesPerFrame = metaVc.config.samplesPerFrame;

            Mic = new VcMic(this, samplesPerFrame);
            Mic.OnFrameReady += SendAndFilterFrame;
            Mic.StartRecording();
        }

        private void OnDestroy()
        {
            if (Mic == null)
            {
                return;
            }

            Mic.OnFrameReady -= SendAndFilterFrame;
            Mic.Dispose();
        }
    }
}
