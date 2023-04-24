#  SDK quickstart

Video Calling enables one-to-one or small-group video chat connections with smooth, jitter-free streaming video. Agora’s Video SDK makes it easy to embed real-time video chat into web, mobile and native apps.

Thanks to Agora’s intelligent and global Software Defined Real-time Network (Agora SD-RTN™), you can rely on the highest available video and audio quality.

This page shows the minimum code you need to integrate high-quality, low-latency Video Calling features into your app using Video SDK.

## Topics

### Agora Logic

Most of the business logic for the Agora quickstart guide can be found in [AgoraManager](AgoraManager.cs). Here you will find code snippets for [initializing the engine](AgoraManager.swift#L23-L28), [joining](AgoraManager.swift#L84-L86) and [leaving the channel](AgoraManager.swift#L97-L99), and handling the  different engine events.

For adding the required UI interface element to the Canvas and adding video surface to the local and remote video views, the project uses [UserInterface](UserInterface.cs).
### Creating a Canvas

To create a canvas to add the UI elements, the project uses [SetupCanvas](). 

## Full Documentation

[Agora's full SDK Quickstart Guide](https://docs.agora.io/en/interactive-live-streaming/get-started/get-started-sdk?platform=unity)
