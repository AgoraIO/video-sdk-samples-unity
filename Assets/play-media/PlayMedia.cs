using Agora.Rtc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayMedia : AgoraUI
{
    internal PlayMediaManager playMediaManager;
    internal GameObject audienceToggleGo, hostToggleGo;
    internal GameObject mediaPlayerBtnGo;
    internal GameObject mediaProgressBarGo;
    internal Slider mediaProgressBarSlider;
    internal GameObject channelFieldGo;
    internal bool isMediaPlaying = false;

    // Start is called before the first frame update
    public override void Start()
    {
        // Create an instance of the PlayMediaManager
        playMediaManager = new PlayMediaManager();

        // Setup UI elements
        SetupUI();

        // Attach a video surface to the local view game object.
        // The remote video surface is added when a remote user joins the channel.
        playMediaManager.LocalView = LocalViewGo.AddComponent<VideoSurface>();
    }

    // Function to set up UI elements
    public void SetupUI()
    {
        // Setup UI
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        // Create and position UI elements
        joinBtnGo = AddButton("Join", new Vector3(-350, -172, 0), "Join", new Vector2(160f, 30f));
        leaveBtnGo = AddButton("Leave", new Vector3(350, -172, 0), "Leave", new Vector2(160f, 30f));
        LocalViewGo = MakeLocalView("LocalView", new Vector3(-250, -2, 0), new Vector2(271, 294));
        mediaPlayerBtnGo = AddButton("PlayMediaBtn", new Vector3(-182, -172, 0), "Load Media File", new Vector2(160f, 30f));
        mediaProgressBarGo = AddSlider("mediaPlayerSlider", new Vector2(319, 171));
        channelFieldGo = AddInputField("channelName", new Vector3(0, 0, 0), "Channel Name");
        mediaProgressBarSlider = mediaProgressBarGo.GetComponent<Slider>();

        if (playMediaManager.configData.product != "Video Calling")
        {
            hostToggleGo = AddToggle("Host", new Vector2(-19, 50), "Host", new Vector2(200, 30));
            audienceToggleGo = AddToggle("Audience", new Vector2(-19, 100), "Audience", new Vector2(200, 30));
            Toggle audienceToggle = audienceToggleGo.GetComponent<Toggle>();
            Toggle hostToggle = hostToggleGo.GetComponent<Toggle>();
            hostToggle.isOn = false;
            audienceToggle.isOn = false;
            hostToggle.onValueChanged.AddListener((value) =>
            {
                audienceToggle.isOn = !value;
                playMediaManager.SetClientRole("Host");
            });

            audienceToggle.onValueChanged.AddListener((value) =>
            {
                hostToggle.isOn = !value;
                playMediaManager.SetClientRole("Audience");
            });
        }

        // Add click-event functions to the buttons
        leaveBtnGo.GetComponent<Button>().onClick.AddListener(playMediaManager.Leave);
        joinBtnGo.GetComponent<Button>().onClick.AddListener(playMediaManager.Join);
        mediaPlayerBtnGo.GetComponent<Button>().onClick.AddListener(() => ToggleMediaPlayPause());

        // Add a listener to the channel input field to update the channel name in PlayMediaManager
        TMP_InputField tmpInputField = channelFieldGo.GetComponent<TMP_InputField>();
        tmpInputField.onValueChanged.AddListener(HandleChannelFieldChange);
    }

    // Pass the channel name to the PlayMediaManager class to fetch a token from the token server
    private void HandleChannelFieldChange(string newValue)
    {
        playMediaManager.configData.channelName = newValue;
    }

    // Function to toggle media play/pause
    public void ToggleMediaPlayPause()
    {
        // Initialize the mediaPlayer and open a media file
        if (playMediaManager.mediaPlayer == null)
        {
            playMediaManager.InitMediaPlayer();
            return;
        }
        // Start or resume playing media
        if (playMediaManager.state == MEDIA_PLAYER_STATE.PLAYER_STATE_OPEN_COMPLETED)
        {
            playMediaManager.PlayMediaFile();
        }
        else if (playMediaManager.state == MEDIA_PLAYER_STATE.PLAYER_STATE_PAUSED)
        {
            playMediaManager.ResumeMediaFile();
        }
        else if (playMediaManager.state == MEDIA_PLAYER_STATE.PLAYER_STATE_PLAYING)
        {
            // Pause media file
            playMediaManager.PauseMediaFile();
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        // Update the mediaPlayerBtnGo button based on the media player state.
        if (playMediaManager.state == MEDIA_PLAYER_STATE.PLAYER_STATE_OPEN_COMPLETED)
        {
            mediaPlayerBtnGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "Play Media File";
            mediaProgressBarSlider.maxValue = playMediaManager.mediaDuration;
            mediaProgressBarSlider.minValue = 0;
        }
        else if (playMediaManager.state == MEDIA_PLAYER_STATE.PLAYER_STATE_PAUSED)
        {
            mediaPlayerBtnGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "Play Media File";
        }
        else if (playMediaManager.state == MEDIA_PLAYER_STATE.PLAYER_STATE_PLAYING)
        {
            mediaPlayerBtnGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "Pause Media File";
        }
        else if (playMediaManager.state == MEDIA_PLAYER_STATE.PLAYER_STATE_PLAYBACK_ALL_LOOPS_COMPLETED)
        {
            isMediaPlaying = false;
            mediaPlayerBtnGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "Load Media File";
        }
        else if (playMediaManager.state == MEDIA_PLAYER_STATE.PLAYER_STATE_OPENING)
        {
            mediaPlayerBtnGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "Opening File...";
        }
        else if( playMediaManager.state == MEDIA_PLAYER_STATE.PLAYER_STATE_FAILED)
        {
            mediaPlayerBtnGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "Load Media File";
        }
        mediaProgressBarSlider.value = playMediaManager.position;
    }

    //Destroy UI elements.
    private void DestroyUIElements()
    {
        // Destroy UI elements, e.g., channelFieldGo, audienceToggleGo, hostToggleGo, mediaPlayerBtnGo, and mediaProgressBarGo.
        playMediaManager.DestroyEngine();
        if (audienceToggleGo)
            Destroy(LocalViewGo.gameObject);
        if (hostToggleGo)
            Destroy(hostToggleGo.gameObject);
        if (mediaPlayerBtnGo)
            Destroy(mediaPlayerBtnGo.gameObject);
        if (mediaProgressBarGo)
            Destroy(mediaProgressBarGo.gameObject);
        if (channelFieldGo)
            Destroy(channelFieldGo.gameObject);
    }

    // Function to clean up
    public override void OnDestroy()
    {
        DestroyUIElements();
    }
}
