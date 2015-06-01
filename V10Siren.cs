using ColossalFramework.IO;
using ColossalFramework.Steamworks;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using V10CoreUtils;

namespace V10Siren
{
	public class V10Siren : V10CoreMod
	{
		private string _name = "V10Siren";
		private string _version = "0.5";
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
	
/*	public class V10SirenLoadingListener : LoadingExtensionBase
	{
		private V10SirenAudioOptions options;
		
		public override void OnLevelLoaded (LoadMode mode)
		{
			
			if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame)
				return;
			
			try {
				GameObject gameObject = GameObject.FindWithTag ("GameController");
				this.options = gameObject.AddComponent<V10SirenAudioOptions> ();
			} catch (Exception e) {
				Utils.Log (e, "initializing audio options", true);
				return;
			}
		}
	}*/
	
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
			bool policeInjected = policeClip == null;
			bool firetruckInjected = firetruckClip == null;
			
			if (policeInjected && firetruckInjected)
				return;
			
			HashSet<VehicleInfo> infoCache = new HashSet<VehicleInfo> ();
			bool stop, realStop = false, police;
			foreach (Vehicle vehicle in VehicleManager.instance.m_vehicles.m_buffer) {
				if (infoCache.Contains (vehicle.Info))
					continue;
				infoCache.Add (vehicle.Info);
				stop = false;
				SoundEffect soundEffect;
				foreach (VehicleInfo.Effect effect in vehicle.Info.m_effects) {
					if (!effect.m_effect.name.Contains ("Emergency"))
						continue;
					foreach (MultiEffect.SubEffect subEffect in effect.m_effect.GetComponent<MultiEffect>().m_effects) {
						if (subEffect.m_effect.name == ("Police Car Siren")) {
							police = true;
						} else if (subEffect.m_effect.name == ("Ambulance Siren")) {
							police = false;
						} else
							continue;
						
						soundEffect = subEffect.m_effect.GetComponent<SoundEffect> ();
						if (soundEffect == null)
							continue;
						
						if(police) {
							soundEffect.m_audioInfo.m_clip = policeClip;
							policeClip = null;
						} else {
							soundEffect.m_audioInfo.m_clip = firetruckClip;
							firetruckClip = null;
						}
						soundEffect.m_audioInfo.m_loop = true;
						soundEffect.m_audioInfo.m_pitch = 1;
						stop = true;
						break;
						
					}
					if (stop) {
						if (policeInjected && firetruckInjected)
							realStop = true;
						break;
					}
				}
				if (realStop)
					break;
			}
		}
	}
}
