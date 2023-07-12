# Config

This README provides information about the configuration file [`config.json`](config.json) used in the project. The file contains the following structure:

```json
{
  "appID": "",
  "channelName": "",
  "token": "",
  "tokenUrl": "", 
  "tokenExpiryTime": 3600,
  "uid": 0,
  "encryptionKey" : "",
  "salt" : ""
}
```


### Properties

- `appID`: The unique ID for the application obtained from https://console.agora.io.
- `channelName`: The pre-filled text for the channel to join.
- `token`: The RTC (Real-Time Communication) token generated for authentication.
- `tokenUrl`: The URL for the token generator (no trailing slash).
- `tokenExpiryTime`: The expiry time (in seconds) for token generated from the token generator.
- `uid`: The user ID associated with the application.
- `encryptionKey`: The encryption key used for RTC encryption.
- `salt`: The salt used for RTC encryption.


The configuration data is loaded from the `config.json` into `ConfigData` using the following function:

```js
public class ConfigData
{
    public string appID;
    public string channelName;
    public string token;
    public string encryptionKey = "";
    public string salt = "";
    public int tokenExpiryTime = 3600; // Default time of 1 hour
    public string tokenUrl = ""; // Add Token Generator URL ...
    public int uid  = 0; // RTC elected user ID (0 = choose random)
}
private void LoadConfigFromJSON()
{
    string path = System.IO.Path.Combine(Application.dataPath, "AgoraManager", "config.json");
    if (File.Exists(path))
    {
        string json = File.ReadAllText(path);
        configData = JsonUtility.FromJson<ConfigData>(json);

        _appID = configData.appID;
        _channelName = configData.channelName;
        _token = configData.token;
    }
    else
    {
        Debug.LogError("Config file not found!");
    }
}
```

Please ensure that the [`config.json`](config.json) file is correctly populated with the required values before running the application.
