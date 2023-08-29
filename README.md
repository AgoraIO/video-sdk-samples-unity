# Agora Signaling SDK for Web v2.x reference app

This repository holds the code examples used for the [Agora Video SDK for Unity](https://docs-staging-git-milestone-21-new-get-started-2d5f31-agora-gdxe.vercel.app/en/sdks?platform=unity) documentation. Clone the repo, run and test the samples, use the code in your own project. Enjoy.

## Samples  

The runnable code examples are:

- [SDK quickstart](/Assets/get_started) - the minimum code you need to integrate low-latency, high-concurrency
  video calling features into your app using Agora Video SDK.
- [Secure authentication with tokens](/Assets/Authentication_Wokflow/) - quickly set up an authentication token server, retrieve
  a token from the server, and use it to connect securely to the SD-RTN server as a specific user.
- [Connect through restricted networks with Cloud Proxy](/Assets/cloud_proxy/) - ensure reliable connectivity for your users when they connect from an
  environment with a restricted network.
- [Media encryption](/Assets/media_stream_encryption/) - integrate built-in data encryption into your app using Video SDK.

- [Call Quality](/Assets/ensure_call_quality/) - ensure optimal audio and video quality in your game.


## Run this project

To run the sample game, take the following steps:

1. Clone the Git repository by executing the following command in a terminal window:

    ```bash
    git https://github.com/AgoraIO/video-sdk-samples-unity.git
    ```
    Unzip the cloned project and open it the unity editor.

    ```

1. Go to [SDKs](https://docs.agora.io/en/sdks?platform=unity), download the latest version of the Agora Video SDK, and unzip the downloaded SDK to a local folder.

1. In **Unity**, click **Assets** > **Import Package** > **Custom Package**.

1. Navigate to the Video SDK package and click **Open**.

1. In **Import Unity Package**, click **Import**.

1. Generate [temporary authentication tokens](https://webdemo.agora.io/token-builder/). 
   In Video SDK, each token you create for your app is specific to a user ID. To test your app, you need a token for each user in the channel. 

1. In `Assets/AgoraManager/config.json`, replace `appId`, `channelName`, `uid` and `token` values with your app ID, channel name, and authentication token.

1. In **Unity Editor**, click **Play**. A moment later you see the game running on your development device.
    Choose a sample code from the dropdown that you wish to execute.

