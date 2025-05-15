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
	public bool Execute()
	{
		// Get arguments
		CPH.TryGetArg("command", out string command);
		CPH.TryGetArg("rawInput", out string InputString);
		
		// Flipped Text Result
		string newText = TextFlip(InputString.ToLower().Replace("@", ""));
		
		// Switch Action based on Command
		switch(command) {
			case "!flip":
				if (String.IsNullOrEmpty(InputString)) {
					CPH.SendMessage("(╯°□°）╯︵ ┻━┻", true, true);
					return true;
				}
				else {
					CPH.SendMessage("(╯°□°）╯︵  "+newText.ToLower(), true, true);
					return true;
				}
				break;
			case "!unflip":
				if(String.IsNullOrEmpty(InputString)) {
					CPH.SendMessage($"┬─┬ ノ( ゜-゜ノ)", true, true);
					return true;
				} else {
					CPH.SendMessage($""+InputString.ToLower()+" ノ( ゜-゜ノ)", true, true);
					return true;
				}
				break;
			case "!angryflip":
				int mid = newText.Length / 2;
				string left = newText.Substring(0, mid);
				string right = newText.Substring(mid);
				CPH.SendMessage($@"{left} ︵ \(°□°)/ ︵ {right}", true, true);
				break;
		}
		return true;
	}
	
	// Characters to flip
	private string TextFlip(string InputString)
	{
	char[] X = @"¿/˙'\‾¡zʎxʍʌnʇsɹbdouɯlʞɾıɥƃɟǝpɔqɐ".ToCharArray();
	string V = @"?\.,/_!zyxwvutsrqponmlkjihgfedcba";
	
	// Return Flipped Text
	return new string((from char obj in InputString.ToCharArray()
			   select (V.IndexOf(obj) != -1) ? X[V.IndexOf(obj)] : obj).Reverse().ToArray());

	}
}
