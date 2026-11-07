using HarmonyLib;
using Il2Cpp;

namespace DevourClient.Hooks
{
    public class Hooks
    {
	[HarmonyPatch(typeof(Il2Cpp.NolanBehaviour), "OnAttributeUpdateValue")]
        static class NolanBehaviour_UV
        {
            [HarmonyPrefix]
            static void Prefix(Il2Cpp.NolanBehaviour __instance, ref Il2CppOpsive.UltimateCharacterController.Traits.Attribute attribute)
            {
                if (!ClientMain.unlimitedUV || attribute == null || attribute.m_Name != "Battery")
                {
                    return;
                }

                // Only your own character: other players' copies in a friend lobby are left alone.
                if (__instance == null || __instance.entity == null || !__instance.entity.IsOwner)
                {
                    return;
                }

                attribute.m_Value = attribute.MaxValue;
            }
        }    
	      
        [HarmonyPatch(typeof(Il2Cpp.RankHelpers))]
        [HarmonyPatch(nameof(Il2Cpp.RankHelpers.CalculateExpGain))] //annotation boiler plate to tell Harmony what to patch. Refer to docs.
        static class RankHelpers_CalculateExpGain
        {
            static void Postfix(ref Il2Cpp.RankHelpers.ExpGainInfo __result)
            {
                if (ClientMain.exp_modifier)
                {
                    __result.totalExp = (int)(__result.totalExp * ClientMain.exp);
                }
                return;
            }
        }
        

        [HarmonyPatch(typeof(Il2CppHorror.Menu))]
        [HarmonyPatch(nameof(Il2CppHorror.Menu.SetupPerk))] //annotation boiler plate to tell Harmony what to patch. Refer to docs.
        static class Horror_Menu_SetupPerk_Patch
        {
            static void Prefix(ref Il2Cpp.CharacterPerk perk)
            {
                /*
                    public int cost { get; set; }
                    public bool isOwned { get; set; }
                    public bool isHidden { get; set; }
                */

                //MelonLoader.MelonLogger.Msg("cost : " + perk.cost);
                //MelonLoader.MelonLogger.Msg("isOwned : " + perk.isOwned);
                //MelonLoader.MelonLogger.Msg("isHidden : " + perk.isHidden);
                perk.cost = 0;
                perk.isOwned = true;
                perk.isHidden = false;
                return;
            }
        }

        [HarmonyPatch(typeof(Il2CppHorror.Menu))]
        [HarmonyPatch(nameof(Il2CppHorror.Menu.SetupOutfit))] //annotation boiler plate to tell Harmony what to patch. Refer to docs.
        static class Horror_Menu_SetupOutfit_Patch
        {
            static void Prefix(ref Il2Cpp.CharacterOutfit outfit)
            {
                /*
                    public ulong currentPrice;
	                public ulong basePrice;
	                public bool isOwned;
	                public bool isHidden;
                */

                //MelonLoader.MelonLogger.Msg("basePrice : " + outfit.basePrice);
                //MelonLoader.MelonLogger.Msg("currentPrice : " + outfit.currentPrice);
                //MelonLoader.MelonLogger.Msg("isOwned : " + outfit.isOwned);
                //MelonLoader.MelonLogger.Msg("isHidden : " + outfit.isHidden);
                outfit.isOwned = true;
                outfit.isHidden = false;
                return;
            }
        }

        [HarmonyPatch(typeof(Il2Cpp.OptionsHelpers))]
        [HarmonyPatch(nameof(Il2Cpp.OptionsHelpers.IsRobeUnlocked))] //annotation boiler plate to tell Harmony what to patch. Refer to docs.
        static class OptionsHelpers_IsRobeUnlocked_Patch
        {
            static bool Prefix(ref string robe)
            {
                //MelonLoader.MelonLogger.Msg("robe : " + robe);

                robe = "Default";
                return true;
            }
        }

        [HarmonyPatch(typeof(Il2CppHorror.Menu))]
        [HarmonyPatch(nameof(Il2CppHorror.Menu.SetupFlashlight))] //annotation boiler plate to tell Harmony what to patch. Refer to docs.
        static class Horror_Menu_SetLocked_Patch
        {
            static void Prefix(Il2Cpp.CharacterFlashlight flashlight)
            {
                /*
                public bool isHidden { get; set; }
                public int cost { get; set; }
                public bool requiresPurchase { get; set; }
                public bool isOwned { get; set; }
                */

                //MelonLoader.MelonLogger.Msg("isHidden : " + flashlight.isHidden);
                //MelonLoader.MelonLogger.Msg("cost : " + flashlight.cost);
                //MelonLoader.MelonLogger.Msg("requiresPurchase : " + flashlight.requiresPurchase);
                //MelonLoader.MelonLogger.Msg("isOwned : " + flashlight.isOwned);

                flashlight.isHidden = false;
                flashlight.cost = 0;
                flashlight.requiresPurchase = false;
                flashlight.isOwned = true;
                return;
            }
        }

