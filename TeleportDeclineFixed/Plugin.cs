using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalCompanyInputUtils.Api;
using StaticNetcodeLib;
using Unity.Netcode;
using UnityEngine.InputSystem;

namespace TeleportDecline
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(StaticNetcodeLib.StaticNetcodeLib.Guid, BepInDependency.DependencyFlags.HardDependency)]
    public class TeleportDeclineBase : BaseUnityPlugin
    {
        public const string GUID = "MasterAli2.TeleportDeclineFixed";
        public const string NAME = "Teleport Decline Fixed";
        public const string VERSION = "1.1.0";
        public const string AUTHOR = "MasterAli2";

        private readonly Harmony harmony = new Harmony(GUID);
        internal ManualLogSource mls;

        public static TeleportDeclineBase instance;

        public bool isTeleporting = false;
        public bool declining = false;
        public ShipTeleporter teleporter;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            mls = this.Logger;

            TeleportDeclineInput.instance.DeclineKey.performed += DeclineTeleport;
            ApplyPatches();

            mls.LogInfo($"{GUID} v{VERSION} has loaded!");
        }

        void ApplyPatches()
        {
            mls.LogInfo("Patching...");
            harmony.PatchAll(typeof(Patches.Patch));
            mls.LogInfo("Patched...");
        }

        public void DeclineTeleport(InputAction.CallbackContext context)
        {
            if (!context.performed || !isTeleporting) return;

            StartOfRound.Instance.localPlayerController.beamUpParticle.Stop();
            HUDManager.Instance.tipsPanelBody.text = "Declining teleport...";

            declining = true;

            TeleportDeclineNetcode.DeclineTeleportServerRpc();
            mls.LogInfo("Declining teleport...");
        }
    }

    public class TeleportDeclineInput : LcInputActions
    {
        public static TeleportDeclineInput instance = new();

        [InputAction("<Keyboard>/c", Name = "Decline Teleport")]
        public InputAction DeclineKey { get; set; }
    }

    [StaticNetcode]
    public static class TeleportDeclineNetcode
    {
        [ClientRpc]
        public static void DeclineTeleportClientRpc()
        {
            var plugin = TeleportDeclineBase.instance;

            if (plugin.declining)
            {
                HUDManager.Instance.tipsPanelBody.text = "Teleport declined!";
                plugin.declining = false;
            }
            plugin.teleporter.StopCoroutine(plugin.teleporter.beamUpPlayerCoroutine);
            plugin.isTeleporting = false;
            plugin.mls.LogInfo("Teleport was declined!");

            if (plugin.isTeleporting || !StartOfRound.Instance.localPlayerController.isInHangarShipRoom) return;

            HUDManager.Instance.DisplayTip("Teleport Decline", "That teleport got declined");
        }

        [ServerRpc]
        public static void DeclineTeleportServerRpc() => DeclineTeleportClientRpc();
    }
}
