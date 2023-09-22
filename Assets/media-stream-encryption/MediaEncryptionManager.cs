using UnityEngine;
using Agora.Rtc;
using System;

public class MediaEncryptionManager : AuthenticationWorkflowManager
{

    public override void SetupAgoraEngine()
    {
        base.SetupAgoraEngine();

        // Enable media stream encryption
        enableEncryption();

    }

    public override void Join()
    {
        base.Join();

    }
    public override void Leave()
    {
        // Leave the channel.
        base.Leave();
    }

    void enableEncryption()
    {
        if (agoraEngine != null)
        {
            if(configData.encryptionKey == "" || configData.salt == "")
            {
                Debug.Log("Encryption key or encryption salt were not specified in the config.json file");
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
            agoraEngine.EnableEncryption(true, config);
        }
    }
    
}
