using System;
using System.Reflection;
using System.Collections.Generic;
using Harmony;
using UnityEngine;

namespace MoreDaysOfNight {
	internal static class Patches {

		private static bool IsMainMenu() => GameManager.m_ActiveScene == "MainMenu";
		private static bool Is4DoNSave() => GameManager.GetExperienceModeManagerComponent().GetCurrentExperienceModeType() == ExperienceModeType.FourDaysOfNight;
		private static bool IsActive() => ThreeDaysOfNight.m_ForcedOn && (IsMainMenu() || Is4DoNSave());

		[HarmonyPatch(typeof(ThreeDaysOfNight), "IsActive", new Type[0])]
		private static class RestoreIsActive {
			private static bool Prefix(ref bool __result) {
				__result = IsActive();
				return false; // Don't run original method
			}
		}

		[HarmonyPatch(typeof(ThreeDaysOfNight), "Awake")]
		private static class RestoreUpdate {
			private static void Postfix(ThreeDaysOfNight __instance) {
				// Work around not being able to patch the empty ThreeDaysOfNight#Update method as this doesn't work on MacOS
				// Instead, add another MonoBehavior and use that object's Update method
				__instance.gameObject.AddComponent<MacWorkaround>();
			}
		}

		private class MacWorkaround : MonoBehaviour {

			private void Update() {
				if (GameManager.m_IsPaused)
					return;

				int currentDay = ThreeDaysOfNight.GetCurrentDayNumber();
				if (ThreeDaysOfNight.m_CurrentDay != currentDay && !IsMainMenu() && Is4DoNSave()) {
					ThreeDaysOfNight.m_CurrentDay = currentDay;
					MainMissionManager mainMissionManager = GameManager.GetSandboxManager().m_MainMissionManager;
					mainMissionManager.SendMissionEvent("4DON_DayChange");
					string key = "NOTIFICATION_4DONDay" + ThreeDaysOfNight.m_CurrentDay + "Title";
					FullScreenMessage.AddMessage(Localization.Get(key), 10f, false, true);
					GameManager.GetWeatherTransitionComponent().ChooseNextWeatherSet(null, false);
					if (ThreeDaysOfNight.m_CurrentDay == 4) {
						GameManager.GetWeatherTransitionComponent().OverrideDurationOfStageInCurrentWeatherSet(WeatherStage.Blizzard, 999999f);
					}
				}
			}
		}

		[HarmonyPatch(typeof(Panel_OptionsMenu), "Enable", new Type[] { typeof(bool) })]
		private static class OnlyShowDaysSettingIn4DoNGames {
			private static void Postfix(bool enable) {
				if (enable) {
					DaysOfNightSettings.inGameSettings.SetVisible(IsActive());
				}
			}
		}

		[HarmonyPatch(typeof(Panel_MainMenu), "Awake", new Type[0])]
		private static class Prevent4DoNSaveDeletion {
			private static void Postfix(Panel_MainMenu __instance) {
				Traverse.Create(__instance).Field("m_DoneFourDaysOfNightDeleteCheck").SetValue(true);
			}
		}

		// All of the places ThreeDaysOfNight.IsActive got inlined at... :/

