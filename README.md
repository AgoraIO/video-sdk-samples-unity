# Video SDK for Unity code examples POC

Each folder contains the runnable code explained in the documentation. 

  Advantages are that we supply runnable code where the UI is abstracted so we concentrate more clearly on Agora
     SDK. This means the docs become much shorter and simpler. Possible disadvantage is that we have to write the
     code for the project. TBH, we already have the code, we are just putting it in a better format for learning.

  - [SDK Quickstart](./Assets/Get_started/)
  - [Call Quality](./Assets/Ensure_call_quality/)
  - [Secure Authentication with Tokens](./Assets/Authentication_workflow/)
  - [Cloud Proxy](./Assets/Cloud_proxy/)
  - [Secure Media Encryption](./Assets/Media_stream_encryption/)

## Run a sample project

To run a sample project in this repository, take the following steps:

1. Clone this Git repository by executing the following command in a terminal window:

    ```bash
    git clone https://github.com/AgoraIO/video-sdk-samples-unity
    ```

1. Follow [Integrate the video SDK](https://docs.agora.io/en/video-calling/get-started/get-started-sdk?platform=unity#project-setup) steps in order to add the video SDK into your project.


1. Go to [config.js](./Assets//AgoraManager/config.json) and add `appID`, `channelName`, and `token` from the agora console.

1. In **Unity Editor**, click **Play** and select a sample code from the dropdown that you wish to execute.


