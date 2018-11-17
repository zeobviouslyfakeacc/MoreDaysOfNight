using ModSettings;

namespace MoreDaysOfNight {
	internal static class DaysOfNightSettings {

		internal static bool Enabled {
			get => ThreeDaysOfNight.m_ForcedOn;
			set {
				ThreeDaysOfNight.m_ForcedOn = value;
				MainMenu.SetEnabled(value);
				GameManager.GetPlayerManagerComponent().ForceRotateCamera();
			}
		}

		internal static int DayNumber {
			get => ThreeDaysOfNight.GetCurrentDayNumber();
			set {
				ThreeDaysOfNight.SetCurrentDay(value);
				GameManager.GetWeatherTransitionComponent().ChooseNextWeatherSet();
			}
		}

		internal static readonly MainMenuSettings mainMenuSettings = new MainMenuSettings();
		internal static readonly InGameSettings inGameSettings = new InGameSettings();

		public static void OnLoad() {
			mainMenuSettings.AddToModSettings("More Days of Night", MenuType.MainMenuOnly);
			inGameSettings.AddToModSettings("More Days of Night", MenuType.InGameOnly);
		}

		internal class MainMenuSettings : ModSettingsBase {

			[Name("NOTIFICATION_4DONGeneralTitle", Localize = true)]
			[Choice("GAMEPLAY_Off", "GAMEPLAY_On", Localize = true)]
			public bool enabled = false;

			[Name("GAMEPLAY_Day", Localize = true)]
			[Choice("NOTIFICATION_4DONDay1Title", "NOTIFICATION_4DONDay2Title", "NOTIFICATION_4DONDay3Title", "NOTIFICATION_4DONDay4Title", Localize = true)]
			public int day = 0;

			protected override void OnConfirm() {
				if (!IsVisible())
					return;

				Enabled = enabled;
				if (enabled) {
					DayNumber = day + 1;
				}
				inGameSettings.day = day;
			}
		}

		internal class InGameSettings : ModSettingsBase {

			[Name("GAMEPLAY_Day", Localize = true)]
			[Choice("NOTIFICATION_4DONDay1Title", "NOTIFICATION_4DONDay2Title", "NOTIFICATION_4DONDay3Title", "NOTIFICATION_4DONDay4Title", Localize = true)]
			public int day = 0;

			protected override void OnConfirm() {
				if (!IsVisible())
					return;

				DayNumber = day + 1;
				mainMenuSettings.day = day;
			}
		}
	}
}
