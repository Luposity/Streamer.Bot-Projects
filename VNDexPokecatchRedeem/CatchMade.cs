using Streamer.bot.Plugin.Interface;
using Streamer.bot.Plugin.Interface.Enums;
using Streamer.bot.Plugin.Interface.Model;
using Streamer.bot.Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;

public class CPHInline : CPHInlineBase
{
    public bool Execute()
    {
        // Check that WS data exists.
        if (!CPH.TryGetArg("data", out string wsData) || String.IsNullOrEmpty(wsData)) {
            CPH.LogDebug("[VNDex] WebSocket Data is Null or Empty."); 
            return false;
        }

        // Check data starts with VNDex Prefix
        if (!wsData.StartsWith("VNDex")) {
            CPH.LogInfo("[VNDex] Data does not start with Prefix 'VNDex'. Ignoring.");
            return false;
        }

        // Generate Random
        Random rand = new Random();

        // Get Custom Chance Rates. Default Value: 20 (20%)
        if (!CPH.TryGetArg("ShinyChance", out int ShinyChance)) {
            ShinyChance = 20;
        }

        // Get Custom Hype Train Shiny Chance. Default Value: 50 (50%)
        if (!CPH.TryGetArg("HypeTrainShinyChance", out int HypeTrainShinyChance)) {
            HypeTrainShinyChance = 50;
        }

        // Get if Escapes are allowed. Default Value: False.
        if (!CPH.TryGetArg("AllowEscapes", out bool AllowEscapes)) {
            AllowEscapes = false;
        }

        // Get Custom Escape Chance, Default to 5 (5%)
        if (!CPH.TryGetArg("EscapesChance", out int EscapeChance)) {
            EscapeChance = 5;
        }

        // If Shiny chance exceeds 100 then set to default value
        if (ShinyChance > 100) {
            CPH.LogDebug("[VNDex] Shiny Chance Exceeds limits, defaulting to 20");
            ShinyChance = 20;
        }

        // If Shiny chance exceeds 100 then set to default value
        if (HypeTrainShinyChance > 100) {
            CPH.LogDebug("[VNDex] Hype Train Shiny Chance Exceeds limits, defaulting to 50");
            HypeTrainShinyChance = 50;
        }

        // Convert Chance Rates into Double.
        double EscapeChanceDecimal = Convert.ToDouble(EscapeChance);
        double ShinyChanceDecimal = Convert.ToDouble(ShinyChance);
        double HypeTrainShinyChanceDecimal = Convert.ToDouble(HypeTrainShinyChance);

        // Get User Variables, based on UserId.
        string userId = wsData.Replace("VNDex", "").Replace(" ", "").TrimEnd();
        int userAttemptedCatches = CPH.GetTwitchUserVarById<int>(userId, "attemptedPokeCatch", true);
        int userCatchAmount = CPH.GetTwitchUserVarById<int>(userId, "userpokeballCatches", true);
        int userShinyCatchAmount = CPH.GetTwitchUserVarById<int>(userId, "userpokeballCatchesShiny", true);

        return true;
    }

    public bool CheckWSSConnection()
    {
        int VNDexWS = CPH.WebsocketCustomServerGetConnectionByName("VN-Dex - WSS");
        if (!CPH.WebsocketCustomServerIsListening(VNDexWS)){
            CPH.LogError("[VNDex] WebSocket Server Isn't Listening, Attempting to Start WebSocket Server...");
            try {
                CPH.WebsocketCustomServerStart(VNDexWS);
                return true;
            } catch {
                CPH.LogError("[VNDex] Failed to Start VNDex WSS, Check the Port isn't being used by another Application.");
                return false;
            }
            
        }
        return true;
    }
    
    public bool CheckVNConnection()
    {
        int VNyanWSNumber = CPH.GetGlobalVar<int>("VNDex - VNyan WebSocket Number", true);
        if (!CPH.WebsocketIsConnected(VNyanWSNumber)){
            CPH.LogError("[VNDex] StreamerBot isn't connected to VNyan, Attempting to connect...");
            try {
                CPH.WebsocketConnect(VNyanWSNumber);
                return true;
            } catch {
                CPH.LogError("[VNDex] Failed to Start VNDex WSS, Check the Port isn't being used by another Application.");
                return false;
            }
            
        }
        return true;
    }
}