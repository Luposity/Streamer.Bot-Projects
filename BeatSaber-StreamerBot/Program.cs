using Streamer.bot.Plugin.Interface;
using Streamer.bot.Plugin.Interface.Enums;
using Streamer.bot.Plugin.Interface.Model;
using Streamer.bot.Common.Events;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Text.Json;
using Newtonsoft.Json;
using System.Linq;

public class CPHInline : CPHInlineBase
{
    // Define and store a Cached Integer of Misses.
    private int localMissCache = 0;
    
    // Define VNyan WebSocket Index.
    private int VNyanWebSocket = 0;
    
    // Define if currently in level.
    private bool inLevel = false;
    
    // Define Health of Player.
    private double health = 0.0;
    
    public bool Execute()
    {
		CPH.TryGetArg("wsName", out string wsName);
		switch(wsName) {
			case "[BeatSaber] DataPuller | LiveData":
				DataPullerLiveData();
				break;
			case "[BeatSaber] DataPuller | Map Data":
				DataPullerMapData();
				break;
		}
		return true;
    }
    
    public bool DataPullerMapData()
	{
		if (!CPH.TryGetArg("message", out string wsData) || string.IsNullOrEmpty(wsData))
        {
            CPH.LogDebug("No WebSocket data received.");
            return false;
        }
        
        MapData DataPullerMap = JsonConvert.DeserializeObject<MapData>(wsData);
        
        if (DataPullerMap.InLevel && !inLevel && DataPullerMap.BSRKey != null) {
        	CPH.WebsocketSend("BSInSongCam", VNyanWebSocket);
        	CPH.SetGlobalVar("inLevel", true, false);
        	inLevel = true;
			return true;
        }

        // Level Finished, Quit or Failed.
        if (DataPullerMap.LevelFinished || DataPullerMap.LevelFailed || DataPullerMap.LevelQuit && inLevel) {
			CPH.WebsocketSend("BSMenuCam", VNyanWebSocket);
			if (health > 0.0) {
                CPH.WebsocketSend("BSYay", VNyanWebSocket);
            }
			CPH.SetGlobalVar("inLevel", false, false);
			inLevel = false;
			return true;
		}
		
		return true;
	}
	
    public bool DataPullerLiveData()
    {
        // Define VNyan WebSocket Index.
        // Check WebSocket Data Exists or is Null.
        if (!CPH.TryGetArg("message", out string wsData) || string.IsNullOrEmpty(wsData))
        {
            CPH.LogInfo("[DataPuller] No WebSocket data received.");
            return false;
        }

        // Check VNyan WebSocket is Connected.
        if (!CPH.WebsocketIsConnected(VNyanWebSocket))
        {
            CPH.LogWarn("[DataPuller: LiveData] - VNyan Websocket is not connected, is it turned on?");
            return false;
        }

        // Deserialize WebSocket JSON Data 
        LiveData DataPullerLive = JsonConvert.DeserializeObject<LiveData>(wsData);
        
        // Define Misses
        int misses = DataPullerLive.Misses;
        if (localMissCache != 0 && misses == 0)
        {
            localMissCache = 0;
        }
		
		// Update Health Variable to Players Current Health.
		health = DataPullerLive.PlayerHealth;
		
        // Throw Beat Saber Block in VNyan on Miss
        // Will only throw if player is alive.
        if (misses > localMissCache && DataPullerLive.PlayerHealth > 0.0)
        {
            localMissCache = misses;
            CPH.WebsocketSend("DPNoteMissed " + localMissCache, VNyanWebSocket);
        }

        // Set Combo Multiplier to Trigger Expression
        int expressOn = 250;
        bool comboExpression = DataPullerLive.Combo % expressOn == 0;
        
        // Make avatar Express on Specified Multiple of Combo
        if (comboExpression && DataPullerLive.Combo != 0)
        {
            CPH.WebsocketSend("ComboExpression", VNyanWebSocket);
        }

        return true;
    }
}

public class BlockHitScore
{
    public int PreSwing { get; set; }
    public int PostSwing { get; set; }
    public int CenterSwing { get; set; }
}

public class LiveData
{
    public int Score { get; set; }
    public int ScoreWithMultipliers { get; set; }
    public int MaxScore { get; set; }
    public int MaxScoreWithMultipliers { get; set; }
    public string Rank { get; set; }
    public bool FullCombo { get; set; }
    public int NotesSpawned { get; set; }
    public int Combo { get; set; }
    public int Misses { get; set; }
    public double Accuracy { get; set; }
    public BlockHitScore BlockHitScore { get; set; }
    public double PlayerHealth { get; set; }
    public int ColorType { get; set; }
    public int CutDirection { get; set; }
    public int TimeElapsed { get; set; }
    public int EventTrigger { get; set; }
    public long UnixTimestamp { get; set; }
}

