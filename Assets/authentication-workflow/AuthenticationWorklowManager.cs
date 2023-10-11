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
    public int role = 1; // By default, the user role is host.

    public async Task FetchToken()
    {
        if(userRole == "Host")
        {
            role = 1;
        }
        else if (userRole == "Audience")
        {
            role = 2;
        }

        string url = string.Format("{0}/rtc/{1}/{2}/uid/{3}/?expiry={4}", configData.tokenUrl, configData.channelName, role ,configData.uid, configData.tokenExpiryTime);

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

        // Update RTC Engine with new token
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
            Debug.Log("Specify a valid token server URL inside `config.json` if you wish to fetch token from the server");
        }
        else
        {
            await FetchToken();
        }

        // Join the channel.
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
    private AuthenticationWorkflowManager authenticationWorkflowManager;

    internal AuthenticationWorkflowEventHandler(AuthenticationWorkflowManager refAuthenticationWorkflow) : base(refAuthenticationWorkflow)
    {
        authenticationWorkflowManager = refAuthenticationWorkflow;
    }

    public override async void OnTokenPrivilegeWillExpire(RtcConnection connection, string token)
    {
        Debug.Log("Token Expired");
        // Retrieve a fresh token from the token server.
        await authenticationWorkflowManager.FetchToken();
        authenticationWorkflowManager.RenewToken();
    }

    public override async void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
    {
        // Retrieve a fresh token from the token server for the new role.
        Debug.Log("Role is set to " + newRole.ToString());
        await authenticationWorkflowManager.FetchToken();
        authenticationWorkflowManager.RenewToken();
    }
}