using FrooxEngine;
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using ResoniteModLoader;
using System;
using System.Reflection;
//using ResoniteHotReloadLib;

namespace NodeAttachmentIssuesHotline
{
	public class NodeAttachmentIssuesHotline : ResoniteMod
	{
		public override string Name => "NodeAttachmentIssuesHotline";
		public override string Author => "Nytra";
		public override string Version => "1.1.2";
		public override string Link => "https://github.com/Nytra/ResoniteNodeAttachmentIssuesHotline";

		public static ModConfiguration Config;

		[AutoRegisterConfigKey]
		static ModConfigurationKey<bool> MOD_ENABLED = new ModConfigurationKey<bool>("MOD_ENABLED", "Mod Enabled:", () => true);

		public override void OnEngineInit()
		{
			Config = GetConfiguration();
			//HotReloader.RegisterForHotReload(this);
			PatchStuff();
		}

		//static void OnHotReload(ResoniteMod mod)
		//{
		//	Config = mod.GetConfiguration();
		//	PatchStuff();
		//}

		//static void BeforeHotReload()
		//{
		//	Harmony harmony = new Harmony("owo.Nytra.NodeAttachmentIssuesHotline");
		//	harmony.UnpatchAll("owo.Nytra.NodeAttachmentIssuesHotline");
		//}

		static void PatchStuff()
		{
			Harmony harmony = new Harmony("owo.Nytra.NodeAttachmentIssuesHotline");
			harmony.PatchAll();
		}

		static bool ElementExists(IWorldElement element)
		{
			return element != null && !element.IsRemoved;
		}

		//[HarmonyPatch(typeof(Userspace), "OnCommonUpdate")]
		//class HotReloadPatch
		//{
		//	static void Postfix()
		//	{
		//		if (!Config.GetValue(MOD_ENABLED)) return;
		//		if (Engine.Current.InputInterface.GetKeyDown(Key.F3))
		//		{
		//			HotReloader.HotReload(typeof(NodeAttachmentIssuesHotline));
		//		}
		//	}
		//}

		[HarmonyPatch(typeof(ProtoFluxInputProxy))]
		[HarmonyPatch("Disconnect")]
		class Patch_ProtoFluxInputProxy_Disconnect
		{
			static PropertyInfo currentTargetProperty = AccessTools.Property(typeof(ProtoFluxInputProxy), "CurrentTarget");
			static bool Prefix(ProtoFluxInputProxy __instance)
			{
				if (!Config.GetValue(MOD_ENABLED)) return true;
				if (__instance == null) return true;
				Debug("Disconnect ProtoFluxInputProxy");
				var currentTarget = (INodeOutput)currentTargetProperty?.GetValue(__instance);
				if (ElementExists(currentTarget))
				{
					var node = currentTarget.FindNearestParent<ProtoFluxNode>();
					if (ElementExists(node) && node.Group != null)
					{
						try
						{
							Debug("Scheduling group rebuild for group: " + node.Group.Name ?? "NULL");
							__instance.World.ProtoFlux.ScheduleGroupRebuild(node.Group);
						}
						catch (Exception e)
						{
							Error("Exception while scheduling group rebuild:\n" + e.ToString());
						}
					}
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(ProtoFluxImpulseProxy))]
		[HarmonyPatch("Disconnect")]
		class Patch_ProtoFluxImpulseProxy_Disconnect
		{
			static PropertyInfo currentTargetProperty = AccessTools.Property(typeof(ProtoFluxImpulseProxy), "CurrentTarget");
			static bool Prefix(ProtoFluxImpulseProxy __instance)
			{
				if (!Config.GetValue(MOD_ENABLED)) return true;
				if (__instance == null) return true;
				Debug("Disconnect ProtoFluxImpulseProxy");
				var currentTarget = (INodeOperation)currentTargetProperty?.GetValue(__instance);
				if (ElementExists(currentTarget))
				{
					var node = currentTarget.FindNearestParent<ProtoFluxNode>();
					if (ElementExists(node) && node.Group != null)
					{
						try
						{
							Debug("Scheduling group rebuild for group: " + node.Group.Name ?? "NULL");
							__instance.World.ProtoFlux.ScheduleGroupRebuild(node.Group);
						}
						catch (Exception e)
						{
							Error("Exception while scheduling group rebuild:\n" + e.ToString());
						}
					}
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(ProtoFluxNode))]
		[HarmonyPatch("OnDestroying")]
		class Patch_ProtoFluxNode_OnDestroying
		{
			static bool Prefix(ProtoFluxNode __instance)
			{
				if (!Config.GetValue(MOD_ENABLED)) return true;
				if (!ElementExists(__instance)) return true;
				if (__instance.Group?.Nodes == null) return true;
				Debug($"OnDestroying ProtoFluxNode: {__instance.Name ?? "NULL"} {__instance.ReferenceID.ToString() ?? "NULL"} Group: {__instance.Group.Name ?? "NULL"}");
				try
				{
					//Debug("Scheduling group rebuild for group: " + node.Group.Name ?? "NULL");
					__instance.World.ProtoFlux.ScheduleGroupRebuild(__instance.Group);
				}
				catch (Exception e)
				{
					Error("Exception while scheduling group rebuild:\n" + e.ToString());
				}
				return true;
			}
		}
	}
}