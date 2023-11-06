using System;
using Agora.Rtc;
using RingBuffer;
using System.Threading;
using UnityEngine;
using System.Collections;
using Unity.Collections;

public class CustomAudioVideoManager : AgoraManager
{
    private const int CHANNEL = 2;
    // Please do not change this value because Unity re-samples the sample rate to 48000.
    private const int SAMPLE_RATE = 48000;

    // Number of push audio frame per second.
    private const int PUSH_FREQ_PER_SEC = 20;

    private RingBuffer<byte> _audioBuffer;
    private bool _startConvertSignal = false;
    private uint audioTrackID = 0;

    private Thread _pushAudioFrameThread;
    private System.Object _rtcLock = new System.Object();

    public Texture2D texture;

    private byte[] shareData = null;

    private Rect rect;

    private uint videoTrackID = 0;

    public override void SetupAgoraEngine()
    {
        InitTexture();
        base.SetupAgoraEngine();
        agoraEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY,
            AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
        SetExternalAudioSource();
        SetExternalVideoSource();
        InitEventHandler();
    }
    public void SetupAudioSource()
    {

        // Find the Canvas GameObject
        GameObject canvas = GameObject.Find("Canvas");
        AudioSource audioSource = canvas.GetComponent<AudioSource>();
        if(audioSource)
        {
            // Play the audio
            audioSource.Play();
        }
        else
        {
            Debug.Log("Audio source not found");
        }

    }
    public void SetVideoEncoderConfiguration()
    {
        VideoEncoderConfiguration videoEncoderConfiguration = new VideoEncoderConfiguration();
        videoEncoderConfiguration.dimensions = new VideoDimensions((int)rect.width, (int)rect.height);
        agoraEngine.SetVideoEncoderConfiguration(videoEncoderConfiguration);
    }
    public override void Join()
    {
        base.Join();
        SetupAudioSource();
        StartPushAudioFrame();
        UpdateChannelMediaOptions();
        SetVideoEncoderConfiguration();
    }

    private void SetExternalAudioSource()
    {
        lock (_rtcLock)
        {
            audioTrackID = agoraEngine.CreateCustomAudioTrack(AUDIO_TRACK_TYPE.AUDIO_TRACK_MIXABLE, new AudioTrackConfig(false));
        }
    }
    public override void Leave()
    {
        base.Leave();
        StopAudioFile();
    }
    private void InitTexture()
    {
        rect = new Rect(0, 0, Screen.width, Screen.height);
        texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
    }

    private void SetExternalVideoSource()
    {
        var ret = agoraEngine.SetExternalVideoSource(true, false, EXTERNAL_VIDEO_SOURCE_TYPE.VIDEO_FRAME, new SenderOptions());
        videoTrackID = agoraEngine.CreateCustomVideoTrack();
        agoraEngine.DisableVideo();
        LocalView.SetForUser(configData.uid, configData.channelName, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CUSTOM);
    }

    private void StartPushAudioFrame()
    {
        // 1-sec-length buffer
        var bufferLength = SAMPLE_RATE * CHANNEL;
        _audioBuffer = new RingBuffer<byte>(bufferLength, true);
        _startConvertSignal = true;
        _pushAudioFrameThread = new Thread(PushAudioFrameThread);
        _pushAudioFrameThread.Start();
    }
    public IEnumerator ShareScreen()
    {
        yield return new WaitForEndOfFrame();
        if (agoraEngine != null)
        {
            texture.ReadPixels(rect, 0, 0);
            texture.Apply();

#if UNITY_2018_1_OR_NEWER
            NativeArray<byte> nativeByteArray = texture.GetRawTextureData<byte>();
            if (shareData?.Length != nativeByteArray.Length)
            {
                shareData = new byte[nativeByteArray.Length];
            }
            nativeByteArray.CopyTo(shareData);
#else
                _shareData = _texture.GetRawTextureData();
#endif

            ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
            externalVideoFrame.type = VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
            externalVideoFrame.format = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA;
            externalVideoFrame.buffer = shareData;
            externalVideoFrame.stride = (int)rect.width;
            externalVideoFrame.height = (int)rect.height;
            externalVideoFrame.cropLeft = 10;
            externalVideoFrame.cropTop = 10;
            externalVideoFrame.cropRight = 10;
            externalVideoFrame.cropBottom = 10;
            externalVideoFrame.rotation = 180;
            externalVideoFrame.timestamp = DateTime.Now.Ticks / 10000;
            var ret = agoraEngine.PushVideoFrame(externalVideoFrame, videoTrackID);
            Debug.Log("PushVideoFrame ret = " + ret + "time: " + DateTime.Now.Millisecond);
        }
    }

