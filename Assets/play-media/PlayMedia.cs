using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PlayMedia : AgoraUI
{
    internal PlayMediaManager playMediaManager;
    internal GameObject audienceToggleGo, hostToggleGo;
    internal GameObject mediaPlayerBtnGo;
    internal GameObject sliderGo;

    // Start is called before the first frame update
    public override void Start()
    {
        // Setup UI
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        // Create and position UI elements
        joinBtn = AddButton("Join", new Vector3(-350, -172, 0), "Join", new Vector2(160f, 30f));
        leaveBtn = AddButton("Leave", new Vector3(350, -172, 0), "Leave", new Vector2(160f, 30f));
        LocalViewGo = MakeLocalView("LocalView", new Vector3(-250, -2, 0), new Vector2(271, 294));
        mediaPlayerBtnGo = AddButton("PlayMediaBtn", new Vector3(-182, -172,0), "Load Media File", new Vector2(160f, 30f));
        sliderGo = AddSlider("mediaPlayerSlider", new Vector2(319,171));

        // Create an instance of the AgoraManagerGetStarted
        playMediaManager = new PlayMediaManager();

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
                playMediaManager.setClientRole("Host");
            });

            audienceToggle.onValueChanged.AddListener((value) =>
            {
                hostToggle.isOn = !value;
                playMediaManager.setClientRole("Audience");
            });
        }


        // Add click-event functions to the join and leave buttons
        leaveBtn.GetComponent<Button>().onClick.AddListener(playMediaManager.Leave);
        joinBtn.GetComponent<Button>().onClick.AddListener(playMediaManager.Join);

        // Pass the media button text handle and the progress bar slider to PlayMediaManager to play media.
        TMP_Text text = mediaPlayerBtnGo.GetComponentInChildren<TextMeshProUGUI>(true);
        Slider slider = sliderGo.GetComponent<Slider>();
        mediaPlayerBtnGo.GetComponent<Button>().onClick.AddListener(() => playMediaManager.playMedia(slider, text));
    }
    public override void OnDestroy()
    {
        // Clean up
        base.OnDestroy();
        playMediaManager.DestroyEngine();
        if (audienceToggleGo)
            Destroy(LocalViewGo.gameObject);
        if (sliderGo)
            Destroy(sliderGo.gameObject);
    }
}
