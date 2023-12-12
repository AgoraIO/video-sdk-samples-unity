using System;
using Agora.Rtc;
using RingBuffer;
using UnityEngine;
using UnityEngine.UI;

public class RawAudioVideoManager : AgoraManager
{
    internal byte[] VideoBuffer = new byte[0];
    private bool _needResize = false;

    public int _videoFrameWidth = 1080;
    public int VideoFrameWidth
    {
        set
        {
            if (value != _videoFrameWidth)
            {
                _needResize = true;
            }
        }

        get
        {
            return _videoFrameWidth;
        }
    }

    public int _videoFrameHeight = 720;
    public int VideoFrameHeight
    {
        set
        {
            if (value != _videoFrameHeight)
            {
                _needResize = true;
            }
        }

        get
        {
            return _videoFrameHeight;
        }
    }

    private bool _isTextureAttach = false;
    private Texture2D _texture;

    public int CHANNEL = 2;
    public int PULL_FREQ_PER_SEC = 100;
    public int SAMPLE_RATE = 48000;

    internal int _count;
    internal int _writeCount;
    internal int _readCount;
    internal RingBuffer<float> _audioBuffer;
    internal AudioClip _audioClip;

    // Initialize the texture
    public void InitializeTexture()
    {
        _texture = new Texture2D(_videoFrameWidth, _videoFrameHeight, TextureFormat.RGBA32, false);
        _texture.Apply();
    }

    // Set video encoder configuration
    public void SetVideoEncoderConfiguration()
    {
        VideoEncoderConfiguration config = new VideoEncoderConfiguration();
        config.dimensions = new VideoDimensions(_videoFrameWidth, _videoFrameHeight);
        agoraEngine.SetVideoEncoderConfiguration(config);
    }

    public override void SetupAgoraEngine()
    {
        var bufferLength = SAMPLE_RATE * CHANNEL; // 1-sec-length buffer
        _audioBuffer = new RingBuffer<float>(bufferLength, true);
        var canvas = GameObject.Find("Canvas");
        var aud = canvas.AddComponent<AudioSource>();
        SetupAudio(aud, "externalClip");
        base.SetupAgoraEngine();
        InitializeTexture();
        agoraEngine.InitEventHandler(new UserEventHandler(this));
        agoraEngine.RegisterVideoFrameObserver(new RawAudioVideoEventHandler(this),
            VIDEO_OBSERVER_FRAME_TYPE.FRAME_TYPE_RGBA,
            VIDEO_OBSERVER_POSITION.POSITION_POST_CAPTURER |
            VIDEO_OBSERVER_POSITION.POSITION_PRE_RENDERER |
            VIDEO_OBSERVER_POSITION.POSITION_PRE_ENCODER,
            OBSERVER_MODE.RAW_DATA);
        SetVideoEncoderConfiguration();
        agoraEngine.SetPlaybackAudioFrameParameters(SAMPLE_RATE, CHANNEL,
            RAW_AUDIO_FRAME_OP_MODE_TYPE.RAW_AUDIO_FRAME_OP_MODE_READ_ONLY, 1024);
        agoraEngine.SetRecordingAudioFrameParameters(SAMPLE_RATE, CHANNEL,
            RAW_AUDIO_FRAME_OP_MODE_TYPE.RAW_AUDIO_FRAME_OP_MODE_READ_ONLY, 1024);
        agoraEngine.SetMixedAudioFrameParameters(SAMPLE_RATE, CHANNEL, 1024);
        agoraEngine.SetEarMonitoringAudioFrameParameters(SAMPLE_RATE, CHANNEL,
            RAW_AUDIO_FRAME_OP_MODE_TYPE.RAW_AUDIO_FRAME_OP_MODE_READ_ONLY, 1024);
        agoraEngine.RegisterAudioFrameObserver(new RawAudioEventHandler(this),
            AUDIO_FRAME_POSITION.AUDIO_FRAME_POSITION_PLAYBACK |
            AUDIO_FRAME_POSITION.AUDIO_FRAME_POSITION_RECORD |
            AUDIO_FRAME_POSITION.AUDIO_FRAME_POSITION_MIXED |
            AUDIO_FRAME_POSITION.AUDIO_FRAME_POSITION_BEFORE_MIXING |
            AUDIO_FRAME_POSITION.AUDIO_FRAME_POSITION_EAR_MONITORING,
            OBSERVER_MODE.RAW_DATA);
        agoraEngine.AdjustPlaybackSignalVolume(0);
    }