    public override void DestroyEngine()
    {
        if (agoraEngine == null)
        {
            return;
        }
        // need wait pullAudioFrameThread stop 
        _pushAudioFrameThread.Abort();
        agoraEngine.DestroyCustomAudioTrack(audioTrackID);
        agoraEngine.DestroyCustomVideoTrack(videoTrackID);
        base.DestroyEngine();

    }
    private void StopAudioFile()
    {
        // Find the Canvas GameObject
        GameObject canvas = GameObject.Find("Canvas");
        AudioSource audioSource = canvas.GetComponent<AudioSource>();
        audioSource.Stop();
    }
    private void PushAudioFrameThread()
    {
        var bytesPerSample = 2;
        var type = AUDIO_FRAME_TYPE.FRAME_TYPE_PCM16;
        var channels = CHANNEL;
        var samples = SAMPLE_RATE / PUSH_FREQ_PER_SEC;
        var samplesPerSec = SAMPLE_RATE;

        var freq = 1000 / PUSH_FREQ_PER_SEC;

        var audioFrame = new AudioFrame
        {
            bytesPerSample = BYTES_PER_SAMPLE.TWO_BYTES_PER_SAMPLE,
            type = type,
            samplesPerChannel = samples,
            samplesPerSec = samplesPerSec,
            channels = channels,
            RawBuffer = new byte[samples * bytesPerSample * CHANNEL],
            renderTimeMs = freq
        };

        double startMillisecond = GetTimestamp();
        long tick = 0;

        while (true)
        {
            lock (_rtcLock)
            {
                if (agoraEngine == null)
                {
                    break;
                }

                int nRet = -1;
                lock (_audioBuffer)
                {
                    if (_audioBuffer.Size > samples * bytesPerSample * CHANNEL)
                    {
                        for (var j = 0; j < samples * bytesPerSample * CHANNEL; j++)
                        {
                            audioFrame.RawBuffer[j] = _audioBuffer.Get();
                        }
                        nRet = agoraEngine.PushAudioFrame(audioFrame, audioTrackID);
                        //Debug.Log("PushAudioFrame returns: " + nRet);

                    }
                }

                if (nRet == 0)
                {
                    tick++;
                    double nextMillisecond = startMillisecond + tick * freq;
                    double curMillisecond = GetTimestamp();
                    int sleepMillisecond = (int)Math.Ceiling(nextMillisecond - curMillisecond);
                    //Debug.Log("sleepMillisecond : " + sleepMillisecond);
                    if (sleepMillisecond > 0)
                    {
                        Thread.Sleep(sleepMillisecond);
                    }
                }
            }

        }
    }

    // The OnAudioFilterRead method is a Unity-specific callback that is invoked for each audio frame.
    // In this method, you process the audio data and then push it into the _audioBuffer for transmission to the Agora engine.
    // You can apply the same approach to read audio from a custom source and push audio frames to the channel.
    public void OnAudioFilterRead(float[] data, int channels)
    {
        if (!_startConvertSignal) return;
        var rescaleFactor = 32767;
        lock (_audioBuffer)
        {
            foreach (var t in data)
            {
                var sample = t;
                if (sample > 1) sample = 1;
                else if (sample < -1) sample = -1;

                var shortData = (short)(sample * rescaleFactor);
                var byteArr = new byte[2];
                byteArr = BitConverter.GetBytes(shortData);

                _audioBuffer.Put(byteArr[0]);
                _audioBuffer.Put(byteArr[1]);
            }
        }
    }

    public void UpdateChannelMediaOptions()
    {
        ChannelMediaOptions options = new ChannelMediaOptions();
        options.publishCustomAudioTrack.SetValue(true);
        options.publishCustomAudioTrackId.SetValue((int)audioTrackID);
        options.customVideoTrackId.SetValue(videoTrackID);
        options.publishCustomVideoTrack.SetValue(true);
        options.publishCameraTrack.SetValue(false);
        options.publishMicrophoneTrack.SetValue(true);
        options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        agoraEngine.UpdateChannelMediaOptions(options);
    }

    //get timestamp millisecond
    private double GetTimestamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return ts.TotalMilliseconds;
    }
}