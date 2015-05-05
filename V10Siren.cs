using ColossalFramework.IO;
using ColossalFramework.Steamworks;
using ICities;
using System;
using System.IO;
using System.Threading;
using UnityEngine;
using V10CoreUtils;

namespace V10Siren
{
	public class V10Siren : V10CoreMod
	{
		private string _name = "V10Siren";
		private string _version = "0.4";
		private string[] _descriptions = { "Better horns",
		"TATUUUUUU - TATAAAAA",
		"Is this still a game?",
		"(c) 2015 Thomas \"V10lator\" Rohloff"};
		private System.Random rand = new System.Random();
		
		public V10Siren ()
		{
			Utils.init (this);
		}
		
		public string realName
		{
			get { return this._name; }
		}
		
		public string streamID
		{
			get { return "435167188"; }
		}
		
		public string Name {
			get { return this._name + " v" + this._version; }
		}
		
		public string Description
		{
			get { return this._descriptions[this.rand.Next(this._descriptions.Length)]; }
		}
	}
	
	public class V10SirenLoadingListener : LoadingExtensionBase
	{
		public override void OnLevelLoaded (LoadMode mode)
		{
			/*
			if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame)
				return;
			
			// Search our ogg file...
			// ... Mod folder first...
			string ogg = null;
			string toFind = Path.DirectorySeparatorChar + "V10Siren";
			string path = Path.Combine (DataLocation.addonsPath, "Mods"); //addonsPath
			try {
				if (Directory.Exists (path)) {
					string[] dirs = Directory.GetDirectories (path);
					foreach (string dir in dirs) {
						if (dir.EndsWith (toFind)) {
							ogg = Path.Combine (dir, "V10Siren.ogg");
							if (!File.Exists (ogg))
								ogg = null;
							else
								break;
						}
					}
				}
			} catch (Exception) {
			}
			
			// ... Steam Workshop folder next...
			if (ogg == null) {
				PublishedFileId[] subscribedItems = Steam.workshop.GetSubscribedItems ();
				foreach (PublishedFileId id in subscribedItems) {
					path = Steam.workshop.GetSubscribedItemPath (id);
					ogg = Path.Combine (path, "V10Siren.ogg");
					if (!File.Exists (ogg))
						ogg = null;
					else
						break;
				}
			}
			
			if (ogg == null) { // File couldn't be found!
				V10Siren.Log ("Couldn't find ogg file!", true);
				return;
			}
			ogg = "file://" + ogg;
			
			AudioClip clip;
			try {
				WWW www = new WWW (ogg);
				while (!www.isDone)
					Thread.Sleep (2);
				clip = www.GetAudioClip (true, false);
				clip.LoadAudioData ();
				while (clip.loadState != AudioDataLoadState.Loaded) {
					if (clip.loadState == AudioDataLoadState.Failed) {
						V10Siren.Log("Loading audio data failed!", true);
						return;
					}
					Thread.Sleep (2);
				}
			} catch (Exception e) {
				V10Siren.Log (e, "loading ogg file", true);
				return;
			}
			
			float[] data = new float[clip.channels * clip.samples], tmpData;
			clip.GetData (data, 0);
			AudioClip[] clips = Resources.FindObjectsOfTypeAll<AudioClip> ();
			foreach (AudioClip cl in clips) {
				if (cl.name == "siren" || cl.name == "fire_sirens") {
					tmpData = new float[cl.channels * cl.samples];
					if (data.Length > tmpData.Length)
						V10Siren.Log (
							"V10Siren.cfg bigger than "+cl.name+" clip:\nV10Siren.cfg: "+data.Length+"\n"+cl.name+": "+tmpData.Length+"\nV10Siren.cfg gets cutted, this might soud bad!",
							false
							);
					for (int i = 0; i < tmpData.Length; i++) {
						if (i < data.Length)
							tmpData [i] = data [i];
						else
							tmpData [i] = 0.0f;
					}
					cl.SetData (tmpData, 0);
				}
			}
			*/
		}
	}
	
	public class V10SirenThreadingListener : ThreadingExtensionBase
	{
		private AudioClip policeClip = null, firetruckClip = null;
		
		public V10SirenThreadingListener ()
		{
			// Search our ogg file...
			loadOgg ("V10Siren.ogg", ref this.policeClip);
			loadOgg ("V10FireSiren.ogg", ref this.firetruckClip);
		}
		
		private void loadOgg (string file, ref AudioClip clip)
		{
			string ogg = Utils.getFileInModFolder (file);
			if (ogg == null) { // File couldn't be found!
				Utils.Log ("Couldn't find ogg file!", true);
				return;
			}
			ogg = "file://" + ogg;
			
			try {
				WWW www = new WWW (ogg);
				while (!www.isDone)
					Thread.Sleep (2);
				clip = www.GetAudioClip (true, false);
				clip.name = "V10Siren";
				clip.LoadAudioData ();
				while (clip.loadState != AudioDataLoadState.Loaded) {
					if (clip.loadState == AudioDataLoadState.Failed)
						throw new Exception ();
					Thread.Sleep (2);
				}
			} catch (Exception e) {
				Utils.Log (e, "loading ogg file", true);
			}
		}
		
		public override void OnUpdate (float realTimeDelta, float simulationTimeDelta)
		{
			AudioSource[] audioSources = GameObject.FindObjectsOfType<AudioSource> ();
			if (audioSources.Length == 0)
				return;
			bool playing, police;
			foreach (AudioSource source in audioSources) {
				if (source.clip == null || source.clip.name == null || source.clip.name == "V10Siren")
					continue;
				police = source.clip.name == "siren";
				if (!police && source.clip.name != "fire_sirens")
					continue;
				playing = source.isPlaying;
				if (playing)
					source.Stop ();
				if (police)
					source.clip = this.policeClip;
				else {
					source.clip = this.firetruckClip;
					source.pitch = 1;
				}
				source.rolloffMode = AudioRolloffMode.Logarithmic;
				source.loop = true;
				if (playing)
					source.Play ();
			}
		}
	}
}
