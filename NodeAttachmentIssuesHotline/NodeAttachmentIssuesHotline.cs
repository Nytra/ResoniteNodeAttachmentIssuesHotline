using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes;
using HarmonyLib;
using ResoniteModLoader;
using System;
using System.Linq;
using System.Reflection;

namespace NodeAttachmentIssuesHotline
{
	public class NodeAttachmentIssuesHotline : ResoniteMod
	{
		public override string Name => "NodeAttachmentIssuesHotline";
		public override string Author => "Nytra";
		public override string Version => "2.0.0";
		public override string Link => "https://github.com/Nytra/ResoniteNodeAttachmentIssuesHotline";

		public static ModConfiguration Config;

		[AutoRegisterConfigKey]
		static ModConfigurationKey<bool> KEY_MOD_ENABLED = new ModConfigurationKey<bool>("Mod Enabled", "Mod Enabled", () => true);
		[AutoRegisterConfigKey]
		static ModConfigurationKey<bool> KEY_NON_LOCAL = new ModConfigurationKey<bool>("Use non-local fix (Experimental)", "Use non-local fix (Experimental)", () => false);

		const string LinkNodeIndentifier = "NodeAttachmentIssuesHotline_Link";

		public override void OnEngineInit()
		{
			Config = GetConfiguration();
			PatchStuff();
		}

		static void PatchStuff()
		{
			Harmony harmony = new Harmony("owo.Nytra.NodeAttachmentIssuesHotline");
			harmony.PatchAll();
		}

		static bool ElementExists(IWorldElement element)
		{
			return element != null && !element.IsRemoved;
		}

		static void RebuildGroupWithLinkNode(ProtoFluxNode node)
		{
			// Create Link node, attach node, wait, destroy link node
			var linkNodeSlot = node.LocalUserRoot?.Slot?.AddSlot(LinkNodeIndentifier);
			if (linkNodeSlot == null)
			{
				Debug("Link node slot was null after add slot!");
				return;
			}
			var linkNode = linkNodeSlot.AttachComponent<Link>();
			if (linkNode == null)
			{
				Debug("Link was null after attach!");
				linkNodeSlot.Destroy();
				return;
			}
			Debug($"Link attached: {linkNode.Name} {linkNode.ReferenceID}");
			linkNode.A.Target = (FrooxEngine.ProtoFlux.Core.INode)node;
			Debug($"Link node target set to Node: {node.Name} {node.ReferenceID}");
			linkNode.RunInUpdates(3, () =>
			{
				if (ElementExists(linkNode))
				{
					linkNode.A.Target = null;
					Debug("Link node target set to null");
					linkNode.RunInUpdates(3, () =>
					{
						if (ElementExists(linkNode))
						{
							Slot linkNodeSlot = linkNode.Slot;
							linkNode.Destroy(sendDestroyingEvent: false);
							Debug("Link component destroyed");
							if (linkNodeSlot.ComponentCount == 0)
							{
								linkNodeSlot.Destroy();
								Debug("Link slot destroyed");
							}
						}
						else
						{
							Debug("Link node does not exist in second runInUpdates");
						}
					});
				}
				else
				{
					Debug("Link node does not exist in first runInUpdates");
				}
			});
		}

		static string GetNodeString(ProtoFluxNode node)
		{
			return $"ProtoFluxNode: {node.Name} {node.ReferenceID} Group: {node.Group?.Name}";
		}

		[HarmonyPatch(typeof(ProtoFluxInputProxy))]
		[HarmonyPatch("Disconnect")]
		class Patch_ProtoFluxInputProxy_Disconnect
		{
			static PropertyInfo currentTargetProperty = AccessTools.Property(typeof(ProtoFluxInputProxy), "CurrentTarget");
			static bool Prefix(ProtoFluxInputProxy __instance)
			{
				if (!Config.GetValue(KEY_MOD_ENABLED)) return true;
				Debug("Disconnect ProtoFluxInputProxy");
				var currentTarget = (INodeOutput)currentTargetProperty?.GetValue(__instance);
				if (ElementExists(currentTarget))
				{
					var node = currentTarget.FindNearestParent<ProtoFluxNode>();
					if (ElementExists(node) && node.Group != null)
					{
						try
						{
							if (Config.GetValue(KEY_NON_LOCAL))
							{
								Debug($"Running non-local node group rebuild (using Link node) for {GetNodeString(node)}");
								RebuildGroupWithLinkNode(node);
							}
							else
							{
								Debug($"Scheduling (local) node group rebuild for {GetNodeString(node)}");
								node.World.ProtoFlux.ScheduleGroupRebuild(node.Group);
							}
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
				if (!Config.GetValue(KEY_MOD_ENABLED)) return true;
				Debug("Disconnect ProtoFluxImpulseProxy");
				var currentTarget = (INodeOperation)currentTargetProperty?.GetValue(__instance);
				if (ElementExists(currentTarget))
				{
					var node = currentTarget.FindNearestParent<ProtoFluxNode>();
					if (ElementExists(node) && node.Group != null)
					{
						try
						{
							if (Config.GetValue(KEY_NON_LOCAL))
							{
								Debug($"Running non-local node group rebuild (using Link node) for {GetNodeString(node)}");
								RebuildGroupWithLinkNode(node);
							}
							else
							{
								Debug($"Scheduling (local) node group rebuild for {GetNodeString(node)}");
								node.World.ProtoFlux.ScheduleGroupRebuild(node.Group);
							}
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
				if (!Config.GetValue(KEY_MOD_ENABLED)) return true;
				if (Engine.Current?.IsReady != true) return true;
				if (__instance.Group?.Nodes == null) return true;

				ProtoFluxNodeGroup group = __instance.Group;

				// wait some updates just in case the rest of the nodes in the group are also being destroyed
				// not sure how many updates to wait but 1 seems ok, could maybe try 3 if there are issues
				__instance.World.RunInUpdates(1, () =>
				{
					if (group != null && group.NodeCount > 0)
					{
						var node = group.Nodes.FirstOrDefault(node => ElementExists(node));
						if (node != null)
						{
							Debug($"Destroyed: {GetNodeString(node)}");
							if (Config.GetValue(KEY_NON_LOCAL))
							{
								Debug($"Running non-local node group rebuild (using Link node) for {GetNodeString(node)}");
								RebuildGroupWithLinkNode(node);
							}
							else
							{
								Debug($"Scheduling (local) node group rebuild for {GetNodeString(node)}");
								node.World.ProtoFlux.ScheduleGroupRebuild(node.Group);
							}
						}
					}
				});
				return true;
			}
		}
	}
}