    void SetupAudio(AudioSource aud, string clipName)
    {
        _audioClip = AudioClip.Create(clipName,
            SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL,
            CHANNEL, SAMPLE_RATE, true,
            OnAudioRead);
        aud.clip = _audioClip;
        aud.loop = true;
        aud.Play();
    }

    public override void DestroyEngine()
    {
        if (agoraEngine == null)
        {
            return;
        }
        agoraEngine.UnRegisterVideoFrameObserver();
        base.DestroyEngine();
    }

    private void OnAudioRead(float[] data)
    {
        lock (_audioBuffer)
        {
            for (var i = 0; i < data.Length; i++)
            {
                if (_audioBuffer.Count > 0)
                {
                    data[i] = _audioBuffer.Get();
                    _readCount += 1;
                }
            }
        }

        Debug.LogFormat("buffer length remains: {0}", _writeCount - _readCount);
    }

    public void ResizeVideoFrame()
    {
        if (!_isTextureAttach)
        {
            var rd = LocalView.GetComponent<RawImage>();
            rd.texture = _texture;
            _isTextureAttach = true;
        }
        else if (VideoBuffer != null && VideoBuffer.Length != 0 && !_needResize)
        {
            lock (VideoBuffer)
            {
                _texture.LoadRawTextureData(VideoBuffer);
                _texture.Apply();
            }
        }
        else if (_needResize)
        {
            Debug.Log("Resized frame ==> (Width: " + _videoFrameHeight + " Height: " + _videoFrameWidth + ")");
            // Adjust the texture width and height.
            _texture.Resize(_videoFrameHeight, _videoFrameHeight);
            _texture.Apply();
            _needResize = false;
        }
    }

    internal static float[] ConvertByteToFloat16(byte[] byteArray)
    {
        var floatArray = new float[byteArray.Length / 2];
        for (var i = 0; i < floatArray.Length; i++)
        {
            floatArray[i] = BitConverter.ToInt16(byteArray, i * 2) / 32768f; // -Int16.MinValue
        }

        return floatArray;
    }
}

// Internal class for handling media player events
internal class RawAudioVideoEventHandler : IVideoFrameObserver
{
    private RawAudioVideoManager rawAudioVideoManager;

    internal RawAudioVideoEventHandler(RawAudioVideoManager refRawAudioVideoManager)
    {
        rawAudioVideoManager = refRawAudioVideoManager;
    }

    public override bool OnCaptureVideoFrame(VIDEO_SOURCE_TYPE type, VideoFrame videoFrame)
    {
        rawAudioVideoManager.VideoFrameWidth = videoFrame.width;
        rawAudioVideoManager.VideoFrameHeight = videoFrame.height;
        lock (rawAudioVideoManager.VideoBuffer)
        {
            rawAudioVideoManager.VideoBuffer = videoFrame.yBuffer;
        }
        return true;
    }

    public override bool OnRenderVideoFrame(string channelId, uint uid, VideoFrame videoFrame)
    {
        Debug.Log("OnRenderVideoFrameHandler-----------" + " uid:" + uid + " width:" + videoFrame.width +
            " height:" + videoFrame.height);
        return true;
    }
}

// Internal class for handling audio events
internal class RawAudioEventHandler : IAudioFrameObserver
{
    private RawAudioVideoManager _agoraAudioRawData;

    internal RawAudioEventHandler(RawAudioVideoManager agoraAudioRawData)
    {
        _agoraAudioRawData = agoraAudioRawData;
    }

    public override bool OnRecordAudioFrame(string channelId, AudioFrame audioFrame)
    {
        return true;
    }

    public override bool OnPlaybackAudioFrame(string channelId, AudioFrame audioFrame)
    {
        var floatArray = RawAudioVideoManager.ConvertByteToFloat16(audioFrame.RawBuffer);

        lock (_agoraAudioRawData._audioBuffer)
        {
            _agoraAudioRawData._audioBuffer.Put(floatArray);
            _agoraAudioRawData._writeCount += floatArray.Length;
            _agoraAudioRawData._count++;
        }
        return true;
    }

    public override bool OnPlaybackAudioFrameBeforeMixing(string channel_id,
        uint uid,
        AudioFrame audio_frame)
    {
        return false;
    }

    public override bool OnPlaybackAudioFrameBeforeMixing(string channel_id,
        string uid,
        AudioFrame audio_frame)
    {
        return false;
    }
}
