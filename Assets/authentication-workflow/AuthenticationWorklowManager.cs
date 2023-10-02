using UnityEngine;
using UnityEngine.Networking;
using Agora.Rtc;
using System.Threading.Tasks;

public class TokenStruct
{
    public string rtcToken;
}

public class AuthenticationWorkflowManager : AgoraManager
{
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
        Debug.Log("Retrieved token : " + tokenInfo.rtcToken);
        _token = tokenInfo.rtcToken;
        _channelName = configData.channelName;
    }
    public void RenewToken()
    {
        if(_token == "")
        {
            Debug.Log("Token was not retrieved");
            return;
        }
        // Update RTC Engine with new token, which will not expire so soon
        agoraEngine.RenewToken(_token);
    }

    public override void SetupAgoraEngine()
    {
        base.SetupAgoraEngine();

        // Attach the eventHandler
        agoraEngine.InitEventHandler(new AuthenticationWorkflowEventHandler(this));
    }

    public override async void Join()
    {
        if (configData.tokenUrl == "")
        {
            Debug.Log("Please specify a valid token server URL inside `config.json`");
        }
        else
        {
            await FetchToken();
        }

        // Join the channel using the specified token and channel name.
        base.Join();

    }
    public override void Leave()
    {
        // Leave the channel.
        base.Leave();
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