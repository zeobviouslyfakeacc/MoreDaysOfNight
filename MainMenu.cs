using Harmony;
using UnityEngine;

namespace MoreDaysOfNight {
	internal static class MainMenu {

		private static bool visible = false;
		private static bool changed = false;

		internal static void SetEnabled(bool enable) {
			if (visible == enable)
				return;
			visible = enable;
			changed = true;

			Panel_MainMenu mainMenu = InterfaceManager.m_Panel_MainMenu;
			if (enable) {
				Add4DoNItem(mainMenu);
			} else {
				Remove4DoNItem(mainMenu);
				ResetTimeAndWeather(mainMenu);
			}
		}

		private static void Add4DoNItem(Panel_MainMenu mainMenu) {
			Panel_MainMenu.MainMenuItem menuItem = new Panel_MainMenu.MainMenuItem();
			menuItem.m_Type = Panel_MainMenu.MainMenuItem.MainMenuItemType.FourDaysOfNight;
			menuItem.m_LabelLocalizationId = "GAMEPLAY_FourDaysOfNight";

			mainMenu.m_MenuItems.Insert(0, menuItem);
		}

		private static void Remove4DoNItem(Panel_MainMenu mainMenu) {
			mainMenu.m_MenuItems.RemoveAll(item => item.m_Type == Panel_MainMenu.MainMenuItem.MainMenuItemType.FourDaysOfNight);
		}

		private static void ResetTimeAndWeather(Panel_MainMenu mainMenu) {
			Traverse.Create(mainMenu).Field("m_StartSettingsApplied").SetValue(false);
		}

		private static void UpdateVisuals(Panel_MainMenu mainMenu) {
			BasicMenu basicMenu = mainMenu.m_BasicMenuRoot.GetComponentInChildren<BasicMenu>();

			Transform tldLogo = mainMenu.m_MainWindow.transform.Find("TLD_wordmark");
			Vector3 targetPos = tldLogo.localPosition;
			float menuHeight = basicMenu.GetItemCount() * basicMenu.m_MenuGrid.cellHeight;
			targetPos.y = menuHeight - 87;
			tldLogo.localPosition = targetPos;

			basicMenu.m_MenuGrid.Reposition();
			Traverse.Create(basicMenu).Field("m_PreviousSelectedButtonIndex").SetValue(-1);
			basicMenu.ManualUpdate();
		}

		[HarmonyPatch(typeof(Panel_MainMenu), "Update")]
		private static class Patch {
			private static void Postfix(Panel_MainMenu __instance) {
				if (!changed)
					return;

				BasicMenu basicMenu = __instance.m_BasicMenuRoot.GetComponentInChildren<BasicMenu>();
				if (!basicMenu.gameObject.activeInHierarchy)
					return;

				UpdateVisuals(__instance);
				changed = false;
			}
		}
	}
}
