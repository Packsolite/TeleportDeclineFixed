using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine.InputSystem;

namespace TeleportDecline.Patches
{
    internal class Patch
    {
        [HarmonyPatch(typeof(ShipTeleporter), "PressTeleportButtonClientRpc")]
        [HarmonyPostfix]
        static void PressTeleportButtonClientRpcPostfix(ShipTeleporter __instance)
        {
            if (__instance.isInverseTeleporter || StartOfRound.Instance.localPlayerController.isPlayerDead)
                return;

            TeleportDeclineBase.instance.teleporter = __instance;

            if (StartOfRound.Instance.mapScreen.targetedPlayer == StartOfRound.Instance.localPlayerController)
            {
                HUDManager.Instance.DisplayTip("Teleporting!", "Press " + TeleportDeclineInput.instance.DeclineKey.GetBindingDisplayString().Split("|")[0] + "to stop teleport");
                TeleportDeclineBase.instance.isTeleporting = true;
            }
        }

        [HarmonyPatch(typeof(ShipTeleporter), "SetPlayerTeleporterId")]
        [HarmonyPostfix]
        static void SetPlayerTeleporterIdPostfix(ShipTeleporter __instance, ref int teleporterId)
        {
            if (TeleportDeclineBase.instance.teleporter != __instance) return;

            TeleportDeclineBase.instance.isTeleporting = false;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "TeleportPlayer")]
        [HarmonyPrefix]
        static bool TeleportPlayer(ShipTeleporter __instance, UnityEngine.Vector3 pos, bool withRotation, float rot, bool allowInteractTrigger, bool enableController)
        {
            if (!TeleportDeclineBase.instance.declining) return true;
            TeleportDeclineBase.instance.declining = false;
            TeleportDeclineBase.instance.mls.LogWarning("Teleport decline not acknowledged by host. Desync may occur.");
            return false;
        }
    }
}
