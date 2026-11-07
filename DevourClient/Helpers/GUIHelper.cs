using UnityEngine;
using System.Collections.Generic;

namespace DevourClient.Helpers
{
    class GUIHelper
    {
        private static float R;
        private static float G;
        private static float B;

        private static Texture2D previewTexture = null;
        private static Dictionary<Color, Texture2D> colorTextureCache = new Dictionary<Color, Texture2D>();
        private static Dictionary<int, Texture2D> circularTextureCache = new Dictionary<int, Texture2D>();

        private static Color lastPreviewColor = Color.clear;
        private static GUIStyle cachedBoxStyle = null;

        public static Color ColorPick(string title, Color color)
        {
            GUI.Label(new Rect(Settings.Settings.x + 195, Settings.Settings.y + 70, 250, 30), title);

            R = GUI.VerticalSlider(new Rect(Settings.Settings.x + 240, Settings.Settings.y + 100, 30, 90), color.r, 0f, 1f);
            G = GUI.VerticalSlider(new Rect(Settings.Settings.x + 270, Settings.Settings.y + 100, 30, 90), color.g, 0f, 1f);
            B = GUI.VerticalSlider(new Rect(Settings.Settings.x + 300, Settings.Settings.y + 100, 30, 90), color.b, 0f, 1f);

            GUI.Label(new Rect(Settings.Settings.x + 240, Settings.Settings.y + 190, 30, 30), "R");
            GUI.Label(new Rect(Settings.Settings.x + 270, Settings.Settings.y + 190, 30, 30), "G");
            GUI.Label(new Rect(Settings.Settings.x + 300, Settings.Settings.y + 190, 30, 30), "B");
            
            color = new Color(R, G, B, 1);

            void DrawPreview(Rect position, Color color_to_draw)
            {
                if (previewTexture == null || lastPreviewColor != color_to_draw)
                {
                    if (previewTexture == null)
                    {
                        previewTexture = new Texture2D(1, 1);
                    }
                    
                    previewTexture.SetPixel(0, 0, color_to_draw);
                    previewTexture.Apply();
                    lastPreviewColor = color_to_draw;
                }

                if (cachedBoxStyle == null)
                {
                    cachedBoxStyle = new GUIStyle(GUI.skin.box);
                }
                cachedBoxStyle.normal.background = previewTexture;
                GUI.Box(position, GUIContent.none, cachedBoxStyle);
            }

            DrawPreview(new Rect(Settings.Settings.x + 195, Settings.Settings.y + 100, 20, 90), color);

            return color;
        }
        
        public static Texture2D MakeTex(int width, int height, Color col)
        {
            if (colorTextureCache.TryGetValue(col, out Texture2D cachedTexture))
            {
                if (cachedTexture != null)
                {
                    return cachedTexture;
                }
            }

            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            colorTextureCache[col] = result;

            return result;
        }
        
        public static Texture2D GetCircularTexture(int width, int height)
        {
            int cacheKey = width;

            if (circularTextureCache.TryGetValue(cacheKey, out Texture2D cachedTexture))
            {
                if (cachedTexture != null)
                {
                    return cachedTexture;
                }
            }

            Texture2D texture = new Texture2D(width, height);
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    if (Vector2.Distance(new Vector2(x, y), new Vector2(texture.width / 2, texture.height / 2)) <= texture.width / 2)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();

            circularTextureCache[cacheKey] = texture;

            return texture;
        }

        public static void Cleanup()
        {
            if (previewTexture != null)
            {
                UnityEngine.Object.Destroy(previewTexture);
                previewTexture = null;
            }

            foreach (var texture in colorTextureCache.Values)
            {
                if (texture != null)
                {
                    UnityEngine.Object.Destroy(texture);
                }
            }
            colorTextureCache.Clear();

            foreach (var texture in circularTextureCache.Values)
            {
                if (texture != null)
                {
                    UnityEngine.Object.Destroy(texture);
                }
            }
            circularTextureCache.Clear();
        }
    }
}
