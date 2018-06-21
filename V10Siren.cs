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

using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using UnityEngine;
using V10CoreUtils;

namespace V10Siren
{
	public class V10Siren : LoadingExtensionBase, IUserMod
	{
		private string _name = null;
		private string[] _descriptions = { "Better horns",
		"TATUUUUUU - TATAAAAA",
		"Is this still a game?",
		"(c) 2015 Thomas \"V10lator\" Rohloff"};
		private System.Random rand = new System.Random();
		private static bool _running = false;

		public V10Siren()
		{
			Utils.init(this);
		}
		
		public string Name {
			get {
				if (this._name == null) {
					AssemblyName me = Assembly.GetExecutingAssembly ().GetName ();
					this._name = me.Name + " v" + me.Version;
				}
				return this._name;
			}
		}
		
		public string Description
		{
			get { return this._descriptions[this.rand.Next(this._descriptions.Length)]; }
		}

		public static bool running
		{
			get
			{
				return V10Siren._running;
			}
		}

		public override void OnCreated(ILoading loading)
		{
			Utils.Log("Loading: " + loading.loadingComplete, false);
			if (Utils.getPlugin(818641631ul) != null)
			{
				V10Siren._running = false;
				Utils.Log("Ambient Sounds Tuner 2.0 detected. Disabling ourself!", true);
			}
			else
			{
				V10Siren._running = true;
				Utils.Log("Running in standalone mode!", false);
				V10SirenThreadingListener.loadOggs();
			}
		}

		public override void OnReleased()
		{
			V10Siren._running = false;
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
		private static AudioClip policeClip = null, ambulanceClip = null, firetruckClip = null;

		public static void loadOggs()
		{
			if (!V10Siren.running)
				return;
			loadOgg("V10Siren.ogg", ref policeClip);
			loadOgg("V10AmbulanceSiren.ogg", ref ambulanceClip);
			loadOgg("V10FireSiren.ogg", ref firetruckClip);
		}
		
		private static void loadOgg (string file, ref AudioClip clip)
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
				clip.name = "V10Siren "+ogg;
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
			if (!V10Siren.running)
				return;
			
			bool policeInjected = policeClip == null;
			bool ambulanceInjected = ambulanceClip == null;
			bool firetruckInjected = firetruckClip == null;
			
			if (policeInjected && ambulanceInjected && firetruckInjected)
				return;
			
			HashSet<VehicleInfo> infoCache = new HashSet<VehicleInfo> ();
			bool stop, realStop = false;
			AudioClip clip = null;
			SoundEffect soundEffect;
			MultiEffect multiEffect;
			foreach (Vehicle vehicle in VehicleManager.instance.m_vehicles.m_buffer) {
				if (vehicle.Info == null || infoCache.Contains (vehicle.Info))
					continue;
				infoCache.Add (vehicle.Info);
				if (vehicle.Info.m_effects == null)
					continue;
				stop = false;
				foreach (VehicleInfo.Effect effect in vehicle.Info.m_effects) {
					if (effect.m_effect == null || !effect.m_effect.name.Contains ("Emergency"))
						continue;
					multiEffect = effect.m_effect.GetComponent<MultiEffect> ();
					if (multiEffect == null || multiEffect.m_effects == null)
						continue;
					foreach (MultiEffect.SubEffect subEffect in multiEffect.m_effects) {
						if (subEffect.m_effect == null)
							continue;
						switch (subEffect.m_effect.name) {
						case "Police Car Siren":
							clip = policeClip;
							if (!policeInjected) {
								policeClip = null;
								policeInjected = true;
							}
							break;
						case "Ambulance Siren":
							clip = ambulanceClip;
							if (!ambulanceInjected) {
								ambulanceClip = null;
								ambulanceInjected = true;
							}
							break;
						case "Fire Truck Siren":
							clip = firetruckClip;
							if (!firetruckInjected) {
								firetruckClip = null;
								firetruckInjected = true;
							}
							break;
						default:
							clip = null;
							break;
						}
						if (clip == null)
							continue;
						
						soundEffect = subEffect.m_effect.GetComponent<SoundEffect> ();
						soundEffect.m_audioInfo.m_clip = clip;
						soundEffect.m_audioInfo.m_loop = true;
						soundEffect.m_audioInfo.m_pitch = 1;
						stop = true;
						break;
					}
					if (stop) {
						if (policeInjected && ambulanceInjected && firetruckInjected)
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
