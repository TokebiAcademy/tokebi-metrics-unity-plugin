# Tokebi Metrics Unity Plugin

An analytics SDK for Unity games to connect with Tokebi Metrics backend.

## 🚀 Features

- **Event tracking**
- **Funnel analysis** 
- **User segmentation**
- **Multiplayer support**
- **Cross-platform compatibility**

### v2.0 Performance Features
- **30-second batching** (like Unity Analytics)
- **Minimal memory allocations** 
- **Object pooling** for network requests
- **Console/mobile optimized**
- **Automatic player tracking**

## 📋 Requirements

- **Unity 2019.4 or higher** (recommended: Unity 2020.3+)
- **Tokebi Metrics account** - [https://tokebimetrics.com]

## 📦 Installation

### Tokebi Unity Plugin — Setup Methods

#### Method 1: Package Manager (Recommended - v2.0 Batched)

1. Open Unity → **Window → Package Manager**
2. Click **"+" → "Add package from git URL"**
3. Enter: `https://github.com/TokebiAcademy/tokebi-metrics-unity-plugin.git`
4. Unity installs automatically
5. **Tools → Tokebi → Setup Analytics** to configure

**✅ Gets you:** v2.0 batched SDK (production-ready)

#### Method 2: Manual Installation (v2.0 Batched)

1. **Download** this repository as ZIP or clone it
2. **Create folders** in Unity: `Assets/Tokebi/` and `Assets/Tokebi/Editor/`
3. **Copy files**:
   ```
   Runtime/TokebiSDK.cs → Assets/Tokebi/TokebiSDK.cs
   Runtime/TokebiSDK.Runtime.asmdef → Assets/Tokebi/TokebiSDK.Runtime.asmdef
   Editor/TokebiInstaller.cs → Assets/Tokebi/Editor/TokebiInstaller.cs  
   Editor/TokebiMenu.cs → Assets/Tokebi/Editor/TokebiMenu.cs
   Editor/TokebiSDK.Editor.asmdef → Assets/Tokebi/Editor/TokebiSDK.Editor.asmdef
   ```
4. **GameObject → Tokebi → Create Analytics SDK** and set API key

**✅ Gets you:** v2.0 batched SDK (production-ready)

#### Method 3: Via manifest.json (v2.0 Batched)

Add to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.tokebi.analytics": "https://github.com/TokebiAcademy/tokebi-metrics-unity-plugin.git#v2.0.0"
  }
}
```

**✅ Gets you:** v2.0 batched SDK (production-ready)

## 🔧 Setup

### Quick Setup

1. **Tools → Tokebi → Setup Analytics**
2. Enter your API key
3. Click "Create Analytics GameObject"
4. Done!

### Manual Setup

1. Create empty GameObject in your scene
2. Add `TokebiSDK` component
3. Set your API key in the inspector
4. Done!

## 📈 Usage

### Start Tracking Events

In your game scripts, track analytics events with:

```csharp
TokebiSDK.Instance.Track("event_name", new Dictionary<string, object>
{
    ["key"] = "value"
});
```

### Basic Tracking Examples

```csharp
// Track any event with custom data
TokebiSDK.Instance.Track("player_action", new Dictionary<string, object> {
    ["action"] = "jump",
    ["level"] = "1-1",
    ["position_x"] = 10.5f
});
```

### Convenience Methods

```csharp
// Level tracking
TokebiSDK.Instance.TrackLevelStart("level-1");
TokebiSDK.Instance.TrackLevelComplete("level-1", 45.2f);
```

### Multiplayer Games

```csharp
// Only track on host/server, not clients
TokebiSDK.Instance.SetMultiplayerMode(isClient: false); // Host
TokebiSDK.Instance.SetMultiplayerMode(isClient: true);  // Client (won't track)
```

## 🔧 Editor Tools

- **Tools → Tokebi → Setup Analytics** - Quick setup wizard
- **Tools → Tokebi → Test Analytics** - Send test event
- **Tools → Tokebi → Force Flush Events** - Send queued events immediately
- **GameObject → Tokebi → Create Analytics SDK** - Add to scene

## ⚡ Performance

This SDK is optimized for high-frequency tracking:

- **Events are batched** every 60 seconds (configurable)
- **Object pooling** minimizes garbage collection
- **String caching** reduces allocations
- **Bulk database inserts** on the server

Perfect for tracking hundreds of events per minute without performance impact.

## 🎮 Example Integration

```csharp
public class PlayerController : MonoBehaviour 
{
    private void Start() 
    {
        TokebiSDK.Instance.TrackLevelStart(SceneManager.GetActiveScene().name);
    }
    
    private void OnJump() 
    {
        TokebiSDK.Instance.Track("player_jump", new Dictionary<string, object> {
            ["level"] = SceneManager.GetActiveScene().name,
            ["player_position"] = transform.position.ToString(),
            ["timestamp"] = Time.time
        });
    }
    
    private void OnLevelComplete() 
    {
        TokebiSDK.Instance.TrackLevelComplete(
            SceneManager.GetActiveScene().name, 
            Time.timeSinceLevelLoad
        );
    }
}
```

## 🏗️ API Compatibility

Works with both:
- **Single events**: `{"eventType": "jump", "playerId": "123", ...}`
- **Batch events**: `{"events": [...]}`

Your API automatically detects the format.

## 📁 Repository Structure

```
tokebi-metrics-unity-plugin/
├── package.json                    # Unity package manifest
├── README.md                       # This file
├── CHANGELOG.md                    # Version history
├── LICENSE                         # MIT license
├── Runtime/                        # Runtime scripts
│   ├── TokebiSDK.cs               # Main SDK with batching
│   └── TokebiSDK.Runtime.asmdef   # Runtime assembly definition
└── Editor/                         # Editor-only scripts
    ├── TokebiInstaller.cs         # Setup wizard
    ├── TokebiMenu.cs              # Editor menu items
    └── TokebiSDK.Editor.asmdef    # Editor assembly definition
```

## 🔄 Changelog

### v2.0.0
- Added batched event tracking (60-second intervals)
- Object pooling for network requests
- String caching to minimize allocations
- 95%+ reduction in GC pressure
- Up to 3600x fewer network requests

### v1.0.0
- Initial release with basic event tracking

## 📄 License

MIT License - see LICENSE file for details.

## 📚 Documentation

More detailed docs at — [https://tokebimetrics.com/documentation-guide/unity-plugin-guide]

## 🤝 Contact & Support

**Created by Marco Diaz** — [https://tokebimetrics.com]

- **Issues**: [GitHub Issues](https://github.com/TokebiAcademy/tokebi-metrics-unity-plugin/issues)
- **Documentation**: [Unity Plugin Guide](https://tokebimetrics.com/documentation-guide/unity-plugin-guide)
- **Website**: [https://tokebimetrics.com]
