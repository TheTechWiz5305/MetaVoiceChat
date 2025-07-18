7/18/2025:
- Optional [rnnoise](https://github.com/xiph/rnnoise) with [Vatsal Ambastha's RNNoise4Unity](https://github.com/adrenak/RNNoise4Unity) and [RnnoiseVcInputFilter](rnnoise/RnnoiseVcInputFilter.cs)
- Renamed first and next input and output filters for more clarity
- More documentation
- More example stuff

7/17/2025:
- Example code and screenshots for advanced usage
- 48kHz audio (was 16kHz)
- Improved VcMicAudioInput and VcMic
    - OnActiveDeviceChanged event
    - Automatic microphone reconnection
    - SetSelectedDevice(string device) to control automatic reconnection
- Better namespaces (removed Assets.Metater)
- Microphone devices listener utility
- Warnings for incorrect VC input and output filter usage
- Fixed VcAudioSourceOutput bug (audio loop when no data received instead of clearing)
- Added Mirror server build warning
- More intuitive local echo functionality
- Better documentation
