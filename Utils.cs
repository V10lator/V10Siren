/*
 * This file is part of V10Siren.
 *
 * Foobar is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as
 * published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version.
 *
 * V10Siren is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General
 * Public License along with V10Siren.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

/*
 * Don't copy this file to your mod!
 * It might get splitted into a library later but right now
 * copying it is a save way to end the world as we know it.
 */
using ColossalFramework.IO;
using ColossalFramework.Plugins;
using ColossalFramework.Steamworks;
using ICities;
using System;
using System.IO;
using UnityEngine;

namespace V10CoreUtils
{
	public class Utils
	{
		private static IUserMod mod;
		private static bool _ready = false;
		private static string _version = "0.1";
		
		public static void init (IUserMod mod)
		{
			Utils.mod = mod;
			Utils._ready = true;
		}
		
		public static bool ready {
			get {
				return Utils._ready;
			}
		}
		
		public static string version {
			get {
				return Utils._version;
			}
		}
		
		private static string[] splitVersion ()
		{
			return Utils.version.Split ('.');
		}
		
		public static int majorVersion {
			get {
				return int.Parse (Utils.splitVersion() [0]);
			}
		}
		
		public static int minorVersion {
			get {
				return int.Parse (Utils.splitVersion() [1]);
			}
		}
		
		public static void Log (Exception e, bool disable)
		{
			Utils.Log (e, "doing unknown things", disable);
		}
		
		public static void Log (Exception e, string action, bool disable)
		{
			Utils.Log (e.GetType ().Name + " while " + action + ": " + e.Message + "\n" + e.StackTrace, disable);
		}
		
		public static void Log (string text, bool disable)
		{
			text = text.Replace ("\r\n", "\n"); // DOS2UNIX
			string[] lines = text.Split ('\n');
			string[] newLines;
			int offset;
			if (lines.Length > 4) {
				newLines = new string[lines.Length + 2];
				newLines [0] = "\t---------------------------- SNIP HERE ----------------------------";
				newLines [newLines.Length - 1] = newLines [0];
				offset = 1;
			} else {
				offset = 0;
				newLines = new string[lines.Length];
			}
			for (int i = 0; i < lines.Length; i++)
				newLines [i + offset] = mod.Name + " | " + lines [i];
			Debug.Log (string.Join ("\n", newLines));
			
			if (disable)
				Utils.disable ();
		}
		public static bool disable ()
		{
			if (!Utils.ready)
				return false;
			Utils._ready = false;
			PluginManager.PluginInfo plugin = Utils.getPluginInfo ();
			if (plugin == null) {
				Utils.Log ("Couldn't disable!", false);
				return false;
			}
			plugin.Unload ();
			Utils.Log ("Disabled! Have a nice day.", false);
			return true;
		}
		
		public static string getFileInModFolder (string filename)
		{
			if (!Utils.ready)
				return null;
			
			// ... Mod folder first...
			PluginManager.PluginInfo plugin = getPluginInfo ();
			if (plugin != null && plugin.modPath != null) {
				string file = Path.Combine (plugin.modPath, filename);
				if (File.Exists (file))
					return file;
			}
			if (plugin == null)
				Utils.Log ("plugin == null", false);
			else if(plugin.modPath == null)
				Utils.Log ("plugin.modPath == null", false);
			return null;
			/*
			string path = Path.Combine (DataLocation.addonsPath, "Mods"); //addonsPath
			path = Path.Combine (path, mod.realName);
			string file = null;
			try {
				if (Directory.Exists (path)) {
					file = Path.Combine (path, filename);
					if (File.Exists (file))
						return file;
				}
			} catch (Exception) {
			}
			
			// ... Steam Workshop folder next...
			PluginManager.PluginInfo plugin = getPluginInfo ();
			if (plugin != null) {
				PublishedFileId steamID = plugin.publishedFileID;
				Utils.Log ("SteamID: " + steamID.AsUInt64, false);
				path = Steam.workshop.GetSubscribedItemPath (steamID);
				if (path != null) {
					Utils.Log ("Checking: " + path, false);
					file = Path.Combine (path, filename);
					if (File.Exists (file))
						return file;
					Utils.Log ("File doesn't exist!", false);
				} else
					Utils.Log ("path == null", false);
			} else
				Utils.Log ("plugin == null", false);
			Utils.Log ("File not found!", false);
			return null;*/
		}
		
		private static PluginManager.PluginInfo cachedPluginInfo = null;
		public static PluginManager.PluginInfo getPluginInfo ()
		{
			if (Utils.ready && cachedPluginInfo == null)
				foreach (PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo ()) {
					if (info == null)
						continue;
					if (mod == info.userModInstance) {
						cachedPluginInfo = info;
						break;
					}
				}
			
			return cachedPluginInfo;
		}
	}
}
