using UnityEngine;

namespace DevourClient.Settings
{
    public class Settings
    {
        public static bool menu_enable = false;
        public static float width = Screen.width / 2f;
        public static float height = Screen.height / 2f;
        public static float x = 0;
        public static float y = 0;
        public static Color flashlight_color = new Color(1.00f, 1.00f, 1.00f, 1);
        public static Color player_esp_color = new Color(0.00f, 1.00f, 0.00f, 1);
        public static Color azazel_esp_color = new Color(1.00f, 0.00f, 0.00f, 1);
        public static float speed = 1f;
        public const string message_to_spam = "Deez Nutz";
        public static KeyCode flyKey = KeyCode.None;
        public static KeyCode menuKey = KeyCode.Insert;

        // build.bat saves the chosen menu key to UserData\DevourClient_MenuKey.txt; falls back to Insert
        public static void LoadMenuKey()
        {
            try
            {
                string file = System.IO.Path.Combine(MelonLoader.MelonUtils.UserDataDirectory, "DevourClient_MenuKey.txt");
                if (!System.IO.File.Exists(file)) return;
                string name = System.IO.File.ReadAllText(file).Trim();
                if (System.Enum.TryParse(name, true, out KeyCode key) && key != KeyCode.None)
                    menuKey = key;
                else
                    MelonLoader.MelonLogger.Warning($"Unknown menu key '{name}' in {file}, using Insert.");
            }
            catch (System.Exception e)
            {
                MelonLoader.MelonLogger.Warning($"Couldn't read the menu key, using Insert: {e.Message}");
            }
            MelonLoader.MelonLogger.Msg($"Menu key: {menuKey}");
        }
        public static Vector2 itemsScrollPosition = Vector2.zero;
        public static Vector2 rituelObjectsScrollPosition = Vector2.zero;
        public static Vector2 stuffsScrollPosition = Vector2.zero;
        public static Vector2 tpItemsScrollPosition = Vector2.zero;
        public static Vector2 perksScrollPosition = Vector2.zero;
        
        public static KeyCode GetKey()
        {
            Thread.Sleep(50); //TOFIX tried using anyKeydown, no success
            foreach (KeyCode vkey in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(vkey))
                {
                    if (vkey != KeyCode.Delete)
                    {
                        return vkey;
                    }
                }
            }

            return KeyCode.None;
        }
    }
}