public class ColorScheme
{
    public SaberAColor SaberAColor { get; set; }
    public SaberBColor SaberBColor { get; set; }
    public ObstaclesColor ObstaclesColor { get; set; }
    public EnvironmentColor0 EnvironmentColor0 { get; set; }
    public EnvironmentColor1 EnvironmentColor1 { get; set; }
    public EnvironmentColor0Boost EnvironmentColor0Boost { get; set; }
    public EnvironmentColor1Boost EnvironmentColor1Boost { get; set; }
}

public class EnvironmentColor0
{
    public string HexCode { get; set; }
    public int Red { get; set; }
    public int Green { get; set; }
    public int Blue { get; set; }
    public double Alpha { get; set; }
}

public class EnvironmentColor0Boost
{
    public string HexCode { get; set; }
    public int Red { get; set; }
    public int Green { get; set; }
    public int Blue { get; set; }
    public double Alpha { get; set; }
}

public class EnvironmentColor1
{
    public string HexCode { get; set; }
    public int Red { get; set; }
    public int Green { get; set; }
    public int Blue { get; set; }
    public double Alpha { get; set; }
}

public class EnvironmentColor1Boost
{
    public string HexCode { get; set; }
    public int Red { get; set; }
    public int Green { get; set; }
    public int Blue { get; set; }
    public double Alpha { get; set; }
}

public class Modifiers
{
    public bool NoFailOn0Energy { get; set; }
    public bool OneLife { get; set; }
    public bool FourLives { get; set; }
    public bool NoBombs { get; set; }
    public bool NoWalls { get; set; }
    public bool NoArrows { get; set; }
    public bool GhostNotes { get; set; }
    public bool DisappearingArrows { get; set; }
    public bool SmallNotes { get; set; }
    public bool ProMode { get; set; }
    public bool StrictAngles { get; set; }
    public bool ZenMode { get; set; }
    public bool SlowerSong { get; set; }
    public bool FasterSong { get; set; }
    public bool SuperFastSong { get; set; }
}

public class ObstaclesColor
{
    public string HexCode { get; set; }
    public int Red { get; set; }
    public int Green { get; set; }
    public int Blue { get; set; }
    public double Alpha { get; set; }
}

public class PracticeModeModifiers
{
    public double SongSpeedMul { get; set; }
    public bool StartInAdvanceAndClearNotes { get; set; }
    public double SongStartTime { get; set; }
}

public class RankedState
{
    public bool Ranked { get; set; }
    public bool Qualified { get; set; }
    public bool BeatleaderQualified { get; set; }
    public bool ScoresaberQualified { get; set; }
    public bool BeatleaderRanked { get; set; }
    public bool ScoresaberRanked { get; set; }
    public double BeatleaderStars { get; set; }
    public double ScoresaberStars { get; set; }
}

public class MapData
{
    public string GameVersion { get; set; }
    public string PluginVersion { get; set; }
    public bool InLevel { get; set; }
    public bool LevelPaused { get; set; }
    public bool LevelFinished { get; set; }
    public bool LevelFailed { get; set; }
    public bool LevelQuit { get; set; }
    public string Hash { get; set; }
    public object LevelID { get; set; }
    public string SongName { get; set; }
    public string SongSubName { get; set; }
    public string SongAuthor { get; set; }
    public string Mapper { get; set; }
    public List<string> Mappers { get; set; }
    public List<object> Lighters { get; set; }
    public string ContentRating { get; set; }
    public object BSRKey { get; set; }
    public object CoverImage { get; set; }
    public int Duration { get; set; }
    public string MapType { get; set; }
    public string Environment { get; set; }
    public string Difficulty { get; set; }
    public string CustomDifficultyLabel { get; set; }
    public int BPM { get; set; }
    public double NJS { get; set; }
    public Modifiers Modifiers { get; set; }
    public double ModifiersMultiplier { get; set; }
    public bool PracticeMode { get; set; }
    public PracticeModeModifiers PracticeModeModifiers { get; set; }
    public double PP { get; set; }
    public double Star { get; set; }
    public RankedState RankedState { get; set; }
    public double Rating { get; set; }
    public ColorScheme ColorScheme { get; set; }
    public bool IsMultiplayer { get; set; }
    public int MultiplayerLobbyMaxSize { get; set; }
    public int MultiplayerLobbyCurrentSize { get; set; }
    public int PreviousRecord { get; set; }
    public object PreviousBSR { get; set; }
    public long UnixTimestamp { get; set; }
}

public class SaberAColor
{
    public string HexCode { get; set; }
    public int Red { get; set; }
    public int Green { get; set; }
    public int Blue { get; set; }
    public double Alpha { get; set; }
}

public class SaberBColor
{
    public string HexCode { get; set; }
    public int Red { get; set; }
    public int Green { get; set; }
    public int Blue { get; set; }
    public double Alpha { get; set; }
}