using System;
using System.Runtime.InteropServices;
using Assets.Metater.MetaVoiceChat;
using Assets.Metater.MetaVoiceChat.Input;
using Lasp;
using NAudio.Dsp;
using UnityEngine;

// to see usage of WdlResampler
// https://github.com/naudio/NAudio/blob/master/NAudio.Core/Wave/SampleProviders/WdlResamplingSampleProvider.cs

public class LowLatencyAudioInput : VcAudioInput
{
    public AudioSource attackSfx;

    private InputStream inputStream;
    private int channels;
    private WdlResampler resampler;

    private int samplesPerFrame;
    private byte[] frameData;
    private float[] frame;

    private int frameIndex = 0;

    public override void StartLocalPlayer()
    {
        //var inputDevices = Lasp.AudioSystem.InputDevices;
        //foreach (var device in inputDevices)
        //{
        //    print("Input device: " + device.Name + ", ID: " + device.ID);
        //}

        inputStream = Lasp.AudioSystem.GetDefaultInputStream();

        channels = inputStream.ChannelCount;

        print("inputStream created with channel count: " + inputStream.ChannelCount +
              ", sample rate: " + inputStream.SampleRate);

        resampler = new();
        resampler.SetMode(true, 2, false);
        resampler.SetFilterParms();
        resampler.SetFeedMode(false);
        resampler.SetRates(inputStream.SampleRate, VcConfig.SamplesPerSecond);

        samplesPerFrame = metaVc.config.samplesPerFrame;
        frameData = new byte[metaVc.config.samplesPerFrame * channels * sizeof(float)];
        frame = new float[metaVc.config.samplesPerFrame];

        lock (inputStream.DataQueueLock)
        {
            inputStream.DataQueue.Clear();
        }
    }

    private void Update()
    {
        if (inputStream == null)
        {
            return;
        }

        // keep stream awake
        _ = inputStream.InterleavedDataSpan;

        lock (inputStream.DataQueueLock)
        {
            while (inputStream.DataQueue.Count >= frameData.Length)
            {
                for (int i = 0; i < frameData.Length; i++)
                {
                    frameData[i] = inputStream.DataQueue.Dequeue();
                }

                ReadOnlySpan<float> rawData = MemoryMarshal.Cast<byte, float>(new ReadOnlySpan<byte>(frameData, 0, frameData.Length));
                //float[] oneChannel = new float[rawData.Length / channels];
                //for (int i = 0, j = 0; i < rawData.Length; i += channels, j++)
                //{
                //    // skip every other channel
                //    oneChannel[j] = rawData[i];
                //}

                //var data = Resample(oneChannel, inputStream.SampleRate, VcConfig.SamplesPerSecond);
                for (int i = 0, j = 0; i < rawData.Length; i += channels, j++)
                {
                    frame[j] = rawData[i];
                }

                //if (RMS(frame) > 0.1f && !attackSfx.isPlaying)
                //{
                //    attackSfx.Play();
                //}

                SendAndFilterFrame(frameIndex, frame);
                frameIndex++;
            }
        }


        //while (resampledQueue.Count >= samplesPerFrame)
        //{
        //    for (int i = 0; i < samplesPerFrame; i++)
        //    {
        //        // todo assuming two channels
        //        //_ = resampledQueue.Dequeue();
        //        float sample = resampledQueue.Dequeue();

        //        frame[i] = sample;

        //    }

        //    SendAndFilterFrame(frameIndex, frame);
        //    frameIndex++;

        //    //print("RMS: " + GetT(frame));
        //}
    }

    private float RMS(float[] samples)
    {
        if (samples == null || samples.Length == 0)
        {
            return 0f;
        }

        // calculate RMS (Root Mean Square) of the samples
        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i];
        }
        float rms = Mathf.Sqrt(sum / samples.Length);
        return rms;
    }

    //private void OnAudioFrameReceived(MetaAudioFrame frame)
    //{
    //    ReadOnlySpan<float> data = MemoryMarshal.Cast<byte, float>(new ReadOnlySpan<byte>(frame.window, 0, frame.windowSize));

    //    float[] d = new float[data.Length / channels];
    //    for (int i = 0, j = 0; i < data.Length; i += 2, j++)
    //    {
    //        d[j] = data[i];
    //    }

    //    var output = Resample(d, inputStream.SampleRate, VcConfig.SamplesPerSecond);
    //    for (int i = 0; i < output.Length; i++)
    //    {
    //        resampledQueue.Enqueue(output[i]);
    //    }

    //    //// skip every other
    //    //float[] d = new float[data.Length / 2];
    //    //for (int i = 0, j = 0; i < data.Length; i += 2, j++)
    //    //{
    //    //    d[j] = data[i];
    //    //}

    //    //print("RMS: " + GetT(d));

    //    //if (data.Length == 0)
    //    //{
    //    //    return;
    //    //}

    //    //int count = data.Length;
    //    //int offset = 0;
    //    //int inAvailable = count / channels;
    //    //float[] buffer = data.ToArray();

    //    //float[] inBuffer;
    //    //int inBufferOffset;
    //    //int framesRequested = count / channels;
    //    //int inNeeded = resampler.ResamplePrepare(framesRequested, channels, out inBuffer, out inBufferOffset);
    //    //int outAvailable = resampler.ResampleOut(buffer, offset, inAvailable, framesRequested, channels);

    //    //int resampledCount = outAvailable * channels;
    //    //for (int i = 0; i < resampledCount; i++)
    //    //{
    //    //    float sample = buffer[i];
    //    //    resampledQueue.Enqueue(sample);
    //    //}

    //    //print("resampled " + resampledCount + " samples, queue size: " + resampledQueue.Count +
    //    //      ", inAvailable: " + inAvailable + ", outAvailable: " + outAvailable +
    //    //      ", inNeeded: " + inNeeded + ", framesRequested: " + framesRequested);
    //}

    public static float[] Resample(float[] inputBuffer, int inputSampleRate, int outputSampleRate)
    {
        double sampleRateRatio = (double)outputSampleRate / inputSampleRate;
        int outputBufferLength = (int)(inputBuffer.Length * sampleRateRatio);

        float[] outputBuffer = new float[outputBufferLength];

        for (int i = 0; i < outputBufferLength; i++)
        {
            double position = i / sampleRateRatio;
            int leftIndex = (int)Math.Floor(position);
            int rightIndex = leftIndex + 1;

            double fraction = position - leftIndex;

            if (rightIndex >= inputBuffer.Length)
            {
                outputBuffer[i] = inputBuffer[leftIndex];
            }
            else
            {
                outputBuffer[i] = (float)(inputBuffer[leftIndex] * (1 - fraction) + inputBuffer[rightIndex] * fraction);
            }
        }

        return outputBuffer;
    }
}