		private static readonly List<MethodInfo> methodsToPatch = new List<MethodInfo>() {
			AccessTools.Method(typeof(BaseAi), "Awake", new Type[0]),
			AccessTools.Method(typeof(CabinFever), "Update", new Type[0]),
			AccessTools.Method(typeof(Carrion), "ShouldFlockDisperse", new Type[0]),
			AccessTools.Method(typeof(DateGate), "PassesDateGate", new Type[0]),
			AccessTools.Method(typeof(DetailedStatsView), "Start", new Type[0]),
			AccessTools.Method(typeof(EyeGlow), "TurnOffEyeGlow", new Type[0]),
			AccessTools.Method(typeof(FlareItem), "GetModifiedBurnLifetimeMinutes", new Type[0]),
			AccessTools.Method(typeof(FlashlightItem), "Awake", new Type[0]),
			AccessTools.Method(typeof(FlyOver), "CanSpawnFlyover", new Type[0]),
			AccessTools.Method(typeof(FoodItem), "UpdateHeatPercent", new Type[] { typeof(float) }),
			AccessTools.Method(typeof(Freezing), "UpdateFreezingStatus", new Type[0]),
			AccessTools.Method(typeof(FuelSourceItem), "GetModifiedBurnDurationHours", new Type[] { typeof(float) }),
			AccessTools.Method(typeof(GearItem), "ApplyBuffs", new Type[] { typeof(float) }),
			AccessTools.Method(typeof(GearItem), "Awake", new Type[0]),
			AccessTools.Method(typeof(IceFishingHole), "SetNextCatchTime", new Type[0]),
			AccessTools.Method(typeof(InputManager), "ProcessTimeGatedUnlocks", new Type[0]),
			AccessTools.Method(typeof(IntestinalParasites), "GetNumDosesRequired", new Type[0]),
			AccessTools.Method(typeof(KeroseneLampItem), "GetModifiedFuelBurnLitersPerHour", new Type[0]),
			AccessTools.Method(typeof(MatchesItem), "Ignite", new Type[0]),
			AccessTools.Method(typeof(Panel_MainMenu), "Update", new Type[0]),
			AccessTools.Method(typeof(Panel_MainMenu), "UpdateMainWindow", new Type[0]),
			AccessTools.Method(typeof(PlayerManager), "EnterInspectGearMode", new Type[] { typeof(GearItem), typeof(Container), typeof(IceFishingHole), typeof(Harvestable), typeof(CookingPotItem) }),
			AccessTools.Method(typeof(PlayerManager), "IsPumpkinPieBuffActive", new Type[0]),
			AccessTools.Method(typeof(PumpkinPieSpecialItem), "Update", new Type[0]),
			AccessTools.Method(typeof(SpawnRegion), "InstantiateSpawnInternal", new Type[] { typeof(WildlifeMode) }),
			AccessTools.Method(typeof(SpawnRegion), "Start", new Type[0]),
			AccessTools.Method(typeof(SpawnRegionManager), "Update", new Type[0]),
			AccessTools.Method(typeof(TimeOfDay), "Deserialize", new Type[] { typeof(string) }),
			AccessTools.Method(typeof(TimeOfDay), "GetHoursDaylightString", new Type[0]),
			AccessTools.Method(typeof(TimeOfDay), "Serialize", new Type[0]),
			AccessTools.Method(typeof(TimeWidget), "Update", new Type[0]),
			AccessTools.Method(typeof(TorchItem), "GetModifiedBurnLifetimeMinutes", new Type[0]),
			AccessTools.Method(typeof(UniStormWeatherSystem), "Update", new Type[0]),
			AccessTools.Method(typeof(Weather), "CalculateCurrentTemperature", new Type[0]),
			AccessTools.Method(typeof(Weather), "IsTooDarkForAction", new Type[] { typeof(ActionsToBlock) }),
			AccessTools.Method(typeof(WeatherTransition), "ChooseNextWeatherSet", new Type[] { typeof(int[]), typeof(bool) }),
			AccessTools.Method(typeof(WeatherTransition), "Deserialize", new Type[] { typeof(string) })
		};

		public static void OnLoad() {
			// Apply transpiler to work around inlining of ThreeDaysOfNight.IsActive
			HarmonyInstance harmony = HarmonyInstance.Create("MoreDaysOfNight-Manual");
			HarmonyMethod transpiler = new HarmonyMethod(typeof(Patches), "Transpiler");
			HarmonyMethod dummy = new HarmonyMethod();

			foreach (MethodInfo method in methodsToPatch) {
				harmony.Patch(method, null, null, transpiler);
			}

			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			Debug.Log("[MoreDaysOfNight] Version " + version + " loaded!");
		}

		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			MethodInfo from = AccessTools.Method(typeof(ThreeDaysOfNight), "IsActive");
			MethodInfo to = AccessTools.Method(typeof(Patches), "IsActive");
			return Transpilers.MethodReplacer(instructions, from, to);
		}
	}
}
