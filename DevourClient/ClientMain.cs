using DevourClient.Helpers;
using MelonLoader;
using System.Threading.Tasks;
using Il2CppPhoton.Bolt;
using UnityEngine;
using Il2Cpp;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DevourClient
{
    public class ClientMain : MonoBehaviour, IDisposable
    {
        public ClientMain(IntPtr ptr)
            : base(ptr)
        {
        }

        enum CurrentTab : int
        {
            Visuals = 0,
            Entities = 1,
            Map = 2,
            ESP = 3,
            Items = 4,
            Misc = 5,
            Players = 6
        }

        static Rect windowRect = new Rect(Settings.Settings.x + 10, Settings.Settings.y + 10, 800, 750);
        static CurrentTab current_tab = CurrentTab.Visuals;

        static bool flashlight_toggle = false;
        static bool flashlight_colorpick = false;
        static bool player_esp_colorpick = false;
        static bool azazel_esp_colorpick = false;
        static bool spoofLevel = false;
        static float spoofLevelValue = 0;
        
        // UI variables
        static bool fly = false;
        static float fly_speed = 5;
        static bool fastMove = false;
        static float _PlayerSpeedMultiplier = 1;
        public static bool _IsAutoRespawn = false;
        public static bool unlimitedUV = false;
        public static HashSet<string> extraPerks = new HashSet<string> { "FASTWORKER" }; // normalized keys, see Misc.PerkKey; Fast Worker is always on by default
        static int perksAppliedFor = 0;
        public static bool chaseBoost = true;
        public static float chaseBoostMultiplier = 1.5f;
        public static bool beingChased = false; // set from the game's pursue start/stop events
        static readonly string[] perkIds = { "PERK_ACCELERATION", "PERK_AFTERSHOCK", "PERK_CONSERVATIONIST", "PERK_CRAWLER", "PERK_DURACELL_BUNNY", "PERK_EVADER", "PERK_FAST_WORKER", "PERK_FIELD_MEDIC", "PERK_FIRST_RESPONDER", "PERK_FULLY_CHARGED", "PERK_IMMUNE", "PERK_INSPIRED", "PERK_KEYMASTER", "PERK_LONG_SIGHTED", "PERK_PATIENT", "PERK_POWERFUL_INSIGHT", "PERK_QUARTERMASTER", "PERK_REGENERATION", "PERK_REPELLENT", "PERK_SPEEDY", "PERK_SPEED_WALKER", "PERK_STAY_BACK", "PERK_SUPPLY_RUNNER", "PERK_TAMED", "PERK_TEAMWORK", "PERK_TEAM_LEADER", "PERK_TRAPPER", "PERK_UNDER_PRESSURE" };
        public static bool oneStaggerCalm = false;
        public static bool showStaggerHud = true;
        public static int azazelStaggers = 0;
        public static int azazelStaggersNeeded = 0;
        public static float azazelStaggerSeen = -100f; // last time the host-side calm check reported in
        public static bool exp_modifier = true;
        public static float exp = 3f; // multiplier applied to the game's normal EXP gain
        public static bool _walkInLobby = false;
        public static bool infinite_mirrors = false;
        static bool player_esp = false;
        static bool player_skel_esp = false;
        static bool player_snapline = false;
        static bool azazel_esp = false;
        static bool azazel_skel_esp = false;
        static bool azazel_snapline = false;
        static bool item_esp = false;
        static bool goat_rat_esp = false;
        static bool demon_esp = false;
        static bool fullbright = false;
        static bool need_fly_reset = false;
        static bool crosshair = false;
        static bool in_game_cache = false;
        static bool should_show_start_message = true;
        static Texture2D crosshairTexture = default!;

        private static int frameCount = 0;
        private static int lastMemoryLog = 0;
        private const int GC_GEN0_INTERVAL = 300;
        private const int GC_FULL_INTERVAL = 3600;
        private const int MEMORY_LOG_INTERVAL = 1800;

        public void Start()
        {
            MelonLogger.Msg("For the Queen !");
            MelonLogger.Warning("Made with <3 by patate and Jadis.");
            MelonLogger.Warning("Github : https://github.com/ALittlePatate/DevourClient");
            MelonLogger.Warning("Note : if you payed for this you most likely got scammed.");

            long startMemory = GC.GetTotalMemory(false);
            MelonLogger.Msg($"[Memory Monitor] Startup managed memory: {startMemory / 1024 / 1024} MB");

            crosshairTexture = Helpers.GUIHelper.GetCircularTexture(5, 5);

            Helpers.Entities.StartAllCoroutines();

            Helpers.Entities.RegisterCoroutine(MelonCoroutines.Start(Helpers.Entities.GetLocalPlayer()));
            Helpers.Entities.RegisterCoroutine(MelonCoroutines.Start(Helpers.Entities.GetGoatsAndRats()));
            Helpers.Entities.RegisterCoroutine(MelonCoroutines.Start(Helpers.Entities.GetSurvivalInteractables()));
            Helpers.Entities.RegisterCoroutine(MelonCoroutines.Start(Helpers.Entities.GetKeys()));
            Helpers.Entities.RegisterCoroutine(MelonCoroutines.Start(Helpers.Entities.GetDemons()));
            Helpers.Entities.RegisterCoroutine(MelonCoroutines.Start(Helpers.Entities.GetSpiders()));
            Helpers.Entities.RegisterCoroutine(MelonCoroutines.Start(Helpers.Entities.GetGhosts()));
            Helpers.Entities.RegisterCoroutine(MelonCoroutines.Start(Helpers.Entities.GetBoars()));
            Helpers.Entities.RegisterCoroutine(MelonCoroutines.Start(Helpers.Entities.GetCorpses()));
            Helpers.Entities.RegisterCoroutine(MelonCoroutines.Start(Helpers.Entities.GetCrows()));
            Helpers.Entities.RegisterCoroutine(MelonCoroutines.Start(Helpers.Entities.GetLumps()));
            Helpers.Entities.RegisterCoroutine(MelonCoroutines.Start(Helpers.Entities.GetAzazels()));
            Helpers.Entities.RegisterCoroutine(MelonCoroutines.Start(Helpers.Entities.GetAllPlayers()));
            Helpers.Entities.RegisterCoroutine(MelonCoroutines.Start(Helpers.Entities.GetMonkeys()));
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Insert))
            {
                try
                {
                    if (Player.IsInGame())
                    {
                        //drive the cursor directly so GameUI doesn't freeze the player while the menu is open
                        Cursor.visible = !Settings.Settings.menu_enable;
                        Cursor.lockState = Settings.Settings.menu_enable ? CursorLockMode.Locked : CursorLockMode.None;
                        Hacks.Misc.SetGameCursorCapture(Settings.Settings.menu_enable);
                    }
                    else
                    {
                        Il2Cpp.GameUI gameUI = UnityEngine.Object.FindObjectOfType<Il2Cpp.GameUI>();
                        if (Settings.Settings.menu_enable)
                        {
                            gameUI.HideMouseCursor();
                        }
                        else
                        {
                            gameUI.ShowMouseCursor();
                        }
                    }
                }
                catch { }

                Settings.Settings.menu_enable = !Settings.Settings.menu_enable;
            }

            //the game re-locks the cursor on its own, keep it free while the menu is open
            if (Settings.Settings.menu_enable && Player.IsInGame() && Cursor.lockState != CursorLockMode.None)
            {
                Hacks.Misc.SetGameCursorCapture(false);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            if (Player.IsInGame())
            {
                Il2Cpp.NolanBehaviour me = Player.GetPlayer();
                if (me != null && me.GetInstanceID() != perksAppliedFor)
                {
                    perksAppliedFor = me.GetInstanceID();
                    Hacks.Misc.ApplyExtraPerkStates(me);
                }

                if (chaseBoost && me != null)
                {
                    bool chased = Hacks.Misc.IsBeingChased(me);
                    float scale = chased ? chaseBoostMultiplier : (fastMove ? _PlayerSpeedMultiplier : 1f);
                    var loco = me.GetComponent<Il2CppOpsive.UltimateCharacterController.Character.UltimateCharacterLocomotion>();
                    if (loco != null && !Mathf.Approximately(loco.TimeScale, scale))
                    {
                        loco.TimeScale = scale;
                    }
                }

                if (flashlight_toggle && !fullbright)
                {
                    Hacks.Misc.BigFlashlight(false);
                }
                else if (!flashlight_toggle && !fullbright)
                {
                    Hacks.Misc.BigFlashlight(true);
                }

                if (fullbright && !flashlight_toggle)
                {
                    Hacks.Misc.Fullbright(false);
                }
                else if (!fullbright && !flashlight_toggle)
                {
                    Hacks.Misc.Fullbright(true);
                }

                if (_IsAutoRespawn && Helpers.Player.IsPlayerDowned())
                {
                    Hacks.Misc.AutoRespawn();
                }
                else
                {
                    Hacks.Misc.ResetAutoRespawn();
                }

                if (unlimitedUV)
                {
                    Hacks.Misc.RefillLocalUV(me);
                }

                if (crosshair && !in_game_cache)
                {
                    in_game_cache = true;
                }
            }
            else
            {

                if (crosshair && in_game_cache)
                {
                    in_game_cache = false;
                }
            }

            if (Input.GetKeyDown(Settings.Settings.flyKey))
            {
                fly = !fly;
            }

            if (Player.IsInGameOrLobby())
            {
                if (fly && !need_fly_reset)
                {
                    Il2Cpp.NolanBehaviour nb = Player.GetPlayer();
                    if (nb)
                    {
                        Collider coll = nb.GetComponentInChildren<Collider>();
                        if (coll)
                        {
                            coll.enabled = false;
                            need_fly_reset = true;
                        }
                    }
                }

                else if (!fly && need_fly_reset)
                {
                    Il2Cpp.NolanBehaviour nb = Player.GetPlayer();
                    if (nb)
                    {
                        Collider coll = nb.GetComponentInChildren<Collider>();
                        if (coll)
                        {
                            coll.enabled = true;
                            need_fly_reset = false;
                        }
                    }
                }

                if (fly)
                {
                    Hacks.Misc.Fly(fly_speed);
                }

            }

            if (Helpers.Map.GetActiveScene() == "Menu")
            {
                Hacks.Misc.WalkInLobby(_walkInLobby);
            }

            if (fastMove && !chaseBoost) // chase boost handles speed itself when it is on
            {
                try
                {
                    Helpers.Entities.LocalPlayer_.p_GameObject.GetComponent<Il2CppOpsive.UltimateCharacterController.Character.UltimateCharacterLocomotion>().TimeScale = _PlayerSpeedMultiplier;
                }
                catch { return;                 }
            }


            frameCount++;

            // No forced GC.Collect here: with a ~2 GB managed heap a blocking full collection froze the game
            // for a moment about once a minute. The runtime collects on its own without stalling the frame.
            if (frameCount >= GC_FULL_INTERVAL)
            {
                frameCount = 0;
                lastMemoryLog = 0;
            }

            if (frameCount - lastMemoryLog >= MEMORY_LOG_INTERVAL)
            {
                long memoryUsed = GC.GetTotalMemory(false);
                MelonLogger.Msg($"[Memory Monitor] Current managed memory: {memoryUsed / 1024 / 1024} MB | Frame: {frameCount}");
                lastMemoryLog = frameCount;
            }
        }

        public void OnGUI()
        {
            if (should_show_start_message)
            {
                if (DevourClient.Hacks.Misc.ShowMessageBox("Welcome to DevourClient.\n\nPress the INS key to open the menu.") == 0)
                    should_show_start_message = false;
            }

            // 保存原始GUI状态
            Color originalBackgroundColor = GUI.backgroundColor;
            GUISkin originalSkin = GUI.skin;

            try
            {
                GUI.backgroundColor = Color.grey;

                // 设置按钮样式
                GUI.skin.button.normal.background = GUIHelper.MakeTex(2, 2, Color.black);
                GUI.skin.button.normal.textColor = Color.white;
                GUI.skin.button.hover.background = GUIHelper.MakeTex(2, 2, Color.green);
                GUI.skin.button.hover.textColor = Color.black;

                // 设置切换按钮样式
                GUI.skin.toggle.onNormal.textColor = Color.yellow;

                // 设置文本框样式 - 这是关键修复
                GUI.skin.textField.normal.background = GUIHelper.MakeTex(2, 2, new Color(0.2f, 0.2f, 0.2f, 0.8f));
                GUI.skin.textField.normal.textColor = Color.white;
                GUI.skin.textField.focused.background = GUIHelper.MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f, 0.9f));
                GUI.skin.textField.focused.textColor = Color.yellow;
                GUI.skin.textField.border = new RectOffset(2, 2, 2, 2);

                // 设置标签样式
                GUI.skin.label.normal.textColor = Color.white;

                // 设置滑块样式
                GUI.skin.horizontalSlider.normal.background = GUIHelper.MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f, 0.8f));
                GUI.skin.horizontalSliderThumb.normal.background = GUIHelper.MakeTex(2, 2, Color.white);

            if (UnityEngine.Event.current.type == EventType.Repaint)
            {
                if (showStaggerHud && Player.IsInGame())
                {
                    string staggerText;
                    if (Time.time - azazelStaggerSeen < 2f)
                    {
                        int left = Math.Max(0, azazelStaggersNeeded - azazelStaggers);
                        staggerText = oneStaggerCalm
                            ? "Azazel: calms after the next stagger"
                            : $"Azazel staggers: {azazelStaggers}/{azazelStaggersNeeded} ({left} more to calm)";
                    }
                    else
                    {
                        staggerText = BoltNetwork.IsServer ? "Azazel staggers: waiting for Azazel" : "Azazel staggers: only the lobby owner can see this";
                    }
                    GUI.Label(new Rect(12, Screen.height - 34, 420, 24), staggerText);
                }

                if (player_esp || player_snapline || player_skel_esp)
                {
                    foreach (Helpers.BasePlayer p in Helpers.Entities.Players)
                    {
                        if (p == null)
                        {
                            continue;
                        }

                        GameObject player = p.p_GameObject;
                        if (player != null)
                        {
                            Il2Cpp.NolanBehaviour nb = player.GetComponent<Il2Cpp.NolanBehaviour>();
                            if (nb.entity.IsOwner)
                            {
                                continue;
                            }

                            if (player_skel_esp)
                            {
                                Render.Render.DrawAllBones(Hacks.Misc.GetAllBones(nb.animator), Settings.Settings.player_esp_color);
                            }

                            Render.Render.DrawBoxESP(player, -0.25f, 1.75f, p.Name, Settings.Settings.player_esp_color, player_snapline, player_esp);
                        }
                    }
                }

                if (goat_rat_esp)
                {
                    foreach (Il2Cpp.GoatBehaviour goat in Helpers.Entities.GoatsAndRats)
                    {
                        if (goat != null)
                        {
                            Render.Render.DrawNameESP(goat.transform.position, goat.name.Replace("Survival", "").Replace("(Clone)", ""), new Color(0.94f, 0.61f, 0.18f, 1.0f));
                        }
                    }
                }

                if (item_esp)
                {
                    foreach (Il2Cpp.SurvivalInteractable obj in Helpers.Entities.SurvivalInteractables)
                    {
                        if (obj != null)
                        {
                            Render.Render.DrawNameESP(obj.transform.position, obj.prefabName.Replace("Survival", ""), new Color(1.0f, 1.0f, 1.0f));
                        }
                    }

                    foreach (Il2Cpp.KeyBehaviour key in Helpers.Entities.Keys)
                    {
                        if (key != null)
                        {
                            Render.Render.DrawNameESP(key.transform.position, "Key", new Color(1.0f, 1.0f, 1.0f));
                        }
                    }
                }

                if (demon_esp)
                {
                    foreach (Il2Cpp.SurvivalDemonBehaviour demon in Helpers.Entities.Demons)
                    {
                        if (demon != null)
                        {
                            Render.Render.DrawNameESP(demon.transform.position, demon.name.Replace("Survival", "").Replace("(Clone)", ""), new Color(1.0f, 0.0f, 0.0f, 1.0f));
                        }
                    }

                    foreach (Il2Cpp.SpiderBehaviour spider in Helpers.Entities.Spiders)
                    {
                        if (spider != null)
                        {
                            Render.Render.DrawNameESP(spider.transform.position, "Spider", new Color(1.0f, 0.0f, 0.0f, 1.0f));
                        }
                    }

                    foreach (Il2Cpp.GhostBehaviour ghost in Helpers.Entities.Ghosts)
                    {
                        if (ghost != null)
                        {
                            Render.Render.DrawNameESP(ghost.transform.position, "Ghost", new Color(1.0f, 0.0f, 0.0f, 1.0f));
                        }
                    }

                    foreach (Il2Cpp.BoarBehaviour boar in Helpers.Entities.Boars)
                    {
                        if (boar != null)
                        {
                            Render.Render.DrawNameESP(boar.transform.position, "Boar", new Color(1.0f, 0.0f, 0.0f, 1.0f));
                        }
                    }

                    foreach (Il2Cpp.CorpseBehaviour corpse in Helpers.Entities.Corpses)
                    {
                        if (corpse != null)
                        {
                            Render.Render.DrawNameESP(corpse.transform.position, "Corpse", new Color(1.0f, 0.0f, 0.0f, 1.0f));
                        }
                    }

                    foreach (Il2Cpp.CrowBehaviour crow in Helpers.Entities.Crows)
                    {
                        if (crow != null)
                        {
                            Render.Render.DrawNameESP(crow.transform.position, "Crow", new Color(1.0f, 0.0f, 0.0f, 1.0f));
                        }
                    }

                    foreach (Il2Cpp.ManorLumpController lump in Helpers.Entities.Lumps)
                    {
                        if (lump != null)
                        {
                            Render.Render.DrawNameESP(lump.transform.position, "Lump", new Color(1.0f, 0.0f, 0.0f, 1.0f));
                        }
                    }
                    foreach (Il2Cpp.MonkeyBehaviour monkey in Helpers.Entities.Monkeys)
                    {
                        if (monkey != null)
                        {
                            Render.Render.DrawNameESP(monkey.transform.position, "Monkey", new Color(1.0f, 0.0f, 0.0f, 1.0f));
                        }
                    }
                }

                if (azazel_esp || azazel_snapline || azazel_skel_esp)
                {
                    foreach (Il2Cpp.SurvivalAzazelBehaviour survivalAzazel in Helpers.Entities.Azazels)
                    {
                        if (survivalAzazel != null)
                        {
                            if (azazel_skel_esp)
                            {
                                Render.Render.DrawAllBones(Hacks.Misc.GetAllBones(survivalAzazel.animator), Settings.Settings.azazel_esp_color);
                            }

                            Render.Render.DrawBoxESP(survivalAzazel.gameObject, -0.25f, 2.0f, "Azazel", Settings.Settings.azazel_esp_color, azazel_snapline, azazel_esp);
                        }
                    }
                }

                if (crosshair && in_game_cache)
                {
                    const float crosshairSize = 4;

                    float xMin = (Settings.Settings.width) - (crosshairSize / 2);
                    float yMin = (Settings.Settings.height) - (crosshairSize / 2);

                    if (crosshairTexture == null)
                    {
                        crosshairTexture = Helpers.GUIHelper.GetCircularTexture(5, 5);
                    }

                    GUI.DrawTexture(new Rect(xMin, yMin, crosshairSize, crosshairSize), crosshairTexture);
                }
            }

            if (Settings.Settings.menu_enable)
            {
                windowRect = GUI.Window(0, windowRect, (GUI.WindowFunction)Tabs, "DevourClient");
            }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Msg($"OnGUI Error: {ex.Message}");
            }
            finally
            {
                // 恢复原始GUI状态
                GUI.backgroundColor = originalBackgroundColor;
                GUI.skin = originalSkin;
            }
        }

        public static void Tabs(int windowID)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Visual", GUILayout.Height(40)) || Input.GetKeyDown(KeyCode.Alpha1))
            {
                current_tab = CurrentTab.Visuals;
            }

            if (GUILayout.Button("Entites", GUILayout.Height(40)) || Input.GetKeyDown(KeyCode.Alpha2))
            {
                current_tab = CurrentTab.Entities;
            }

            if (GUILayout.Button("Map Specific", GUILayout.Height(40)) || Input.GetKeyDown(KeyCode.Alpha3))
            {
                current_tab = CurrentTab.Map;
            }

            if (GUILayout.Button("ESP", GUILayout.Height(40)) || Input.GetKeyDown(KeyCode.Alpha4))
            {
                current_tab = CurrentTab.ESP;
            }

            if (GUILayout.Button("Items", GUILayout.Height(40)) || Input.GetKeyDown(KeyCode.Alpha5))
            {
                current_tab = CurrentTab.Items;
            }

            if (GUILayout.Button("Misc", GUILayout.Height(40)) || Input.GetKeyDown(KeyCode.Alpha6))
            {
                current_tab = CurrentTab.Misc;
            }

            if (GUILayout.Button("Player", GUILayout.Height(40)) || Input.GetKeyDown(KeyCode.Alpha7))
            {
                current_tab = CurrentTab.Players;
            }

            GUILayout.EndHorizontal();

            switch (current_tab)
            {
                case CurrentTab.Visuals:
                    VisualsTab();
                    break;
                case CurrentTab.Entities:
                    EntitiesTab();
                    break;
                case CurrentTab.Map:
                    MapSpecificTab();
                    break;
                case CurrentTab.ESP:
                    EspTab();
                    break;
                case CurrentTab.Items:
                    ItemsTab();
                    break;
                case CurrentTab.Misc:
                    MiscTab();
                    break;
                case CurrentTab.Players:
                    PlayersTab();
                    break;

            }

            GUI.DragWindow();
        }

        private static void VisualsTab()
        {
            // draw visuals

            flashlight_toggle = GUI.Toggle(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 70, 120, 30), flashlight_toggle, "Big Flashlight");
            fullbright = GUI.Toggle(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 100, 120, 30), fullbright, "Fullbright");
            unlimitedUV = GUI.Toggle(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 130, 130, 30), unlimitedUV, "Unlimited UV Light");
            crosshair = GUI.Toggle(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 160, 130, 30), crosshair, "Crosshair");


            if (GUI.Button(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 190, 130, 30), "Flashlight Color"))
            {
                flashlight_colorpick = !flashlight_colorpick;
                MelonLogger.Msg("Flashlight color picker : " + flashlight_colorpick.ToString());

            }

            if (flashlight_colorpick)
            {
                Color flashlight_color_input = DevourClient.Helpers.GUIHelper.ColorPick("Flashlight Color", Settings.Settings.flashlight_color);
                Settings.Settings.flashlight_color = flashlight_color_input;

                if (Player.IsInGame())
                {
                    Hacks.Misc.FlashlightColor(flashlight_color_input);
                }
            }
        }

        private static void EntitiesTab()
        {
            //draw entities

            if (GUI.Button(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 70, 130, 30), "TP items to you"))
            {
                Hacks.Misc.TPItems();
                MelonLogger.Msg("TP Items!");
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 110, 130, 30), "Freeze azazel"))
            {
                Hacks.Misc.FreezeAzazel();
            }

            oneStaggerCalm = GUI.Toggle(new Rect(Settings.Settings.x + 150, Settings.Settings.y + 115, 220, 20), oneStaggerCalm, "Calm Azazel after 1 stagger (owner)");
            showStaggerHud = GUI.Toggle(new Rect(Settings.Settings.x + 150, Settings.Settings.y + 75, 220, 20), showStaggerHud, "Show stagger counter");

            GUI.Label(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 150, 120, 30), "Azazel & Demons");

            if (GUI.Button(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 180, 60, 25), "Sam") && Player.IsInGameOrLobby() && BoltNetwork.IsServer)
            {
                Hacks.Misc.SpawnAzazel((PrefabId)BoltPrefabs.AzazelSam);
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 80, Settings.Settings.y + 180, 60, 25), "Molly") && Player.IsInGameOrLobby() && BoltNetwork.IsServer)
            {
                Hacks.Misc.SpawnAzazel((PrefabId)BoltPrefabs.SurvivalAzazelMolly);
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 150, Settings.Settings.y + 180, 60, 25), "Anna") && Player.IsInGameOrLobby() && BoltNetwork.IsServer)
            {
                BoltNetwork.Instantiate(BoltPrefabs.SurvivalAnnaNew, Player.GetPlayer().transform.position, Quaternion.identity);
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 220, Settings.Settings.y + 180, 60, 25), "Zara") && Player.IsInGameOrLobby() && BoltNetwork.IsServer)
            {
                BoltNetwork.Instantiate(BoltPrefabs.AzazelZara, Player.GetPlayer().transform.position, Quaternion.identity);
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 290, Settings.Settings.y + 180, 60, 25), "Nathan") && Player.IsInGameOrLobby() && BoltNetwork.IsServer)
            {
                BoltNetwork.Instantiate(BoltPrefabs.AzazelNathan, Player.GetPlayer().transform.position, Quaternion.identity);
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 360, Settings.Settings.y + 180, 60, 25), "April") && Player.IsInGameOrLobby() && BoltNetwork.IsServer)
            {
                BoltNetwork.Instantiate(BoltPrefabs.AzazelApril, Player.GetPlayer().transform.position, Quaternion.identity);
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 430, Settings.Settings.y + 180, 60, 25), "Kai") && Player.IsInGameOrLobby() && BoltNetwork.IsServer)
            {
                BoltNetwork.Instantiate(BoltPrefabs.AzazelKai, Player.GetPlayer().transform.position, Quaternion.identity);
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 220, 60, 25), "Ghost") && Player.IsInGameOrLobby() && BoltNetwork.IsServer)
            {
                BoltNetwork.Instantiate(BoltPrefabs.Ghost, Player.GetPlayer().transform.position, Quaternion.identity);
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 80, Settings.Settings.y + 220, 60, 25), "Inmate") && Player.IsInGameOrLobby() && BoltNetwork.IsServer)
            {
                BoltNetwork.Instantiate(BoltPrefabs.SurvivalInmate, Player.GetPlayer().transform.position, Quaternion.identity);
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 150, Settings.Settings.y + 220, 60, 25), "Demon") && Player.IsInGameOrLobby() && BoltNetwork.IsServer)
            {
                BoltNetwork.Instantiate(BoltPrefabs.SurvivalDemon, Player.GetPlayer().transform.position, Quaternion.identity);
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 220, Settings.Settings.y + 220, 60, 25), "Boar") && Player.IsInGameOrLobby() && BoltNetwork.IsServer)
            {
                BoltNetwork.Instantiate(BoltPrefabs.Boar, Player.GetPlayer().transform.position, Quaternion.identity);
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 290, Settings.Settings.y + 220, 60, 25), "Corpse") && BoltNetwork.IsServer && Player.IsInGameOrLobby())
            {
                BoltNetwork.Instantiate(BoltPrefabs.Corpse, Player.GetPlayer().transform.position, Quaternion.identity);
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 360, Settings.Settings.y + 220, 60, 25), "Crow") && BoltNetwork.IsServer && Player.IsInGameOrLobby())
            {
                BoltNetwork.Instantiate(BoltPrefabs.Crow, Player.GetPlayer().transform.position, Quaternion.identity);
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 430, Settings.Settings.y + 220, 60, 25), "Lump") && BoltNetwork.IsServer && Player.IsInGameOrLobby())
            {
                BoltNetwork.Instantiate(BoltPrefabs.ManorLump, Player.GetPlayer().transform.position, Quaternion.identity);
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 500, Settings.Settings.y + 220, 60, 25), "Monkey") && BoltNetwork.IsServer && Player.IsInGameOrLobby())
            {
                BoltNetwork.Instantiate(BoltPrefabs.Monkey, Player.GetPlayer().transform.position, Quaternion.identity);
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 260, 60, 25), "Rat"))
            {
                if (BoltNetwork.IsServer && !Player.IsInGame())
                {
                    BoltNetwork.Instantiate(BoltPrefabs.SurvivalRat, Player.GetPlayer().transform.position, Quaternion.identity);
                }

                if (Player.IsInGame() && !Player.IsPlayerCrawling())
                {
                    Hacks.Misc.CarryObject("SurvivalRat");
                }
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 80, Settings.Settings.y + 260, 60, 25), "Goat"))
            {
                if (BoltNetwork.IsServer && !Player.IsInGame())
                {
                    BoltNetwork.Instantiate(BoltPrefabs.SurvivalGoat, Player.GetPlayer().transform.position, Quaternion.identity);
                }

                if (Player.IsInGame() && !Player.IsPlayerCrawling())
                {
                    Hacks.Misc.CarryObject("SurvivalGoat");
                }
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 150, Settings.Settings.y + 260, 60, 25), "Spider") && BoltNetwork.IsServer && Player.IsInGameOrLobby())
            {
                BoltNetwork.Instantiate(BoltPrefabs.Spider, Player.GetPlayer().transform.position, Quaternion.identity);
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 220, Settings.Settings.y + 260, 60, 25), "Pig"))
            {
                if (BoltNetwork.IsServer && !Player.IsInGame())
                {
                    BoltNetwork.Instantiate(BoltPrefabs.SurvivalPig, Player.GetPlayer().transform.position, Quaternion.identity);
                }

                if (Player.IsInGame() && !Player.IsPlayerCrawling())
                {
                    Hacks.Misc.CarryObject("SurvivalPig");
                }
            }    
        }

        private static void MapSpecificTab()
        {
            if (GUI.Button(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 70, 150, 30), "Instant Win") && Player.IsInGame())
            {
                if (BoltNetwork.IsServer)
                {
                    Hacks.Misc.InstantWin();
                    MelonLogger.Msg("EZ Win");
                }
                else
                {
                    MelonLogger.Warning("You must be the host to use this command!");
                    Hacks.Misc.ShowMessageBox("You must be the host to use Instant Win!");
                }
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 110, 150, 30), "Burn a ritual object"))
            {
                Hacks.Misc.BurnRitualObj(Helpers.Map.GetActiveScene(), false);
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 150, 150, 30), "Burn all ritual objects"))
            {
                Hacks.Misc.BurnRitualObj(Helpers.Map.GetActiveScene(), true);
            }

            switch (Helpers.Map.GetActiveScene())
            {
                case "Menu":
                    if (GUI.Button(new Rect(Settings.Settings.x + 190, Settings.Settings.y + 70, 150, 30), "Force Start Game") && BoltNetwork.IsServer && !Player.IsInGame())
                    {
                        Il2CppHorror.Menu menu = UnityEngine.Object.FindObjectOfType<Il2CppHorror.Menu>();
                        menu.OnLobbyStartButtonClick();
                    }
                    break;

                case "Devour":
                    if (GUI.Button(new Rect(Settings.Settings.x + 190, Settings.Settings.y + 70, 150, 30), "TP to Azazel"))
                    {
                        try
                        {
                            Il2Cpp.NolanBehaviour nb = Player.GetPlayer();

                            nb.TeleportTo(Helpers.Map.GetAzazel().transform.position, Quaternion.identity);
                        }
                        catch
                        {
                            MelonLogger.Msg("Azazel not found !");
                        }
                    }

                    if (GUI.Button(new Rect(Settings.Settings.x + 190, Settings.Settings.y + 110, 150, 30), "Despawn Demons"))
                    {
                        Hacks.Misc.DespawnDemons();
                    }
                    break;
                case "Molly":
                    if (GUI.Button(new Rect(Settings.Settings.x + 190, Settings.Settings.y + 70, 150, 30), "TP to Azazel"))
                    {
                        try
                        {
                            Il2Cpp.NolanBehaviour nb = Player.GetPlayer();

                            nb.TeleportTo(Helpers.Map.GetAzazel().transform.position, Quaternion.identity);
                        }
                        catch
                        {
                            MelonLogger.Msg("Azazel not found !");
                        }
                    }

                    if (GUI.Button(new Rect(Settings.Settings.x + 190, Settings.Settings.y + 110, 150, 30), "Despawn Inmates"))
                    {
                        Hacks.Misc.DespawnDemons();
                    }
                    break;
                case "Inn":
                    if (GUI.Button(new Rect(Settings.Settings.x + 190, Settings.Settings.y + 70, 150, 30), "TP to Azazel"))
                    {
                        try
                        {
                            Il2Cpp.NolanBehaviour nb = Player.GetPlayer();

                            nb.TeleportTo(Helpers.Map.GetAzazel().transform.position, Quaternion.identity);
                        }
                        catch
                        {
                            MelonLogger.Msg("Azazel not found !");
                        }
                    }

                    if (GUI.Button(new Rect(Settings.Settings.x + 190, Settings.Settings.y + 110, 150, 30), "Clean The Fountains"))
                    {
                        Hacks.Misc.CleanFountain();
                    }

                    if (GUI.Button(new Rect(Settings.Settings.x + 190, Settings.Settings.y + 150, 150, 30), "Despawn Spiders"))
                    {
                        Hacks.Misc.DespawnSpiders();
                    }
                    break;

                case "Town":
                    if (GUI.Button(new Rect(Settings.Settings.x + 190, Settings.Settings.y + 70, 150, 30), "TP to Azazel"))
                    {
                        try
                        {
                            Il2Cpp.NolanBehaviour nb = Player.GetPlayer();

                            nb.TeleportTo(Helpers.Map.GetAzazel().transform.position, Quaternion.identity);
                        }
                        catch
                        {
                            MelonLogger.Msg("Azazel not found !");
                        }
                    }

                    if (GUI.Button(new Rect(Settings.Settings.x + 190, Settings.Settings.y + 110, 150, 30), "Despawn Ghosts"))
                    {
                        Hacks.Misc.DespawnGhosts();
                    }
                    break;

                case "Slaughterhouse":
                    if (GUI.Button(new Rect(Settings.Settings.x + 190, Settings.Settings.y + 70, 150, 30), "TP to Azazel"))
                    {
                        try
                        {
                            Il2Cpp.NolanBehaviour nb = Player.GetPlayer();

                            nb.TeleportTo(Helpers.Map.GetAzazel().transform.position, Quaternion.identity);
                        }
                        catch
                        {
                            MelonLogger.Msg("Azazel not found !");
                        }
                    }

                    if (GUI.Button(new Rect(Settings.Settings.x + 190, Settings.Settings.y + 110, 150, 30), "Despawn Boars"))
                    {
                        Hacks.Misc.DespawnBoars();
                    }

                    if (GUI.Button(new Rect(Settings.Settings.x + 190, Settings.Settings.y + 150, 150, 30), "Despawn Corpses"))
                    {
                        Hacks.Misc.DespawnCorpses();
                    }
                    break;

                case "Manor":
                    if (GUI.Button(new Rect(Settings.Settings.x + 190, Settings.Settings.y + 70, 150, 30), "TP to Azazel"))
                    {
                        try
                        {
                            Il2Cpp.NolanBehaviour nb = Player.GetPlayer();

                            nb.TeleportTo(Helpers.Map.GetAzazel().transform.position, Quaternion.identity);
                        }
                        catch
                        {
                            MelonLogger.Msg("Azazel not found !");
                        }
                    }

                    if (GUI.Button(new Rect(Settings.Settings.x + 190, Settings.Settings.y + 110, 150, 30), "Despawn Crows"))
                    {
                        Hacks.Misc.DespawnCrows();
                    }

                    if (GUI.Button(new Rect(Settings.Settings.x + 190, Settings.Settings.y + 150, 150, 30), "Despawn Lumps"))
                    {
                        Hacks.Misc.DespawnLumps();
                    }

                    if (GUI.Button(new Rect(Settings.Settings.x + 370, Settings.Settings.y + 70, 150, 30), "Switch realm"))
                    {
                        Il2Cpp.NolanBehaviour nb = Player.GetPlayer();
                        Vector3 pos = nb.transform.position;

                        ManorDeadRealmTrigger realm = Il2Cpp.ManorDeadRealmTrigger.FindObjectOfType<Il2Cpp.ManorDeadRealmTrigger>();
                        if (realm == null)
                        {
                            MelonLogger.Warning("realm was null.");
                            return;
                        }

                        if (realm.IsInDeadRealm)
                        {
                            // normal dimension
                            pos.x += 150f;// -10.216758f;
                            //pos.y = 0.009999979f;
                            //pos.z = -7.632657f;

                        }
                        else
                        {
                            // other dimension
                            pos.x -= 150f;//-160.03688f;
                            //pos.y = 0.010014875f;
                            //pos.z = -7.5686994f;
                        }

                        nb.locomotion.SetPosition(pos, false);
                    }

                    if (GUI.Button(new Rect(Settings.Settings.x + 370, Settings.Settings.y + 110, 150, 30), "Switch realm (house)"))
                    {
                        Il2Cpp.NolanBehaviour nb = Player.GetPlayer();
                        Vector3 pos = nb.transform.position;

                        ManorDeadRealmTrigger realm = Il2Cpp.ManorDeadRealmTrigger.FindObjectOfType<Il2Cpp.ManorDeadRealmTrigger>();
                        if (realm == null)
                        {
                            MelonLogger.Warning("realm was null.");
                            return;
                        }

                        if (realm.IsInDeadRealm)
                        {
                            // normal dimension
                            pos.x = -10.216758f;
                            pos.y = 0.009999979f;
                            pos.z = -7.632657f;

                        }
                        else
                        {
                            // other dimension
                            pos.x = -160.03688f;
                            pos.y = 0.010014875f;
                            pos.z = -7.5686994f;
                        }

                        nb.locomotion.SetPosition(pos, false);
                    }

                    infinite_mirrors = GUI.Toggle(new Rect(Settings.Settings.x + 370, Settings.Settings.y + 150, 150, 20), infinite_mirrors, "Infinite mirrors");
                    break;

                case "Carnival":
                    if (GUI.Button(new Rect(Settings.Settings.x + 190, Settings.Settings.y + 70, 150, 30), "TP to Azazel"))
                    {
                        try
                        {
                            Il2Cpp.NolanBehaviour nb = Player.GetPlayer();

                            nb.TeleportTo(Helpers.Map.GetAzazel().transform.position, Quaternion.identity);
                        }
                        catch
                        {
                            MelonLogger.Msg("Azazel not found !");
                        }
                    }

                    if (GUI.Button(new Rect(Settings.Settings.x + 190, Settings.Settings.y + 110, 150, 30), "Despawn Monkeys"))
                    {
                        Hacks.Misc.DespawnMonkeys();
                    }
                    break;
            }

            GUI.Label(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 210, 100, 30), "Load Map: ");

            if (GUI.Button(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 240, 90, 30), "Farmhouse") && BoltNetwork.IsServer)
            {
                Helpers.Map.LoadMap("Devour");
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 110, Settings.Settings.y + 240, 90, 30), "Asylum") && BoltNetwork.IsServer)
            {
                Helpers.Map.LoadMap("Molly");
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 210, Settings.Settings.y + 240, 90, 30), "Inn") && BoltNetwork.IsServer)
            {
                Helpers.Map.LoadMap("Inn");
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 310, Settings.Settings.y + 240, 90, 30), "Town") && BoltNetwork.IsServer)
            {
                Helpers.Map.LoadMap("Town");
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 280, 90, 30), "Slaughterhouse") && BoltNetwork.IsServer)
            {
                Helpers.Map.LoadMap("Slaughterhouse");
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 110, Settings.Settings.y + 280, 90, 30), "Manor") && BoltNetwork.IsServer)
            {
                Helpers.Map.LoadMap("Manor");
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 210, Settings.Settings.y + 280, 90, 30), "Carnival") && BoltNetwork.IsServer)
            {
                Helpers.Map.LoadMap("Carnival");
            }

        }

        private static void EspTab()
        {
            player_esp = GUI.Toggle(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 70, 150, 20), player_esp, "Player ESP");
            player_skel_esp = GUI.Toggle(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 100, 150, 20), player_skel_esp, "Skeleton ESP");
            player_snapline = GUI.Toggle(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 130, 150, 20), player_snapline, "Player Snapline");
            if (GUI.Button(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 160, 130, 30), "Player ESP Color"))
            {
                player_esp_colorpick = !player_esp_colorpick;
            }

            if (player_esp_colorpick)
            {
                Color player_esp_color_input = GUIHelper.ColorPick("Player ESP Color", Settings.Settings.player_esp_color);
                Settings.Settings.player_esp_color = player_esp_color_input;
            }

            azazel_esp = GUI.Toggle(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 200, 150, 20), azazel_esp, "Azazel ESP");
            azazel_skel_esp = GUI.Toggle(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 230, 150, 20), azazel_skel_esp, "Skeleton ESP");
            azazel_snapline = GUI.Toggle(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 260, 150, 20), azazel_snapline, "Azazel Snapline");
            if (GUI.Button(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 290, 130, 30), "Azazel ESP Color"))
            {
                azazel_esp_colorpick = !azazel_esp_colorpick;
            }

            if (azazel_esp_colorpick)
            {
                Color azazel_esp_color_input = GUIHelper.ColorPick("Azazel ESP Color", Settings.Settings.azazel_esp_color);
                Settings.Settings.azazel_esp_color = azazel_esp_color_input;
            }

            item_esp = GUI.Toggle(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 330, 150, 20), item_esp, "Item ESP");
            goat_rat_esp = GUI.Toggle(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 360, 150, 20), goat_rat_esp, "Goat/Rat ESP");
            demon_esp = GUI.Toggle(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 390, 150, 20), demon_esp, "Demon ESP");
        }

        private static void ItemsTab()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Spawn {Hacks.Misc.spawnBatch} at a time (more than 1 needs lobby owner)", GUILayout.Width(330));
            Hacks.Misc.spawnBatch = (int)GUILayout.HorizontalSlider(Hacks.Misc.spawnBatch, 1f, 20f, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();

            GUILayout.Label("Items");

            Settings.Settings.itemsScrollPosition = GUILayout.BeginScrollView(Settings.Settings.itemsScrollPosition, GUILayout.Width(220), GUILayout.Height(190));

            if (GUILayout.Button("Hay"))
            {
                Hacks.Misc.SpawnItem(BoltPrefabs.SurvivalHay, "SurvivalHay");
            }

            if (GUILayout.Button("First aid"))
            {
                Hacks.Misc.SpawnItem(BoltPrefabs.SurvivalFirstAid, "SurvivalFirstAid");
            }

            if (GUILayout.Button("Battery"))
            {
                Hacks.Misc.SpawnItem(BoltPrefabs.SurvivalBattery, "SurvivalBattery");
            }

            if (GUILayout.Button("Gasoline"))
            {
                Hacks.Misc.SpawnItem(BoltPrefabs.SurvivalGasoline, "SurvivalGasoline");
            }

            if (GUILayout.Button("Fuse"))
            {
                Hacks.Misc.SpawnItem(BoltPrefabs.SurvivalFuse, "SurvivalFuse");
            }

            if (GUILayout.Button("Food"))
            {
                Hacks.Misc.SpawnItem(BoltPrefabs.SurvivalRottenFood, "SurvivalRottenFood");
            }

            if (GUILayout.Button("Bone"))
            {

                Hacks.Misc.SpawnItem(BoltPrefabs.SurvivalBone, "SurvivalBone");
            }

            if (GUILayout.Button("Bleach"))
            {
                Hacks.Misc.SpawnItem(BoltPrefabs.SurvivalBleach, "SurvivalBleach");
            }

            if (GUILayout.Button("Matchbox"))
            {
                Hacks.Misc.SpawnItem(BoltPrefabs.SurvivalMatchbox, "Matchbox-3");
            }

            if (GUILayout.Button("Spade"))
            {
                Hacks.Misc.SpawnItem(BoltPrefabs.SurvivalSpade, "SurvivalSpade");
            }

            if (GUILayout.Button("Cake"))
            {
                Hacks.Misc.SpawnItem(BoltPrefabs.SurvivalCake, "SurvivalCake");
            }
            if (GUILayout.Button("MusicBox"))
            {
                Hacks.Misc.SpawnItem(BoltPrefabs.SurvivalMusicBox, "MusicBox-Idle");
            }
            if (GUILayout.Button("Coin"))
            {
                Hacks.Misc.SpawnItem(BoltPrefabs.SurvivalCoin, "SurvivalCoin");
            }


            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.BeginVertical();

            GUILayout.Label("Ritual Objects");

            Settings.Settings.rituelObjectsScrollPosition = GUILayout.BeginScrollView(Settings.Settings.rituelObjectsScrollPosition, GUILayout.Width(220), GUILayout.Height(190));

            if (GUILayout.Button("Egg-1"))
            {
                Hacks.Misc.CarryObject("Egg-Clean-1");
            }

            if (GUILayout.Button("Egg-2"))
            {
                Hacks.Misc.CarryObject("Egg-Clean-2");
            }

            if (GUILayout.Button("Egg-3"))
            {
                Hacks.Misc.CarryObject("Egg-Clean-3");
            }

            if (GUILayout.Button("Egg-4"))
            {
                Hacks.Misc.CarryObject("Egg-Clean-4");
            }

            if (GUILayout.Button("Egg-5"))
            {
                Hacks.Misc.CarryObject("Egg-Clean-5");
            }

            if (GUILayout.Button("Egg-6"))
            {
                Hacks.Misc.CarryObject("Egg-Clean-6");
            }

            if (GUILayout.Button("Egg-7"))
            {
                Hacks.Misc.CarryObject("Egg-Clean-7");
            }

            if (GUILayout.Button("Egg-8"))
            {
                Hacks.Misc.CarryObject("Egg-Clean-8");
            }

            if (GUILayout.Button("Egg-9"))
            {
                Hacks.Misc.CarryObject("Egg-Clean-9");
            }

            if (GUILayout.Button("Egg-10"))
            {
                Hacks.Misc.CarryObject("Egg-Clean-10");
            }

            if (GUILayout.Button("Ritual Book"))
            {
                Hacks.Misc.SpawnItem(BoltPrefabs.SurvivalRitualBook, "RitualBook-Active-1");
            }
     
            // if (GUILayout.Button("Dirty head"))
            // {
            //     if (BoltNetwork.IsServer && !Player.IsInGame())
            //     {
            //         BoltNetwork.Instantiate(BoltPrefabs.SurvivalHead, Player.GetPlayer().transform.position, Quaternion.identity);
            //     }
            //     else
            //     {
            //         Hacks.Misc.CarryObject("SurvivalHead");
            //     }
            // }

            // if (GUILayout.Button("Clean head"))
            // {
            //     if (BoltNetwork.IsServer && !Player.IsInGame())
            //     {
            //         BoltNetwork.Instantiate(BoltPrefabs.SurvivalCleanHead, Player.GetPlayer().transform.position, Quaternion.identity);
            //     }
            //     else
            //     {
            //         Hacks.Misc.CarryObject("SurvivalCleanHead");
            //     }
            // }
            if (GUILayout.Button("Doll Head"))
            {
                Hacks.Misc.SpawnItem(BoltPrefabs.SurvivalDollHead, "SurvivalDollHead");
            }   

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.BeginVertical();

            GUILayout.Label("Spawnable Prefabs");

            Settings.Settings.stuffsScrollPosition = GUILayout.BeginScrollView(Settings.Settings.stuffsScrollPosition, GUILayout.Width(220), GUILayout.Height(190));

            if (GUILayout.Button("Animal_Gate"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.Animal_Gate, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("AsylumDoor"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.AsylumDoor, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("AsylumDoubleDoor"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.AsylumDoubleDoor, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("AsylumWhiteDoor"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.AsylumWhiteDoor, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }


            if (GUILayout.Button("DevourDoorBack"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.DevourDoorBack, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("DevourDoorMain"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.DevourDoorMain, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("DevourDoorRoom"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.DevourDoorRoom, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("Elevator_Door"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.Elevator_Door, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("InnDoor"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.InnDoor, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("InnDoubleDoor"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.InnDoubleDoor, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("InnShojiDoor"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.InnShojiDoor, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("InnShrine"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.InnShrine, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("InnWardrobe"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.InnWardrobe, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("InnWoodenDoor"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.InnWoodenDoor, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("PigExcrement"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.PigExcrement, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("SlaughterhouseFireEscapeDoor"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.SlaughterhouseFireEscapeDoor, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("SurvivalAltarMolly"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.SurvivalAltarMolly, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("SurvivalAltarSlaughterhouse"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.SurvivalAltarSlaughterhouse, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("SurvivalAltarTown"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.SurvivalAltarTown, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }


            if (GUILayout.Button("SurvivalCultist"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.SurvivalCultist, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("SurvivalKai"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.SurvivalKai, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("SurvivalNathan"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.SurvivalNathan, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("SurvivalMolly"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.SurvivalMolly, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("SurvivalApril"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.SurvivalApril, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("SurvivalFrank"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.SurvivalFrank, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("SurvivalRose"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.SurvivalRose, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("SurvivalSmashableWindow"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.SurvivalSmashableWindow, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("TownDoor"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.TownDoor, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("TownDoor2"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.TownDoor2, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("TownPentagram"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.TownPentagram, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("TrashCan"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.TrashCan, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("Truck_Shutter"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.Truck_Shutter, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("TV"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.TV, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            if (GUILayout.Button("Mirror"))
            {
                if (BoltNetwork.IsServer)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.ManorMirror, Player.GetPlayer().transform.position, Quaternion.identity);
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            // Built from whatever is lying around the current map, so each map shows its own items.
            GUILayout.Label("TP to me: click for the closest one, Shift+click for all of that type");
            Settings.Settings.tpItemsScrollPosition = GUILayout.BeginScrollView(Settings.Settings.tpItemsScrollPosition, GUILayout.Width(680), GUILayout.Height(110));
            if (Player.IsInGame())
            {
                int col = 0;
                GUILayout.BeginHorizontal();
                foreach (var group in Hacks.Misc.GetMapItems())
                {
                    if (GUILayout.Button($"{group.Key} ({group.Value.Count})", GUILayout.Width(160)))
                    {
                        Hacks.Misc.TPItemGroup(group.Value, UnityEngine.Event.current.shift);
                    }
                    if (++col % 4 == 0)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                    }
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("Start a round to see this map's items.");
            }
            GUILayout.EndScrollView();
        }

        private static void MiscTab()
        {
            // === 游戏功能按钮区域 (左上) ===
            if (GUI.Button(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 70, 140, 25), "Unlock Achievements"))
            {
                Thread AchievementsThread = new Thread(new ThreadStart(Hacks.Unlock.Achievements));
                AchievementsThread.Start();

                MelonLogger.Msg("Achievements unlocked!");
                Hacks.Misc.ShowMessageBox("Achievements unlocked!");
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 160, Settings.Settings.y + 70, 140, 25), "Unlock Doors"))
            {
                Hacks.Unlock.Doors();

                MelonLogger.Msg("Doors unlocked!");
                Hacks.Misc.ShowMessageBox("Doors unlocked!");
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 105, 140, 25), "Teleport Keys") && Player.IsInGame())
            {
                Hacks.Misc.TPKeys();
                MelonLogger.Msg("Keys teleported!");
                Hacks.Misc.ShowMessageBox("Keys teleported!");
            }

            if (GUI.Button(new Rect(Settings.Settings.x + 160, Settings.Settings.y + 105, 140, 25), "Play Random Sound"))
            {
                Hacks.Misc.PlaySound();
                MelonLogger.Msg("Playing random sound!");
                Hacks.Misc.ShowMessageBox("Playing random sound!");
            }

            // === 基础开关区域 (左中) ===
            _walkInLobby = GUI.Toggle(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 150, 140, 20), _walkInLobby, "Walk In Lobby");
            _IsAutoRespawn = GUI.Toggle(new Rect(Settings.Settings.x + 160, Settings.Settings.y + 150, 140, 20), _IsAutoRespawn, "Auto Respawn");

            chaseBoost = GUI.Toggle(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 335, 140, 20), chaseBoost, "Chase speed boost");
            chaseBoostMultiplier = Mathf.Round(GUI.HorizontalSlider(new Rect(Settings.Settings.x + 160, Settings.Settings.y + 340, 80, 10), chaseBoostMultiplier, 1f, 3f) * 10f) / 10f;
            GUI.Label(new Rect(Settings.Settings.x + 250, Settings.Settings.y + 335, 50, 20), "x" + chaseBoostMultiplier.ToString("0.0"));

            if (GUI.Button(new Rect(Settings.Settings.x + 160, Settings.Settings.y + 300, 140, 25), "Revive Me") && Player.IsInGame())
            {
                Helpers.Player.Revive(Helpers.Entities.LocalPlayer_.p_GameObject);
            }

            // === Extra perks (stack on top of the two picked in the lobby) ===
            GUI.Label(new Rect(Settings.Settings.x + 330, Settings.Settings.y + 70, 220, 20), $"Extra perks ({extraPerks.Count} on)");
            Settings.Settings.perksScrollPosition = GUI.BeginScrollView(new Rect(Settings.Settings.x + 330, Settings.Settings.y + 92, 230, 240), Settings.Settings.perksScrollPosition, new Rect(0, 0, 205, perkIds.Length * 22));
            for (int p = 0; p < perkIds.Length; p++)
            {
                string id = perkIds[p];
                bool was = extraPerks.Contains(Hacks.Misc.PerkKey(id));
                string label = System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(id.Replace("PERK_", "").Replace('_', ' ').ToLowerInvariant());
                bool now = GUI.Toggle(new Rect(0, p * 22, 200, 20), was, label);
                if (now != was)
                {
                    Hacks.Misc.SetExtraPerk(id, now);
                }
            }
            GUI.EndScrollView();

            // === 飞行控制区域 (左中下) ===
            fly = GUI.Toggle(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 180, 40, 20), fly, "Fly");
            if (GUI.Button(new Rect(Settings.Settings.x + 60, Settings.Settings.y + 180, 50, 20), Settings.Settings.flyKey.ToString()))
            {
                Settings.Settings.flyKey = Settings.Settings.GetKey();
            }
            GUI.Label(new Rect(Settings.Settings.x + 120, Settings.Settings.y + 180, 80, 20), "Speed:");
            fly_speed = GUI.HorizontalSlider(new Rect(Settings.Settings.x + 170, Settings.Settings.y + 185, 80, 10), fly_speed, 5f, 20f);
            GUI.Label(new Rect(Settings.Settings.x + 260, Settings.Settings.y + 180, 40, 20), ((int)fly_speed).ToString());

            // === 等级欺骗区域 (左下) ===
            spoofLevel = GUI.Toggle(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 210, 100, 20), spoofLevel, "Spoof Level");
            spoofLevelValue = GUI.HorizontalSlider(new Rect(Settings.Settings.x + 120, Settings.Settings.y + 215, 80, 10), spoofLevelValue, 0f, 666f);
            GUI.Label(new Rect(Settings.Settings.x + 210, Settings.Settings.y + 210, 50, 20), ((int)spoofLevelValue).ToString());

            // === 经验修改区域 (左下) ===
            exp_modifier = GUI.Toggle(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 240, 100, 20), exp_modifier, "Exp Multiplier");
            exp = Mathf.Round(GUI.HorizontalSlider(new Rect(Settings.Settings.x + 120, Settings.Settings.y + 245, 80, 10), exp, 1f, 10f));
            GUI.Label(new Rect(Settings.Settings.x + 210, Settings.Settings.y + 240, 50, 20), "x" + ((int)exp).ToString());

            // === 速度修改区域 (左下) ===
            fastMove = GUI.Toggle(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 270, 100, 20), fastMove, "Player Speed");
            _PlayerSpeedMultiplier = GUI.HorizontalSlider(new Rect(Settings.Settings.x + 120, Settings.Settings.y + 275, 80, 10), _PlayerSpeedMultiplier, (int)1f, (int)10f);
            GUI.Label(new Rect(Settings.Settings.x + 210, Settings.Settings.y + 270, 50, 20), ((int)_PlayerSpeedMultiplier).ToString());

        }

        private static void PlayersTab()
        {
            if (Helpers.Map.GetActiveScene() != "Menu")
            {
                GUI.Label(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 70, 150, 30), "Player list:");
                int i = 0;
                foreach (BasePlayer bp in Entities.Players)
                {
                    if (bp == null || bp.Name == "")
                    {
                        MelonLogger.Warning("players is null");
                        continue;
                    }

                    GUI.Label(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 110 + i, 150, 30), bp.Name);

                    if (GUI.Button(new Rect(Settings.Settings.x + 70, Settings.Settings.y + 105 + i, 60, 30), "Kill"))
                    {
                        bp.Kill();
                    }

                    if (GUI.Button(new Rect(Settings.Settings.x + 140, Settings.Settings.y + 105 + i, 60, 30), "Revive"))
                    {
                        bp.Revive();
                    }

                    if (GUI.Button(new Rect(Settings.Settings.x + 210, Settings.Settings.y + 105 + i, 90, 30), "Jumpscare"))
                    {
                        bp.Jumpscare();
                    }

                    if (GUI.Button(new Rect(Settings.Settings.x + 310, Settings.Settings.y + 105 + i, 60, 30), "TP to"))
                    {
                        bp.TP();
                    }

                    if (GUI.Button(new Rect(Settings.Settings.x + 380, Settings.Settings.y + 105 + i, 100, 30), "Lock in cage"))
                    {
                        bp.LockInCage();
                    }

                    if (GUI.Button(new Rect(Settings.Settings.x + 490, Settings.Settings.y + 105 + i, 90, 30), "TP Azazel"))
                    {
                        bp.TPAzazel();
                    }

                    if (Helpers.Map.GetActiveScene() == "Town")
                    {
                        if (GUI.Button(new Rect(Settings.Settings.x + 590, Settings.Settings.y + 105 + i, 90, 30), "Shoot Player"))
                        {
                            bp.ShootPlayer();
                        }
                    }

                    i += 30;
                }
            }
            else
            {
                GUI.Label(new Rect(Settings.Settings.x + 10, Settings.Settings.y + 70, 150, 30), "Waiting for the game to start.");
            }
        }

        private void OnDestroy()
        {
            try
            {
                base.StopAllCoroutines();
                
                Dispose();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error in OnDestroy: {ex.Message}");
            }
        }

        private void OnApplicationQuit()
        {
            try
            {
                Dispose();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error in OnApplicationQuit: {ex.Message}");
            }
        }

        public void Dispose()
        {
            try
            {
                MelonLogger.Msg("Starting ClientMain cleanup...");

                long memoryBeforeCleanup = GC.GetTotalMemory(false);
                MelonLogger.Msg($"[Memory Monitor] Pre-cleanup managed memory: {memoryBeforeCleanup / 1024 / 1024} MB");

                Helpers.Entities.StopAllCoroutines();

                if (crosshairTexture != null)
                {
                    UnityEngine.Object.DestroyImmediate(crosshairTexture);
                    crosshairTexture = null;
                }

                Helpers.GUIHelper.Cleanup();

                Helpers.Entities.CleanupCachedObjects();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                long memoryAfterCleanup = GC.GetTotalMemory(true);
                long memoryFreed = memoryBeforeCleanup - memoryAfterCleanup;
                MelonLogger.Msg($"[Memory Monitor] Post-cleanup managed memory: {memoryAfterCleanup / 1024 / 1024} MB");
                MelonLogger.Msg($"[Memory Monitor] Memory freed: {memoryFreed / 1024 / 1024} MB");

                MelonLogger.Msg("ClientMain disposed successfully.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error disposing ClientMain: {ex.Message}");
            }
        }
    }
}