        // Azazel's calm-down check. It runs where the AI runs, i.e. on the lobby owner's game only.
        [HarmonyPatch(typeof(Il2CppBehaviorDesigner.Runtime.Tasks.ShouldCalmDown), "OnUpdate")]
        static class ShouldCalmDown_OnUpdate
        {
            static void Prefix(Il2CppBehaviorDesigner.Runtime.Tasks.ShouldCalmDown __instance)
            {
                try
                {
                    var count = __instance.staggerCount;
                    var needed = __instance.staggersToCalmDown;
                    if (count == null || needed == null || needed.Value == null || needed.Value.Count == 0)
                    {
                        return;
                    }

                    // The required count grows with ritual progress.
                    int progress = 0;
                    if (__instance.burnController != null) progress = __instance.burnController.NumBurntObjects();
                    else if (__instance.mapController != null) progress = __instance.mapController.GetMapProgress();
                    int target = needed.Value[System.Math.Clamp(progress, 0, needed.Value.Count - 1)];

                    if (ClientMain.oneStaggerCalm && count.Value >= 1 && count.Value < target)
                    {
                        count.Value = target;
                    }

                    ClientMain.azazelStaggers = count.Value;
                    ClientMain.azazelStaggersNeeded = target;
                    ClientMain.azazelStaggerSeen = UnityEngine.Time.time;
                }
                catch { }
            }
        }

        // Lets the local player count as having the perks ticked under Misc > Extra perks.
        [HarmonyPatch(typeof(Il2Cpp.NolanPerkController), nameof(Il2Cpp.NolanPerkController.HasPerk))]
        static class NolanPerkController_HasPerk
        {
            static void Postfix(Il2Cpp.NolanPerkController __instance, string perk, ref bool __result)
            {
                if (__result || perk == null || ClientMain.extraPerks.Count == 0)
                {
                    return;
                }

                // Cheap name check first; the owner check crosses into the game's native code, so only do it for perks we add.
                if (!ClientMain.extraPerks.Contains(Hacks.Misc.PerkKey(perk)))
                {
                    return;
                }

                Il2Cpp.NolanBehaviour nb = __instance.nolanBehaviour;
                if (nb == null || nb.entity == null || !nb.entity.IsOwner)
                {
                    return;
                }

                __result = true;
            }
        }

        [HarmonyPatch(typeof(Il2Cpp.NolanPerkController), nameof(Il2Cpp.NolanPerkController.OnAzazelPursueStart))]
        static class NolanPerkController_PursueStart
        {
            static void Postfix(Il2Cpp.NolanPerkController __instance)
            {
                Il2Cpp.NolanBehaviour nb = __instance.nolanBehaviour;
                if (nb != null && nb.entity != null && nb.entity.IsOwner) ClientMain.beingChased = true;
            }
        }

        [HarmonyPatch(typeof(Il2Cpp.NolanPerkController), nameof(Il2Cpp.NolanPerkController.OnAzazelPursueStop))]
        static class NolanPerkController_PursueStop
        {
            static void Postfix(Il2Cpp.NolanPerkController __instance)
            {
                Il2Cpp.NolanBehaviour nb = __instance.nolanBehaviour;
                if (nb != null && nb.entity != null && nb.entity.IsOwner) ClientMain.beingChased = false;
            }
        }

        [HarmonyPatch(typeof(ManorMirrorController))]
        [HarmonyPatch(nameof(ManorMirrorController.IsBroken))] //annotation boiler plate to tell Harmony what to patch. Refer to docs.
        static class ManorMirrorController_IsBroken
        {
            static void Postfix(ref bool __result)
            {
                if (ClientMain.infinite_mirrors)
                    __result = false;
            }
        }

        [HarmonyPatch(typeof(Il2CppHorror.Menu))]
        [HarmonyPatch(nameof(Il2CppHorror.Menu.SetupPet))] //annotation boiler plate to tell Harmony what to patch. Refer to docs.
        static class Horror_Menu_SetupPet_Patch
        {
            static void Prefix(ref Il2Cpp.CharacterPet pet)
            {
                //supporter pets are paid DLC, leave them as they are
                if (pet == null || pet.isSupporter)
                    return;

                pet.isOwned = true;
                pet.isHidden = false;
                return;
            }
        }

        //nightmare difficulty is gated behind having beaten hard mode
        [HarmonyPatch]
        static class AchievementHelpers_HasCompletedHard_Patch
        {
            static IEnumerable<System.Reflection.MethodBase> TargetMethods()
            {
                return typeof(Il2Cpp.AchievementHelpers)
                    .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly)
                    .Where(m => m.ReturnType == typeof(bool)
                        && m.GetParameters().Length == 0
                        && (m.Name.StartsWith("HasCompletedHard") || m.Name.StartsWith("get_hasCompletedHard"))
                        && !m.Name.Contains("_Public_") && !m.Name.Contains("_Private_"))
                    .Cast<System.Reflection.MethodBase>();
            }

            static void Postfix(ref bool __result)
            {
                __result = true;
            }
        }
    }
}
