using System;
using Agora.Rtc;
using RingBuffer;
using UnityEngine;
using System.Collections;
using Unity.Collections;
using UnityEngine.UI;

public class CustomAudioVideoManager : AgoraManager
{
    // Constants for audio configuration
    private const int CHANNEL = 2;
    private const int SAMPLE_RATE = 48000;
    private const int PUSH_FREQ_PER_SEC = 20;

    // Audio buffer for storing audio data
    private RingBuffer<byte> _audioBuffer = new RingBuffer<byte>(SAMPLE_RATE * CHANNEL, true);
    private uint audioTrackID = 0;

    // Flag to control audio conversion process
    private bool _startConvertSignal = false;
    private System.Object _rtcLock = new System.Object();

    // Texture and image variables for video sharing
    public Texture2D texture;
    private byte[] shareData = null;
    private Rect rect;
    private RawImage rawImage;
    private AudioSource audioSource;

    // Agora engine setup
    public override void SetupAgoraEngine()
    {
        InitTexture();
        base.SetupAgoraEngine();
        agoraEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY,
            AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
        ConfigureExternalAudioSource();
        ConfigureExternalVideoSource();
    }

    // Method to play custom audio source
    public void PlayCustomAudioSource()
    {
        GameObject canvas = GameObject.Find("Canvas");
        audioSource = canvas.GetComponent<AudioSource>();
        if (audioSource)
        {
            audioSource.Play();
        }
        else
        {
            Debug.Log("Audio source not found");
        }
    }

    // Method to set video encoder configuration
    public void SetVideoEncoderConfiguration()
    {
        VideoEncoderConfiguration videoEncoderConfiguration = new VideoEncoderConfiguration();
        videoEncoderConfiguration.dimensions = new VideoDimensions((int)rect.width, (int)rect.height);
        agoraEngine.SetVideoEncoderConfiguration(videoEncoderConfiguration);
    }

    // Join the channel
    public override void Join()
    {
        base.Join();
        PlayCustomAudioSource();
        _startConvertSignal = true;
        UpdateChannelMediaOptions();
        SetVideoEncoderConfiguration();
        PreviewCustomVideoSourceOutput();
    }

    // Method to configure external audio source
    private void ConfigureExternalAudioSource()
    {
        lock (_rtcLock)
        {
            audioTrackID = agoraEngine.CreateCustomAudioTrack(AUDIO_TRACK_TYPE.AUDIO_TRACK_MIXABLE, new AudioTrackConfig(false));
        }
    }

    // Leave the channel
    public override void Leave()
    {
        _startConvertSignal = false;
        base.Leave();
        StopAudioFile();
    }

    // Initialize texture for video sharing
    private void InitTexture()
    {
        rect = new Rect(0, 0, Screen.width, Screen.height);
        texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
    }

    // Configure external video source
    private void ConfigureExternalVideoSource()
    {
        Texture2D texture = Resources.Load<Texture2D>("agora");
        var ret = agoraEngine.SetExternalVideoSource(true, false, EXTERNAL_VIDEO_SOURCE_TYPE.VIDEO_FRAME, new SenderOptions());
    }

    // Coroutine for sharing screen
    public IEnumerator ShareScreen()
    {
        PushAudioFrameCoroutine();
        yield return new WaitForEndOfFrame();
        if (agoraEngine != null)
        {
            texture.ReadPixels(rect, 0, 0);
            texture.Apply();

            // Check Unity version for texture data access
#if UNITY_2018_1_OR_NEWER
            NativeArray<byte> nativeByteArray = texture.GetRawTextureData<byte>();
            if (shareData?.Length != nativeByteArray.Length)
            {
                shareData = new byte[nativeByteArray.Length];
            }
            nativeByteArray.CopyTo(shareData);
#else
            shareData = texture.GetRawTextureData();
#endif

            ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
            externalVideoFrame.type = VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
            externalVideoFrame.format = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA;
            externalVideoFrame.buffer = shareData;
            externalVideoFrame.stride = (int)rect.width;
            externalVideoFrame.height = (int)rect.height;
            externalVideoFrame.rotation = 180;
            externalVideoFrame.timestamp = DateTime.Now.Ticks / 1000;
            var ret = agoraEngine.PushVideoFrame(externalVideoFrame);
            Debug.Log("PushVideoFrame ret = " + ret + "time: " + DateTime.Now.Millisecond);
        }
    }

    // Destroy the Agora engine
    public override void DestroyEngine()
    {
        if(audioSource)
        {
            audioSource.Stop();
        }
        if (agoraEngine == null)
        {
            return;
        }
        agoraEngine.DestroyCustomAudioTrack(audioTrackID);
        base.DestroyEngine();
    }

    // Stop playing audio file
    private void StopAudioFile()
    {
        GameObject canvas = GameObject.Find("Canvas");
        AudioSource audioSource = canvas.GetComponent<AudioSource>();
        audioSource.Stop();
    }

    // Coroutine for pushing audio frames
    public IEnumerator PushAudioFrameCoroutine()
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
            yield return null; // Wait for the next frame

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
                        Debug.Log("PushAudioFrame");
                    }
                }

                if (nRet == 0)
                {
                    tick++;
                    double nextMillisecond = startMillisecond + tick * freq;
                    double curMillisecond = GetTimestamp();
                    int sleepMillisecond = (int)Math.Ceiling(nextMillisecond - curMillisecond);
                    if (sleepMillisecond > 0)
                    {
                        yield return new WaitForSeconds(sleepMillisecond / 1000.0f);
                    }
                }
            }
        }
    }

    // Callback for Unity's OnAudioFilterRead
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
                var byteArr = BitConverter.GetBytes(shortData);

                _audioBuffer.Put(byteArr[0]);
                _audioBuffer.Put(byteArr[1]);
            }
        }
    }

    // Preview the custom video source output
    public void PreviewCustomVideoSourceOutput()
    {
        // Update the VideoSurface component of the local view GameObject.
        GameObject localViewGo = LocalView.gameObject;
        LocalView = localViewGo.AddComponent<VideoSurface>();
        // Render the screen sharing track on the local view GameObject.
        LocalView.SetForUser(configData.uid, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CUSTOM);
    }

    // Update channel media options
    public void UpdateChannelMediaOptions()
    {
        ChannelMediaOptions options = new ChannelMediaOptions();
        options.publishCustomAudioTrack.SetValue(true);
        options.publishCustomAudioTrackId.SetValue((int)audioTrackID);
        options.publishCustomVideoTrack.SetValue(true);
        options.publishCameraTrack.SetValue(false);
        options.publishMicrophoneTrack.SetValue(true);
        options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        agoraEngine.UpdateChannelMediaOptions(options);
    }

    // Helper method to get current timestamp
    private double GetTimestamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return ts.TotalMilliseconds;
    }
}
