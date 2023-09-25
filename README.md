# Agora Video SDK for Unity reference game

This app demonstrates use of [Agora's Video SDK](https://docs.agora.io/en/video-calling/get-started/get-started-sdk) for real-time audio and video communication. It is a robust and comprehensive documentation reference app for Android, designed to enhance your productivity and understanding. It's built to be flexible, easily extensible, and beginner-friendly.

Clone the repo, run and test the samples, and use the code in your own project. Enjoy.

- [Samples](#samples)
- [Prerequisites](#prerequisites)
- [Run this project](#run-this-project)
- [Contact](#contact)

## Samples  

The runnable code examples are:

- [SDK quickstart](./Assets/get-started) - the minimum code you need to integrate low-latency, high-concurrency
  video calling features into your app using Agora Video SDK.
- [Secure authentication with tokens](./Assets/authentication-workflow/) - quickly set up an authentication token 
  server, retrieve
  a token from the server, and use it to connect securely to the SD-RTN server as a specific user.
- [Call quality best practice](./Assets/ensure-call-quality/) - ensure optimal audio and video quality in your game.
- [Connect through restricted networks with Cloud Proxy](./Assets/cloud-proxy/) - ensure reliable connectivity for 
  your users when they connect from an
  environment with a restricted network.
- [Secure channel encryption](./Assets/media-stream-encryption/) - integrate built-in data encryption into your app 
  using Video SDK.
- [Stream media to a channel](./Assets/play-media/) - play video and audio files during online social and business interactions.
- [Audio and Voice Effect](./Assets/audio-and-voice-effects) - Implement different audio and voice effect features.

## Prerequisites

Before getting started with this reference app, ensure you have the following set up:

- [Unity Hub](https://unity.com/download)
- [Unity Editor 2017.X LTS or higher](https://unity.com/releases/editor/archive)
- Microsoft Visual Studio 2017 or higher

## Run this project

To run the sample game, take the following steps:

1. **Clone the repository**

    To clone the repository to your local machine, open Terminal and navigate to the directory where you want to clone the repository. Then, use the following command:

    ```bash
    git https://github.com/AgoraIO/video-sdk-samples-unity.git
    ```

1. **Open the project**   

    1. In Unity Hub, Open `video-sdk-samples-unity`, Unity Editor opens the project.
       
       Unity Editor warns of compile errors. Don't worry, you fix them when you import Video SDK for Unity. 

    1. Go `Assets\Scenes`, and open `SampleScene.unity`. The sample scene opens.
         
    1. Unzip [the latest version of the Agora Video SDK](https://docs.agora.io/en/sdks?platform=unity) to a local folder.

   1. In **Unity**, click **Assets** > **Import Package** > **Custom Package**.

   1. Navigate to the Video SDK package and click **Open**.

   1. In **Import Unity Package**, click **Import**.
   
      Unity recompiles the Video SDK samples for Unity and the warnings disappear. 

1. **Modify the project configuration**

   The app loads connection parameters from [`./Assets/agora-manager/config.json`](./Assets/agora-manager/config.json)
   . Ensure that the file is populated with the required parameter values before running the application.

    - `uid`: The user ID associated with the application.
    - `appId`: (Required) The unique ID for the application obtained from [Agora Console](https://console.agora.io). 
    - `channelName`: The default name of the channel to join.
    - `token`: An token generated for `channelName`. You generate a temporary token using the [Agora token builder](https://agora-token-generator-demo.vercel.app/).
    - `serverUrl`: The URL for the token generator. See [Secure authentication with tokens](authentication-workflow) for information on how to set up a token server.
    - `tokenExpiryTime`: The time in seconds after which a token expires.

    If a valid `serverUrl` is provided, all samples use the token server to obtain a token except the **SDK quickstart** project that uses the `rtcToken`. If a `serverUrl` is not specified, all samples except **Secure authentication with tokens** use the `rtcToken` from `config.json`.

1. **Build and run the project**

    In **Unity Editor**, click **Play**. A moment later you see the game running on your development device.
1. **Run the samples in the reference app**

   Choose a sample code from the dropdown that you wish to execute.

## Contact

If you have any questions, issues, or suggestions, please file an issue in our [GitHub Issue Tracker](https://github.com/AgoraIO/video-sdk-samples-unity/issues).