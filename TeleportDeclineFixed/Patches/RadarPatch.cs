using HarmonyLib;
using System.IO;
using TeleportDecline;
using UnityEngine;

namespace TeleportDeclineFixed.Patches
{
    internal class RadarPatch
    {
        [HarmonyPatch(typeof(StartOfRound), "Start")]
        [HarmonyPostfix]
        static void Postfix()
        {
            ReplaceRadarIconTexture();
        }

        public static void ReplaceRadarIconTexture()
        {
            var start = StartOfRound.Instance;
            if (start == null)
            {
                TeleportDeclineBase.instance.mls.LogError("StartOfRound.instance is null!");
                return;
            }

            GameObject prefab = start.itemRadarIconPrefab;
            if (prefab == null)
            {
                TeleportDeclineBase.instance.mls.LogError("itemRadarIconPrefab is null!");
                return;
            }

            Transform squareChild = prefab.transform.Find("Square");
            if (squareChild == null)
            {
                TeleportDeclineBase.instance.mls.LogError("Square child not found on prefab!");
                return;
            }

            SpriteRenderer sr = squareChild.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                TeleportDeclineBase.instance.mls.LogError("No SpriteRenderer on Square!");
                return;
            }

            string pluginDir = Path.GetDirectoryName(typeof(TeleportDeclineBase).Assembly.Location);
            string filePath = Path.Combine(pluginDir, "ScrapItemMap.png");

            if (!File.Exists(filePath))
            {
                TeleportDeclineBase.instance.mls.LogError($"Texture file not found: {filePath}");
                return;
            }

            byte[] pngBytes = File.ReadAllBytes(filePath);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(tex, pngBytes))
            {
                TeleportDeclineBase.instance.mls.LogError("Failed to load PNG into Texture2D!");
                return;
            }

            // Create new sprite from texture
            Sprite newSprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                sr.sprite.pixelsPerUnit * 0.9f
            );

            sr.sprite = newSprite;
            TeleportDeclineBase.instance.mls.LogInfo("Radar icon texture replaced successfully!");
        }
    }
}
