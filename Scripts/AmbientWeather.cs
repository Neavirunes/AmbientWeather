using System;
using System.IO;
using System.Threading;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace AmbientWeather
{
	[RequireComponent(typeof(DaggerfallAudioSource))]
	public class AmbientWeatherMod : MonoBehaviour
	{
		static AudioClip L1Clip;
		static AudioClip L2Clip;
		static AudioClip RainClip;
		static AudioClip ThunderClip;
		static AudioClip WindClip;
		static AudioSource AWSource;
		static AutoResetEvent AWEvent;
		static float RainVolume;
		static float StormVolume;
		static float WindVolume;
		static int IndoorsCheck;
		static int LTChance;
		static int Rando;
		static Mod AWMod;
		static ModSettings Settings;
		static string L1Sound;
		static string L2Sound;
		static string RainSound;
		static string ThunderSound;
		static string WindSound;
		static System.Random BOOM;
		static Timer AWTimer;

		[Invoke(StateManager.StateTypes.Start, 0)]
		public static void Init(InitParams initParams)
		{
			AWMod = initParams.Mod;
			var go = new GameObject(AWMod.Title);
			go.AddComponent<AmbientWeatherMod>();
		}

		void Awake()
		{
			InitMod();

			AWSource = gameObject.AddComponent<AudioSource>();
			AWSource.hideFlags = HideFlags.HideInInspector;
			AWSource.playOnAwake = false;
			AWSource.loop = false;
			AWSource.dopplerLevel = 0f;
			AWSource.spatialBlend = 0f;
			AWSource.volume = DaggerfallUnity.Settings.SoundVolume;

			AWMod.IsReady = true;
		}

		void Update()
		{
			if (GameManager.Instance.PlayerEnterExit.IsPlayerInsideBuilding)
			{
				if (IndoorsCheck == 0)
				{
					WeatherManager weatherManager = GameManager.Instance.WeatherManager;
					if (weatherManager.IsRaining)
					{
						AWEvent = new AutoResetEvent(false);
						AWTimer = new Timer(PlayRain, AWEvent, 0, 2000);
					}
					else if (weatherManager.IsSnowing)
					{
						AWEvent = new AutoResetEvent(false);
						AWTimer = new Timer(PlaySnow, AWEvent, 0, 60000);
					}
					else if (weatherManager.IsStorming)
					{
						BOOM = new System.Random();

						AWEvent = new AutoResetEvent(false);
						AWTimer = new Timer(PlayStorm, AWEvent, 0, 2000);
					}

					IndoorsCheck = 1;
				}
				else
				{
					if (!AWSource.isPlaying)
					{
						IndoorsCheck = 0;
					}
				}
			}
			else
			{
				if (IndoorsCheck == 1)
				{
					if (AWTimer != null)
					{
						AWTimer.Dispose();
					}

					if (AWSource != null)
					{
						if (AWSource.isPlaying)
						{
							AWSource.Stop();
						}
					}

					IndoorsCheck = 0;
				}
			}
		}

		void PlayRain(System.Object stateInfo)
		{
			AWSource.PlayOneShot(RainClip, RainVolume);
		}

		void PlaySnow(System.Object stateInfo)
		{
			AWSource.PlayOneShot(WindClip, WindVolume);
		}

		void PlayStorm(System.Object stateInfo)
		{
			AWSource.PlayOneShot(RainClip, RainVolume);

			Rando = BOOM.Next(0, LTChance);
			switch (Rando)
			{
				case 0:
					AWSource.PlayOneShot(L1Clip, StormVolume);
					break;
				case 1:
					AWSource.PlayOneShot(L2Clip, StormVolume);
					break;
				case 2:
					AWSource.PlayOneShot(ThunderClip, StormVolume);
					break;
				default:
					break;
			}
		}

		public static void InitMod()
		{
			Debug.Log("Begin mod init: Ambient Weather");

			Settings = AWMod.GetSettings();

			RainVolume = Settings.GetValue<float>("General", "RainVolume");
			StormVolume = Settings.GetValue<float>("General", "StormVolume");
			WindVolume = Settings.GetValue<float>("General", "WindVolume");

			LTChance = Settings.GetValue<int>("General", "LTChance");
			LTChance = (3 / LTChance) * 100;

			L1Sound = Path.Combine(Application.streamingAssetsPath, "Sound", "Lightning 1.ogg");
			WWW L1WWW = new WWW("file://" + L1Sound);
			L1Clip = L1WWW.GetAudioClip(false, true, AudioType.OGGVORBIS);

			L2Sound = Path.Combine(Application.streamingAssetsPath, "Sound", "Lightning 2.ogg");
			WWW L2WWW = new WWW("file://" + L2Sound);
			L2Clip = L2WWW.GetAudioClip(false, true, AudioType.OGGVORBIS);

			RainSound = Path.Combine(Application.streamingAssetsPath, "Sound", "Rain.ogg");
			WWW RainWWW = new WWW("file://" + RainSound);
			RainClip = RainWWW.GetAudioClip(false, true, AudioType.OGGVORBIS);

			ThunderSound = Path.Combine(Application.streamingAssetsPath, "Sound", "Thunder.ogg");
			WWW ThunderWWW = new WWW("file://" + ThunderSound);
			ThunderClip = ThunderWWW.GetAudioClip(false, true, AudioType.OGGVORBIS);

			WindSound = Path.Combine(Application.streamingAssetsPath, "Sound", "Wind.ogg");
			WWW WindWWW = new WWW("file://" + WindSound);
			WindClip = WindWWW.GetAudioClip(false, true, AudioType.OGGVORBIS);

			Debug.Log("Finished mod init: Ambient Weather");
		}
	}
}
