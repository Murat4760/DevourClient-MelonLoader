namespace DevourClient.Helpers
{
    class Map
    {
        private static int sceneFrame = -1;
        private static string sceneName = "";

        // Called from Update and every OnGUI event; the scene name is only fetched (and its string allocated) once per frame.
        public static string GetActiveScene()
        {
            int frame = UnityEngine.Time.frameCount;
            if (frame != sceneFrame)
            {
                sceneFrame = frame;
                sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            }
            return sceneName;
        }

        public static string GetMapName(string sceneName)
        {
            switch (sceneName)
            {
                case "Anna":
                    return "Farmhouse";
                case "Molly":
                    return "Asylum";
                case "Inn":
                    return "Inn";
                case "Town":
                    return "Town";
                case "Slaughterhouse":
                    return "Slaughterhouse";
                case "Manor":
                    return "Manor";
                case "Carnival":
                    return "Carnival";
                default:
                    return "Menu";
            }
        }
        
        public static UnityEngine.GameObject GetAzazel()
        {
            return Helpers.Entities.Azazels[0].gameObject;
        }
        
        public static void LoadMap(string mapName)
        {
            if (Il2CppPhoton.Bolt.BoltNetwork.IsServer)
            {
                Il2CppPhoton.Bolt.BoltNetwork.LoadScene(mapName);
                
                MelonLoader.MelonLogger.Warning("Please press the button only once, it may take some time for the map to load.");
            }
            else
            {
                MelonLoader.MelonLogger.Warning("You must be the host to use this command!");
            }
        }
    }
}
