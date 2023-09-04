using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Agora.Rtc;
using System.Threading.Tasks;

public class TokenStruct
{
    public string rtcToken;
}

public class AuthenticationWorkflowManager : AgoraManager
{
    public AuthenticationWorkflowManager(GameObject localViewGo, GameObject RemoteViewGo): base()
    {
        LocalView = localViewGo.AddComponent<VideoSurface>();
        RemoteView = RemoteViewGo.AddComponent<VideoSurface>();
        
    }
    public async Task FetchToken()
    {
        string url = string.Format("{0}/rtc/{1}/1/uid/{2}/?expiry={3}", configData.tokenUrl, configData.channelName, configData.uid, configData.tokenExpiryTime);
        UnityWebRequest request = UnityWebRequest.Get(url);
        
        var operation = request.SendWebRequest();

        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
            return;
        }

        TokenStruct tokenInfo = JsonUtility.FromJson<TokenStruct>(request.downloadHandler.text);
        _token = tokenInfo.rtcToken;
    }
    public void RenewToken()
    {
        if(_token == "")
        {
            Debug.Log("Token was not retrieved");
            return;
        }
        // Update RTC Engine with new token, which will not expire so soon
        RtcEngine.RenewToken(_token);
    }
    public override async void Join()
    {
        SetupAgoraEngine();

        // Setup an event handler to receive callbacks.
        InitEventHandler();

        _channelName = GameObject.Find("channelName").GetComponent<TMP_InputField>().text;
        if (_channelName == "")
        {
            Debug.Log("Channel name is required!");
            return;
        }
        await FetchToken();
        if(_token == "")
        {
            Debug.Log("Token was not retrieved");
            return;
        }
        // Join the channel using the specified token and channel name.
        base.Join();
    }
    public override void Leave()
    {
        // Leave the channel.
        base.Leave();
        // Destroy the engine.
        if (RtcEngine != null)
        {
            RtcEngine.Dispose();
            RtcEngine = null;
        }
    }
}

internal class AuthenticationWorkflowEventHandler : UserEventHandler
{
    private AuthenticationWorkflowManager AuthenticationWorkflow;
    internal AuthenticationWorkflowEventHandler(AuthenticationWorkflowManager refAuthenticationWorkflow):base(refAuthenticationWorkflow) 
    {
        AuthenticationWorkflow = refAuthenticationWorkflow;
    }
    public override async void OnTokenPrivilegeWillExpire(RtcConnection connection, string token)
    {
        Debug.Log("Token Expired");
        // Retrieve a fresh token from the token server.
        await AuthenticationWorkflow.FetchToken();
        AuthenticationWorkflow.RenewToken();
    }
}