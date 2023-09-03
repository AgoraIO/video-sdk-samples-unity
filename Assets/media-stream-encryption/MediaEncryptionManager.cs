using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agora.Rtc;
using System;

public class MediaEncryptionManager : AuthenticationWorkflowManager
{
    
    // Start is called before the first frame update
    public MediaEncryptionManager(GameObject LocalViewGo, GameObject RemoteViewGo): base(LocalViewGo, RemoteViewGo)
    {
        // Check if the required permissions are granted
        CheckPermissions();
    }

    public override void Join()
    {
        base.Join();

        // Create an instance of the engine.
        SetupAgoraEngine();

        // Enable media stream encryption
        enableEncryption();

        // Setup an event handler to receive callbacks.
        InitEventHandler();
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

    void enableEncryption()
    {
        if (RtcEngine != null)
        {
            if(configData.encryptionKey == "" || configData.salt == "")
            {
                Debug.Log("Encryption key or encryption salt was not set");
                return;
            }
            // Create an encryption configuration.
            var config = new EncryptionConfig
            {
                // Specify a encryption mode
                encryptionMode = ENCRYPTION_MODE.AES_128_GCM2,
                // Assign a secret key.
                encryptionKey = configData.encryptionKey,
                // Assign a salt in Base64 format
                encryptionKdfSalt = Convert.FromBase64String(configData.salt)
            };
            // Enable the built-in encryption.
            RtcEngine.EnableEncryption(true, config);
        }
    }
    
}
