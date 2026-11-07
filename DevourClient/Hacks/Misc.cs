using UnityEngine;
using MelonLoader;
using UnityEngine.UI;
using System.Reflection;
using System.Runtime.InteropServices;
using DevourClient.Helpers;
using System.Linq;
using System.Collections.Generic;
using Il2CppOpsive.UltimateCharacterController.Character;
using Il2CppPhoton.Bolt;

namespace DevourClient.Hacks
{
    public class Misc
    {
		public static void Fly(float speed) //normal speed 5f
		{
			Il2Cpp.NolanBehaviour nb = Player.GetPlayer();
			Vector3 pos = nb.transform.position;
			Il2Cpp.RewiredHelpers helpers = UnityEngine.Object.FindObjectOfType<Il2Cpp.RewiredHelpers>();
			if (Input.GetKey((KeyCode)System.Enum.Parse(typeof(KeyCode), helpers.GetCurrentBinding("Move Up").ToString().Replace(" ", ""))))
			{
				pos += nb.transform.forward * speed * Time.deltaTime;
			}
			if (Input.GetKey((KeyCode)System.Enum.Parse(typeof(KeyCode), helpers.GetCurrentBinding("Move Down").ToString().Replace(" ", ""))))
			{
				pos += -nb.transform.forward * speed * Time.deltaTime;
			}
			if (Input.GetKey((KeyCode)System.Enum.Parse(typeof(KeyCode), helpers.GetCurrentBinding("Move Right").ToString().Replace(" ", ""))))
			{
				pos += nb.transform.right * speed * Time.deltaTime;
			}
			if (Input.GetKey((KeyCode)System.Enum.Parse(typeof(KeyCode), helpers.GetCurrentBinding("Move Left").ToString().Replace(" ", ""))))
			{
				pos += -nb.transform.right * speed * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.Space))
			{
				pos += nb.transform.up * speed * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.LeftControl))
			{
				pos += -nb.transform.up * speed * Time.deltaTime;
			}
			nb.locomotion.SetPosition(pos, false);
		}
	    
	    public static void WalkInLobby(bool walk)
        {
			GameObject LocalPlayer = Helpers.Entities.LocalPlayer_.p_GameObject;
			if (LocalPlayer == null)
            {
				return;
            }

			//GetComponent called only once as AddComponent returns a component
			UltimateCharacterLocomotionHandler cmp = Helpers.Entities.LocalPlayer_.p_GameObject.GetComponent<UltimateCharacterLocomotionHandler>();

			if (cmp == null)
			{
				cmp = LocalPlayer.AddComponent<UltimateCharacterLocomotionHandler>();
                cmp.enabled = false;
            }

			cmp.enabled = walk;
		}
	    
	    public static void BurnRitualObj(string map, bool burnAll)
		{
			switch (map)
            {
				case "Inn":
					Il2Cpp.InnMapController _innMapController = UnityEngine.Object.FindObjectOfType<Il2Cpp.InnMapController>();
                    if (!_innMapController) {
                        return;
                    }

                    if (burnAll){
                        _innMapController.SetProgressTo(10);
                    }
                    else{
                        _innMapController.IncreaseProgress();
                    }
					break;

				case "Slaughterhouse":
                    Il2Cpp.SlaughterhouseAltarController _slaughterhouseAltarController = UnityEngine.Object.FindObjectOfType<Il2Cpp.SlaughterhouseAltarController>();
                    if (!_slaughterhouseAltarController) {
                        return;
                    }

                    if (burnAll)
                    {
                        _slaughterhouseAltarController.BurnGoat();
                    }
                    else
                    {
                        _slaughterhouseAltarController.SkipToGoat(10);
                    }
					break;

				case "Manor":
                    Il2Cpp.MapController mapc = UnityEngine.Object.FindObjectOfType<Il2Cpp.MapController>();
                    if (!mapc)
                    {
                        return;
                    }

                    if (burnAll)
                    {
                        mapc.SetProgressTo(10);
                    }
                    else
                    {
                        mapc.SetProgressTo(mapc.GetMapProgress() + 1);
                    }
                    break;

                default:
					Il2Cpp.SurvivalObjectBurnController _altar = UnityEngine.Object.FindObjectOfType<Il2Cpp.SurvivalObjectBurnController>();
                    if (!_altar) {
                        return;
                    }

                    if (burnAll)
                    {
                        _altar.SkipToGoat(10);
                    }
                    else
                    {
                        _altar.BurnGoat();
                    }
					break;
            }
        }
	    
	    public static void SpawnAzazel(PrefabId _azazelPrefabId)
		{
			if (!Il2CppPhoton.Bolt.BoltNetwork.IsServer)
			{
				Hacks.Misc.ShowMessageBox("You need to be host to spawn stuff !");
				return;
			}

			GameObject _localPlayer = Helpers.Entities.LocalPlayer_.p_GameObject;

			if (_localPlayer != null)
			{
                Vector3 pos = _localPlayer.transform.position;

                GameObject _azazel;

                _azazel = BoltNetwork.Instantiate(_azazelPrefabId, new Vector3(pos.x, pos.y, pos.z + 1f), Quaternion.identity);
                Il2Cpp.SurvivalAzazelBehaviour? azazelBehaviour = _azazel?.GetComponent<Il2Cpp.SurvivalAzazelBehaviour>();

                if (_azazel != null)
				{
					if (azazelBehaviour != null)
					{
                        _azazel.gameObject.GetComponent<Il2Cpp.SurvivalAzazelBehaviour>().Spawn();
					}
					else
					{
						MelonLogger.Error("azazelBehaviour is null!");
					}
                }
				else
				{
                    MelonLogger.Error("azazel is null!");
                }
            }
		}

		public static void CarryObject(string name)
		{
			Il2Cpp.NolanBehaviour nb = Helpers.Entities.LocalPlayer_.p_GameObject.GetComponent<Il2Cpp.NolanBehaviour>();

			nb.StartCarry(name);
		}

        public static void CleanFountain()
        {
            GameObject[] fountains = GameObject.FindGameObjectsWithTag("InnFountain");

            foreach (GameObject fountain in fountains)
            {
                fountain.GetComponent<Il2Cpp.InnFountainController>().Clean();
            }
        }

		private static float lastAutoRespawn = -10f;
		private static int autoRespawnTries = 0;

		// Call when the local player is no longer downed so the next knockdown starts fresh.
		public static void ResetAutoRespawn()
		{
			autoRespawnTries = 0;
		}

        public static void AutoRespawn()
		{
			// Called every frame while downed; space the revive attempts out so the network isn't flooded.
			if (Time.time - lastAutoRespawn < 3f)
			{
				return;
			}
			lastAutoRespawn = Time.time;

			Il2Cpp.NolanBehaviour nb = Player.GetPlayer();
			if (nb == null || nb.entity == null || !nb.entity.IsOwner)
			{
				return;
			}

			autoRespawnTries++;

			// 1st try: the normal revive network event, so the host and friends see you get up.
			bool posted = false;
			try
			{
				Player.Revive(nb.gameObject);
				posted = true;
			}
			catch (System.Exception e)
			{
				MelonLogger.Warning("[AutoRespawn] network revive refused (" + e.Message + "), reviving locally");
			}

			// Not the host / event ignored / still down 3s later: revive just your own character locally.
			// Your character is yours on the network, so getting up still syncs to the others.
			if (!posted || autoRespawnTries >= 2)
			{
				try
				{
					nb.OnReviveEvent(nb.entity, nb.entity);
				}
				catch (System.Exception e)
				{
					MelonLogger.Warning("[AutoRespawn] local revive failed: " + e.Message);
				}
			}
		}

		// Unlimited UV for your own character only: refill the battery every frame.
		public static void RefillLocalUV(Il2Cpp.NolanBehaviour nb)
		{
			if (nb == null || nb.entity == null || !nb.entity.IsOwner)
			{
				return;
			}

			try
			{
				var battery = nb.batteryAttribute;
				if (battery != null && battery.Value < battery.MaxValue)
				{
					battery.Value = battery.MaxValue;
				}
			}
			catch { }
		}

		public static int spawnBatch = 10;

		public static void SpawnItem(PrefabId prefab, string carryName)
		{
			Il2Cpp.NolanBehaviour nb = Player.GetPlayer();
			if (nb == null)
			{
				return;
			}

			if (!BoltNetwork.IsServer)
			{
				// Only the lobby owner can create network objects; everyone else can only put one in their hands.
				CarryObject(carryName);
				if (spawnBatch > 1)
				{
					MelonLogger.Warning("Batch spawning needs you to be the lobby owner, spawned 1 in your hands.");
				}
				return;
			}

			if (spawnBatch <= 1 && Player.IsInGame())
			{
				CarryObject(carryName);
				return;
			}

			Vector3 origin = nb.transform.position;
			for (int i = 0; i < spawnBatch; i++)
			{
				float angle = i * Mathf.PI * 2f / spawnBatch;
				Vector3 pos = origin + new Vector3(Mathf.Cos(angle) * 1.2f, 0.3f, Mathf.Sin(angle) * 1.2f);
				BoltNetwork.Instantiate(prefab, pos, Quaternion.identity);
			}
		}

		// "SurvivalFirstAid(Clone)" -> "Medkit", "Egg-Clean-3" -> "Egg-Clean"
		public static string ItemLabel(string objName)
		{
			string n = objName.Replace("(Clone)", "").Replace("Survival", "").Trim();
			n = System.Text.RegularExpressions.Regex.Replace(n, @"(\s*\(\d+\)|-\d+)$", "");
			switch (n)
			{
				case "FirstAid": return "Medkit";
				case "RottenFood": return "Food";
				default: return n;
			}
		}

		// Items lying around the current map, grouped by type. Items someone is holding are left out.
		public static SortedDictionary<string, List<Il2Cpp.SurvivalInteractable>> GetMapItems()
		{
			var groups = new SortedDictionary<string, List<Il2Cpp.SurvivalInteractable>>();
			if (Helpers.Entities.SurvivalInteractables == null)
			{
				return groups;
			}

			foreach (Il2Cpp.SurvivalInteractable item in Helpers.Entities.SurvivalInteractables)
			{
				if (item == null || !item.gameObject.activeInHierarchy || item.GetComponentInParent<Il2Cpp.NolanBehaviour>() != null)
				{
					continue;
				}

				string label = ItemLabel(item.gameObject.name);
				if (!groups.TryGetValue(label, out var list))
				{
					groups[label] = list = new List<Il2Cpp.SurvivalInteractable>();
				}
				list.Add(item);
			}
			return groups;
		}

		// Brings the closest item of the group, or every one of them.
		public static void TPItemGroup(List<Il2Cpp.SurvivalInteractable> items, bool all)
		{
			Il2Cpp.NolanBehaviour Nolan = Player.GetPlayer();
			if (Nolan == null || items.Count == 0)
			{
				return;
			}

			Vector3 me = Nolan.transform.position;
			IEnumerable<Il2Cpp.SurvivalInteractable> pick = all ? items : items.OrderBy(i => Vector3.Distance(i.transform.position, me)).Take(1);
			foreach (Il2Cpp.SurvivalInteractable item in pick)
			{
				item.transform.position = me + Nolan.transform.forward * UnityEngine.Random.RandomRange(1f, 2f) + Vector3.up * 0.3f;
			}
		}

		// "PERK_FIELD_MEDIC", "PerkFieldMedic" and "FieldMedic" all become "FIELDMEDIC".
		public static string PerkKey(string perk)
		{
			string k = new string(perk.Where(char.IsLetter).ToArray()).ToUpperInvariant();
			return k.StartsWith("PERK") ? k.Substring(4) : k;
		}

		public static void SetExtraPerk(string perkId, bool on)
		{
			string key = PerkKey(perkId);
			if (on) ClientMain.extraPerks.Add(key); else ClientMain.extraPerks.Remove(key);

			Il2Cpp.NolanBehaviour nb = Player.GetPlayer();
			if (nb != null)
			{
				SetPerkState(nb, perkId, on);
			}
		}

		// Some perks work through a character state named like "PerkFieldMedic" instead of HasPerk checks.
		static void SetPerkState(Il2Cpp.NolanBehaviour nb, string perkId, bool on)
		{
			string state = "Perk" + string.Concat(perkId.Replace("PERK_", "").ToLowerInvariant().Split('_').Select(w => w.Length > 0 ? char.ToUpperInvariant(w[0]) + w.Substring(1) : w));
			try
			{
				Il2CppOpsive.UltimateCharacterController.StateSystem.StateManager.SetState(nb.gameObject, state, on);
			}
			catch (System.Exception ex)
			{
				MelonLogger.Warning($"Perk state {state}: {ex.Message}");
			}
		}

		// Re-applies ticked perks (Fast Worker by default) each time a new character spawns.
		public static void ApplyExtraPerkStates(Il2Cpp.NolanBehaviour nb)
		{
			foreach (string key in ClientMain.extraPerks)
			{
				SetPerkState(nb, "PERK_" + key, true);
			}
		}

		// The game's own pursue events, with Azazel being close by as a fallback in case they don't reach this player.
		public static bool IsBeingChased(Il2Cpp.NolanBehaviour nb)
		{
			if (ClientMain.beingChased)
			{
				return true;
			}

			if (Helpers.Entities.Azazels == null)
			{
				return false;
			}

			foreach (Il2Cpp.SurvivalAzazelBehaviour azazel in Helpers.Entities.Azazels)
			{
				if (azazel != null && Vector3.Distance(azazel.transform.position, nb.transform.position) < 8f)
				{
					return true;
				}
			}
			return false;
		}

		// The game's input re-locks the mouse on click unless told not to, which swallowed clicks on the menu.
		public static void SetGameCursorCapture(bool capture)
		{
			foreach (Il2CppOpsive.UltimateCharacterController.Input.UnityInput input in UnityEngine.Object.FindObjectsOfType<Il2CppOpsive.UltimateCharacterController.Input.UnityInput>())
			{
				input.DisableCursor = capture;
			}
		}
		public static void TPItems()
		{
            Il2Cpp.NolanBehaviour Nolan = Player.GetPlayer();

            foreach (Il2Cpp.SurvivalInteractable item in Helpers.Entities.SurvivalInteractables)
            {
                item.transform.position = Nolan.transform.position + Nolan.transform.forward * UnityEngine.Random.RandomRange(1f, 3f);
            }
		}
	    
		public static void CreateCustomizedLobby(int lobbySize = 4, bool isPrivate = false, Il2CppUdpKit.Platform.Photon.PhotonRegion.Regions __region = Il2CppUdpKit.Platform.Photon.PhotonRegion.Regions.BEST_REGION)
        {			
			//TODO : make it so we can specify a password for a private lobby

			Il2CppHorror.Menu _menu = UnityEngine.Object.FindObjectOfType<Il2CppHorror.Menu>();

			Il2CppUdpKit.Platform.PhotonPlatformConfig __photonPlatformConfig = new Il2CppUdpKit.Platform.PhotonPlatformConfig();
            __photonPlatformConfig.Region = Il2CppUdpKit.Platform.Photon.PhotonRegion.regions[__region];

			BoltLauncher.SetUdpPlatform(new Il2CppUdpKit.Platform.PhotonPlatform(__photonPlatformConfig));

            BoltConfig __config = UnityEngine.Object.FindObjectOfType<Il2CppHorror.Menu>().boltConfig;
			Toggle __toggle = _menu.hostPrivateServer;

			__toggle.isOn = isPrivate;
            __config.serverConnectionLimit = lobbySize;

			BoltLauncher.StartServer(__config, null);
			
			_menu.ShowCanvasGroup(_menu.loadingCanvasGroup, true);
            _menu.ShowCanvasGroup(_menu.hostCanvasGroup, false);
            _menu.ShowCanvasGroup(_menu.mainMenuCanvasGroup, false);
        }
		

		public static void BigFlashlight(bool reset)
        {
			Il2Cpp.NolanBehaviour Nolan = Player.GetPlayer();
			if (Nolan == null)
            {
				return;
            }

			Light flashlightSpot = Nolan.flashlightSpot;
			if (flashlightSpot == null)
            {
				return;
            }

			if (reset)
			{
				flashlightSpot.intensity = 1.4f;
				flashlightSpot.range = 9f;
				flashlightSpot.spotAngle = 70f;
			}
			else
			{
				flashlightSpot.intensity = 1.1f;
				flashlightSpot.range = 200f;
				flashlightSpot.spotAngle = 90f;
			}
			
		}

		public static List<Transform> GetAllBones(Animator a)
		{
			List<Transform> Bones = new List<Transform>
			{
				a.GetBoneTransform(HumanBodyBones.Head), // 0
                a.GetBoneTransform(HumanBodyBones.Neck), // 1
                a.GetBoneTransform(HumanBodyBones.Spine), // 2
                a.GetBoneTransform(HumanBodyBones.Hips), // 3

                a.GetBoneTransform(HumanBodyBones.LeftShoulder), // 4
                a.GetBoneTransform(HumanBodyBones.LeftUpperArm), // 5
                a.GetBoneTransform(HumanBodyBones.LeftLowerArm), // 6
                a.GetBoneTransform(HumanBodyBones.LeftHand), // 7

                a.GetBoneTransform(HumanBodyBones.RightShoulder), // 8
                a.GetBoneTransform(HumanBodyBones.RightUpperArm), // 9
                a.GetBoneTransform(HumanBodyBones.RightLowerArm), // 10
                a.GetBoneTransform(HumanBodyBones.RightHand), // 11

                a.GetBoneTransform(HumanBodyBones.LeftUpperLeg), // 12
                a.GetBoneTransform(HumanBodyBones.LeftLowerLeg), // 13
                a.GetBoneTransform(HumanBodyBones.LeftFoot), // 14

                a.GetBoneTransform(HumanBodyBones.RightUpperLeg), // 15
                a.GetBoneTransform(HumanBodyBones.RightLowerLeg), // 16
                a.GetBoneTransform(HumanBodyBones.RightFoot) // 17
            };

			return Bones;
		}

		public static void Fullbright(bool reset)
		{
			Il2Cpp.NolanBehaviour Nolan = Player.GetPlayer();
			if (Nolan == null)
			{
				return;
			}

			Light flashlightSpot = Nolan.flashlightSpot;
			if (flashlightSpot == null)
			{
				return;
			}

			if (reset)
			{
				flashlightSpot.intensity = 1.4f;
				flashlightSpot.range = 9f;
				flashlightSpot.spotAngle = 70f;
				flashlightSpot.type = LightType.Spot;
				flashlightSpot.shadows = LightShadows.Soft;
			}
			else
			{
				flashlightSpot.intensity = 1.1f;
				flashlightSpot.range = 200f;
				flashlightSpot.spotAngle = 179f;
				flashlightSpot.type = LightType.Point;
				flashlightSpot.shadows = LightShadows.None;
			}

		}
		public static void FlashlightColor(Color color)
        {
			Il2Cpp.NolanBehaviour Nolan = Player.GetPlayer();
			Light flashlightSpot = Nolan.flashlightSpot;

			flashlightSpot.color = color;
		}

		public static void TPKeys()
        {
			//TOFIX: spawn manually the missing key in slaughterhouse
			Il2Cpp.NolanBehaviour Nolan = Player.GetPlayer();

			foreach (Il2Cpp.KeyBehaviour keyBehaviour in Helpers.Entities.Keys)
			{
				if (keyBehaviour == null)
				{
					return;
				}
				keyBehaviour.transform.position = Nolan.transform.position + Nolan.transform.forward * 1.5f;
			}
		}

		public static void SetRank(int rank)
        {
			Il2Cpp.NolanRankController NolanRank = UnityEngine.Object.FindObjectOfType<Il2Cpp.NolanRankController>();
			if (NolanRank == null)
            {
				return;
            }
			NolanRank.SetRank(rank);
		}


		public static void DespawnDemons()
		{
			foreach (Il2Cpp.SurvivalDemonBehaviour demon in Helpers.Entities.Demons)
			{
				if (demon != null)
				{
					demon.Despawn();
				}
			}
		}

		public static void DespawnSpiders()
		{
			foreach (Il2Cpp.SpiderBehaviour spider in Helpers.Entities.Spiders)
			{
				if (spider != null)
				{
					spider.Despawn();
				}
			}
		}

		public static void DespawnGhosts()
		{
			foreach (Il2Cpp.GhostBehaviour ghost in Helpers.Entities.Ghosts)
			{
				if (ghost != null)
				{
					ghost.Despawn();
				}
			}
		}

		public static void DespawnBoars()
		{
			foreach (Il2Cpp.BoarBehaviour boar in Helpers.Entities.Boars)
			{
				if (boar != null)
				{
					boar.Despawn();
				}
			}
		}

		public static void DespawnCorpses()
		{
			foreach (Il2Cpp.CorpseBehaviour corpse in Helpers.Entities.Corpses)
			{
				if (corpse != null)
				{
					corpse.Despawn();
				}
			}
		}

        public static void DespawnCrows()
        {
            foreach (Il2Cpp.CrowBehaviour crow in Helpers.Entities.Crows)
            {
                if (crow != null)
                {
                    crow.Despawn();
                }
            }
        }

        public static void DespawnLumps()
        {
            foreach (Il2Cpp.ManorLumpController lump in Helpers.Entities.Lumps)
            {
                if (lump != null)
                {
					lump.Dissolve();
                }
            }
        }

		public static void DespawnMonkeys()
		{
			foreach (Il2Cpp.MonkeyBehaviour monkey in Helpers.Entities.Monkeys)
			{
				if (monkey != null)
				{
					monkey.Despawn();
				}
			}
		}

        public static int ShowMessageBox(string message)
        {
			//not used, might be useful, some day
			Il2CppHorror.Menu menu = UnityEngine.Object.FindObjectOfType<Il2CppHorror.Menu>();
			if (menu == null)
				return 1;
			menu.ShowMessageModal(message);
			return 0;
		}
		
		public static void PlaySound()
        {
			Il2Cpp.PlayRandomAudioClip playRandomAudioClip = UnityEngine.Object.FindObjectOfType<Il2Cpp.PlayRandomAudioClip>();
			Il2Cpp.NolanVoiceOvers nolanVoiceOvers = UnityEngine.Object.FindObjectOfType<Il2Cpp.NolanVoiceOvers>();
			playRandomAudioClip.delay = 0f;

			int num = UnityEngine.Random.RandomRangeInt(0, 10);
			switch (num)
            {
				case 0:
					nolanVoiceOvers.yesClips.Play();
					return;
				case 1:
					nolanVoiceOvers.noClips.Play();
					return;
				case 2:
					nolanVoiceOvers.beckonClips.Play();
					return;
				case 3:
					nolanVoiceOvers.showOffClips.Play();
					return;
				case 4:
					nolanVoiceOvers.screamClips.Play();
					return;
				case 5:
					nolanVoiceOvers.pickupClips.Play();
					return;
				case 6:
					nolanVoiceOvers.burnGoatClips.Play();
					return;
				case 7:
					nolanVoiceOvers.laughClips.Play();
					return;
				case 8:
					nolanVoiceOvers.PlayMoan();
					return;
				case 9:
					nolanVoiceOvers.Scream();
					return;
				default:
					return;
            }
		}
		
		public static void FreezeAzazel()
		{
			if (Helpers.Map.GetActiveScene() == "Menu")
			{
				return;
			}

            if (Helpers.Map.GetAzazel() == null) {
                return;
            }

            UltimateCharacterLocomotion _azazelLocomotion = Helpers.Map.GetAzazel().GetComponent<UltimateCharacterLocomotion>();

            if (_azazelLocomotion == null)
            {
                return;
            }

            if (_azazelLocomotion.TimeScale > 0.0f) 
            {
                _azazelLocomotion.TimeScale = 0f; //host only (?)
            }
            else
            {
                _azazelLocomotion.TimeScale = 1.0f;
            }
		}
		
		public static void InstantWin()
        {
			Il2Cpp.Survival survival_class = UnityEngine.Object.FindObjectOfType<Il2Cpp.Survival>();
			string map_name = Map.GetMapName(Map.GetActiveScene());

			if (map_name == "Menu" || survival_class == null)
            {
				return;
            }

			if (map_name == "Farmhouse")
            {
				survival_class.PlayEnding("Win");
				return;
			}

			survival_class.PlayEnding(map_name+"Win");
		}
    }
}
