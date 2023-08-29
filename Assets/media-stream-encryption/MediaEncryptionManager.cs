using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agora.Rtc;
using System;

public class MediaEncryptionManager : AuthenticationWorkflowManager
{  
    // Start is called before the first frame update
     public MediaEncryptionManager(VideoSurface LocalVideoSurface, VideoSurface RemoteVideoSurface): base (LocalVideoSurface, RemoteVideoSurface)
    {
        // Check if the required permissions are granted
        CheckPermissions();

        // Create an instance of the engine.
        SetupVideoSDKEngine();

        // Enable media stream encryption
        enableEncryption();

        // Setup an event handler to receive callbacks.
        InitEventHandler();

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
