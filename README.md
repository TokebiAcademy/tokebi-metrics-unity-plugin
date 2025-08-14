Tokebi Metrics Unity Plugin
An analytics SDK for Unity games to connect with Tokebi Metrics backend. Get powerful game analytics with minimal setup.

🚀 Features

Real-time Event Tracking - Track player actions, game events, and custom metrics
Funnel Analysis - Understand player progression and conversion rates
User Segmentation - Analyze player behavior patterns and retention
Multiplayer Support - Smart client/server event handling to prevent duplicates
Cross-platform - Works on PC, Mobile, Console, and WebGL builds
Privacy-focused - Anonymous player IDs with no personal data collection

📋 Requirements

Unity 2019.4 or higher
Tokebi Metrics account: [https://tokebimetrics.com]
.NET Framework 4.x or .NET Standard 2.1

🔧 Installation
Quick Setup Steps

Download Installer
Get the installer script from this repository or:
https://tokebimetrics.com/documentation-guide/unity-plugin-guide
Add to Unity Project
Copy TokebiInstaller.cs to your Assets/Editor/ folder in your Unity project.
Run Installer
In Unity, go to Tools → Install Tokebi Analytics SDK.
Enter your API key and click "Install SDK".
Add to Scene
Go to GameObject → Tokebi → Create Analytics SDK to add the analytics system to your scene.
Start Tracking
Use the SDK in your C# scripts:
csharpTokebiSDK.Instance.Track("level_completed", new Dictionary<string, object>
{
    ["level"] = 1,
    ["time"] = 45.6f,
    ["score"] = 1250
});


💻 Usage Examples
Basic Event Tracking
csharp// Simple event
TokebiSDK.Instance.Track("game_started");

// Event with data
TokebiSDK.Instance.Track("player_died", new Dictionary<string, object>
{
    ["cause"] = "enemy_attack",
    ["level"] = playerLevel,
    ["score"] = currentScore
});
Convenience Methods
csharp// Level progression
TokebiSDK.Instance.TrackLevelStart("level_1");
TokebiSDK.Instance.TrackLevelComplete("level_1", 30.5f);
Multiplayer Games
csharp// Configure multiplayer mode
public class NetworkManager : MonoBehaviour 
{
    void OnServerStarted()
    {
        // Host/Server tracks events
        TokebiSDK.Instance.SetMultiplayerMode(isClient: false);
    }
    
    void OnClientConnected()
    {
        // Clients don't track (prevents duplicates)
        TokebiSDK.Instance.SetMultiplayerMode(isClient: true);
    }
}

// Force client events when needed
TokebiSDK.Instance.TrackClientEvent("ui_button_clicked", payload);
Advanced Usage
csharp// Custom analytics manager
public class GameAnalytics : MonoBehaviour
{
    public void TrackPurchase(string item, float price)
    {
        TokebiSDK.Instance.Track("item_purchased", new Dictionary<string, object>
        {
            ["item_name"] = item,
            ["cost"] = price,
            ["currency"] = "USD",
            ["player_level"] = GetPlayerLevel()
        });
    }
    
    public void TrackSessionEnd(float duration)
    {
        TokebiSDK.Instance.Track("session_ended", new Dictionary<string, object>
        {
            ["duration_seconds"] = duration,
            ["levels_completed"] = GetLevelsCompleted(),
            ["total_score"] = GetTotalScore()
        });
    }
}
🎮 Supported Platforms

PC/Mac/Linux (Standalone builds)
Mobile (iOS/Android)
Console (PlayStation, Xbox, Nintendo Switch)
WebGL (Browser games)
Editor (Play mode testing)

🛠️ What Gets Installed
The installer automatically generates:

Assets/Tokebi/TokebiSDK.cs - Main analytics SDK with your API key
Assets/Tokebi/Editor/TokebiMenu.cs - Unity editor integration and testing tools
Complete C# codebase - No external dependencies required

🔒 Privacy & Security

Anonymous Player IDs - Unique per installation, no personal data
Local Data Storage - Player IDs stored in Application.persistentDataPath
HTTPS Encryption - All data transmitted securely
GDPR Compliant - No cookies, no personal information collected

🧪 Testing & Debugging
The SDK includes built-in testing tools:

In Play Mode: Go to Tools → Tokebi → Test Analytics
Check Console: Look for [Tokebi] log messages
Verify Dashboard: Check your Tokebi dashboard for incoming events

Console output example:
[Tokebi] Initializing SDK...
[Tokebi] Player ID: player_1692123456_7834
[Tokebi] Game registered! ID: abc123
[Tokebi] Tracking: level_completed PlayerId: player_1692123456_7834
📚 Documentation

Complete Setup Guide: Unity Plugin Documentation
API Reference: Tokebi Documentation
Dashboard Guide: Analytics Dashboard
Funnel Analytics: Advanced Analytics Features

🤝 Contributing

Fork the repository
Create a feature branch (git checkout -b feature/amazing-feature)
Commit your changes (git commit -m 'Add amazing feature')
Push to the branch (git push origin feature/amazing-feature)
Open a Pull Request

📄 License
This project is licensed under the MIT License - see the LICENSE file for details.
🐛 Issues & Support

Bug Reports: GitHub Issues
Feature Requests: GitHub Discussions
General Support: Contact Us
Documentation: tokebimetrics.com

🌟 Why Tokebi?
Built by former Roblox engineers who understand the need for simple, powerful analytics tools. Tokebi helps indie developers make better games through data-driven insights.
Key Benefits:

✅ 5-minute setup - Get analytics running immediately
✅ Free forever - No usage limits or hidden fees
✅ Privacy-first - Anonymous analytics, GDPR compliant
✅ Developer-friendly - Built by game developers, for game developers

📞 Contact
Website: [https://tokebimetrics.com]
Documentation: [https://tokebimetrics.com/documentation-guide]
Support: [Contact Form]
