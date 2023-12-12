# Screen share, volume control and mute

To use Video SDK for audio and video communication, you implement a simple workflow in your game. The game joins a new or an existing channel using an app ID and an authentication token. If a channel of the given name exists within the context of the app ID, the game joins the channel. If the named channel does not exist, a new channel is created that other users may join. Once the game joins a channel, it subscribes to one or more of the audio and video streams published in the channel. The game also publishes its own audio and video streams that other users in the channel subscribe to. Each user publishes streams that share their captured camera, microphone, and screen data.

The basics of joining and leaving a channel are presented in the [SDK Quickstart](https://docs.agora.io/en/video-calling/get-started/get-started-sdk?platform=unity) project for Video Calling. This document explains the common workflows you need to handle in your app.

## Understand the code

For context on this sample, and a full explanation of the essential code snippets used in this project, read the **Screen share, volume control and mute** document for your product of interest:

* [Video calling](https://docs.agora.io/en/video-calling/develop/product-workflow?platform=unity)
* [Voice calling](https://docs.agora.io/en/voice-calling/develop/product-workflow?platform=unity)
* [Interactive live Streaming](https://docs.agora.io/en/interactive-live-streaming/develop/product-workflow?platform=unity)
* [Broadcast streaming](https://docs.agora.io/en/broadcast-streaming/develop/product-workflow?platform=unity)

For the UI implementation of this example, refer to [`ProductWorkflow.cs`](./productWorkflow.cs).

## How to run this project

To see how to run this project, read the instructions in the main [README](../../README.md) or [SDK Quickstart](https://docs.agora.io/en/video-calling/get-started/get-started-sdk).


