using UnityEngine;
using UnityEngine.Networking;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using System;
using System.Collections;
using System.IO;
using System.Threading;

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
		static Coroutine AWRoutine;
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

		[Invoke(StateManager.StateTypes.Start, 0)]
		public static void Init(InitParams initParams)
		{
			AWMod = initParams.Mod;
			var go = new GameObject(AWMod.Title);
			go.AddComponent<AmbientWeatherMod>();

			RainVolume = Settings.GetValue<float>("General", "RainVolume");
			StormVolume = Settings.GetValue<float>("General", "StormVolume");
			WindVolume = Settings.GetValue<float>("General", "WindVolume");
			LTChance = Settings.GetValue<int>("General", "LTChance");
			LTChance = (3 / LTChance) * 100;
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

			StartCoroutine(GetAudioClips());

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
						AWRoutine = StartCoroutine(PlayRain());
					}
					else if (weatherManager.IsSnowing)
					{
						AWRoutine = StartCoroutine(PlaySnow());
					}
					else if (weatherManager.IsStorming)
					{
						BOOM = new System.Random();

						AWRoutine = StartCoroutine(PlayStorm());
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
					if (AWRoutine != null)
					{
						StopCoroutine(AWRoutine);
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

		public static void InitMod()
		{
			Debug.Log("Begin mod init: Ambient Weather");

			Settings = AWMod.GetSettings();

			Debug.Log("Finished mod init: Ambient Weather");
		}

		IEnumerator GetAudioClips()
		{
			L1Sound = Path.Combine(Application.streamingAssetsPath, "Sound", "Lightning 1.ogg");
			UnityWebRequest L1WWW = UnityWebRequestMultimedia.GetAudioClip("file://" + L1Sound, AudioType.OGGVORBIS);
			yield return L1WWW.SendWebRequest();
			L1Clip = DownloadHandlerAudioClip.GetContent(L1WWW);

			L2Sound = Path.Combine(Application.streamingAssetsPath, "Sound", "Lightning 2.ogg");
			UnityWebRequest L2WWW = UnityWebRequestMultimedia.GetAudioClip("file://" + L2Sound, AudioType.OGGVORBIS);
			yield return L2WWW.SendWebRequest();
			L2Clip = DownloadHandlerAudioClip.GetContent(L2WWW);

			RainSound = Path.Combine(Application.streamingAssetsPath, "Sound", "Rain.ogg");
			UnityWebRequest RainWWW = UnityWebRequestMultimedia.GetAudioClip("file://" + RainSound, AudioType.OGGVORBIS);
			yield return RainWWW.SendWebRequest();
			RainClip = DownloadHandlerAudioClip.GetContent(RainWWW);

			ThunderSound = Path.Combine(Application.streamingAssetsPath, "Sound", "Thunder.ogg");
			UnityWebRequest ThunderWWW = UnityWebRequestMultimedia.GetAudioClip("file://" + ThunderSound, AudioType.OGGVORBIS);
			yield return ThunderWWW.SendWebRequest();
			ThunderClip = DownloadHandlerAudioClip.GetContent(ThunderWWW);

			WindSound = Path.Combine(Application.streamingAssetsPath, "Sound", "Wind.ogg");
			UnityWebRequest WindWWW = UnityWebRequestMultimedia.GetAudioClip("file://" + WindSound, AudioType.OGGVORBIS);
			yield return WindWWW.SendWebRequest();
			WindClip = DownloadHandlerAudioClip.GetContent(WindWWW);
		}

		IEnumerator PlayRain()
		{
			AWSource.PlayOneShot(RainClip, RainVolume);
			yield return new WaitForSeconds(2);
		}

		IEnumerator PlaySnow()
		{
			AWSource.PlayOneShot(WindClip, WindVolume);
			yield return new WaitForSeconds(60);
		}

		IEnumerator PlayStorm()
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

			yield return new WaitForSeconds(2);
		}
	}
}
