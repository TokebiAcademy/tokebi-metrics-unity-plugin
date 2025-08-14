# Tokebi Metrics Unity Plugin

An analytics SDK for Unity games to connect with Tokebi Metrics backend.

## Features
- Event tracking
- Funnel analysis
- User segmentation
- Multiplayer support
- Cross-platform compatibility

## Requirements
- Unity 2019.4 or higher
- Tokebi Metrics account [https://tokebimetrics.com]

## Installation
# Tokebi Unity Plugin — Quick Setup Steps

1. **Download Installer**  
   Get the installer script from:  
   https://tokebimetrics.com/documentation-guide/unity-plugin-guide or use this repo

2. **Add Installer to Project**  
   Copy the file `TokebiInstaller.cs` into the `Assets/Editor/` folder of your Unity project.

3. **Run Installer**  
   In Unity, go to **Tools → Install Tokebi Analytics SDK**. Enter your API key and click "Install SDK".

4. **Add to Scene**  
   Go to **GameObject → Tokebi → Create Analytics SDK** to add the analytics system to your scene.

5. **Start Tracking Events**  
   In your game scripts, track analytics events with:

   ```csharp
   TokebiSDK.Instance.Track("event_name", new Dictionary<string, object>
   {
       ["key"] = "value"
   });
   ```

## License
MIT License. See LICENSE file.

## Documentation
More detailed docs at — [https://tokebimetrics.com/documentation-guide/unity-plugin-guide]

## Contact
Created by Marco Diaz — [https://tokebimetrics.com]
