using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using TMPro;
using Agora.Rtc;
using System.Threading.Tasks;

public class TokenStruct
{
    public string rtcToken;
}

public class AuthenticationWorkflowManager : AgoraManager
{
    public AuthenticationWorkflowManager(VideoSurface refLocalSurface, VideoSurface refRemoteSurface)
    {
        LocalView = refLocalSurface;
        RemoteView = refRemoteSurface;
        SetupVideoSDKEngine();
        RtcEngine.InitEventHandler(new AuthenticationWorkflowEventHandler(this));
    }
    public async Task FetchToken()
    {
        Debug.Log(configData);
        string url = string.Format("{0}/rtc/{1}/1/uid/{2}/?expiry={3}", configData.tokenUrl, configData.channelName, configData.uid, configData.tokenExpiryTime);
        Debug.Log(url);
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