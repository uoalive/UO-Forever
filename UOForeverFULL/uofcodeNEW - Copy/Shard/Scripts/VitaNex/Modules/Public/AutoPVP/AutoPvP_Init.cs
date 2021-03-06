﻿#region Header
//   Vorspire    _,-'/-'/  AutoPvP_Init.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Mobiles;

using VitaNex.IO;
using VitaNex.Schedules;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	[CoreModule("Auto PvP", "1.1.1", false, TaskPriority.High)]
	public static partial class AutoPvP
	{
		static AutoPvP()
		{
			CMOptions = new AutoPvPOptions();

			Scenarios = new PvPScenario[0];
			BattleTypes = typeof(PvPBattle).GetConstructableChildren();

			SeasonSchedule = new Schedule(CMOptions.ModuleName + " Seasons", false, DefaultSeasonSchedule);

			Seasons = new BinaryDataStore<int, PvPSeason>(VitaNexCore.SavesDirectory + "/AutoPvP", "Seasons") {
				OnSerialize = SerializeSeasons,
				OnDeserialize = DeserializeSeasons
			};
			
			Battles = new BinaryDirectoryDataStore<PvPSerial, PvPBattle>(
				VitaNexCore.SavesDirectory + "/AutoPvP", "Battles", "pvp")
			{
				OnSerialize = SerializeBattle,
				OnDeserialize = DeserializeBattle
			};
			
			Profiles = new BinaryDataStore<PlayerMobile, PvPProfile>(VitaNexCore.SavesDirectory + "/AutoPvP", "Profiles") {
				Async = true,
				OnSerialize = SerializeProfiles,
				OnDeserialize = DeserializeProfiles
			};
		}

		private static void CMConfig()
		{
			SeasonSchedule.OnGlobalTick += ChangeSeason;
		}

		private static void CMEnabled()
		{
			SeasonSchedule.OnGlobalTick += ChangeSeason;
			BattleNotoriety.Enable();
		}

		private static void CMDisabled()
		{
			InternalizeAllBattles();
			SeasonSchedule.OnGlobalTick -= ChangeSeason;
			BattleNotoriety.Disable();
		}

		private static void CMInvoke()
		{
			BattleNotoriety.Enable();

			var scenarios = new List<PvPScenario>();

			foreach (Type type in BattleTypes.Where(t => t != null))
			{
				VitaNexCore.TryCatch(
					() =>
					{
						PvPBattle battle = type.CreateInstanceSafe<PvPBattle>();

						if (battle == null)
						{
							throw new Exception("PvPBattle Type could not be constructed, requires a constructor with 0 arguments.");
						}

						PvPScenario scenario = battle;
						scenarios.Add(scenario);
						battle.Delete();

						CMOptions.ToConsole("Created scenario ({0}) '{1}'", scenario.TypeOf.Name, scenario.Name);
					},
					CMOptions.ToConsole);
			}

			Scenarios = scenarios.ToArray();
			scenarios.Clear();

			foreach (var battle in Battles.Values.Where(b => b != null && !b.Deleted).ToArray())
			{
				VitaNexCore.TryCatch(
					battle.Init,
					ex =>
					{
						VitaNexCore.TryCatch(battle.Delete);

						CMOptions.ToConsole("Failed to initialize battle #{0} '{1}'", battle.Serial, battle.Name);
						CMOptions.ToConsole(ex);
					});
			}

			foreach (var profile in Profiles.Values.Where(p => p != null && !p.Deleted).ToArray())
			{
				VitaNexCore.TryCatch(
					profile.Init,
					ex =>
					{
						VitaNexCore.TryCatch(profile.Delete);

						CMOptions.ToConsole("Failed to initialize profile #{0} '{1}'", profile.Owner.Serial.Value, profile.Owner.RawName);
						CMOptions.ToConsole(ex);
					});
			}
		}

		private static void CMSave()
		{
			Save();
			Sync();
		}

		private static void CMLoad()
		{
			Load();
			Sync();
		}

		public static void Save()
		{
			VitaNexCore.TryCatch(SaveSeasons, CMOptions.ToConsole);
			VitaNexCore.TryCatch(SaveBattles, CMOptions.ToConsole);
			VitaNexCore.TryCatch(SaveProfiles, CMOptions.ToConsole);
		}

		public static void SaveSeasons()
		{
			DataStoreResult result = Seasons.Export();
			CMOptions.ToConsole("Result: {0}", result.ToString());

			switch (result)
			{
				case DataStoreResult.Null:
				case DataStoreResult.Busy:
				case DataStoreResult.Error:
					{
						if (Seasons.HasErrors)
						{
							CMOptions.ToConsole("Seasons database has errors...");

							Seasons.Errors.ForEach(CMOptions.ToConsole);
						}
					}
					break;
				case DataStoreResult.OK:
					CMOptions.ToConsole("Season count: {0:#,0}", Seasons.Count);
					break;
			}
		}

		public static void SaveProfiles()
		{
			DataStoreResult result = Profiles.Export();
			CMOptions.ToConsole("Result: {0}", result.ToString());

			switch (result)
			{
				case DataStoreResult.Null:
				case DataStoreResult.Busy:
				case DataStoreResult.Error:
					{
						if (Profiles.HasErrors)
						{
							CMOptions.ToConsole("Profiles database has errors...");

							Profiles.Errors.ForEach(CMOptions.ToConsole);
						}
					}
					break;
				case DataStoreResult.OK:
					CMOptions.ToConsole("Profile count: {0:#,0}", Profiles.Count);
					break;
			}
		}

		public static void SaveBattles()
		{
			DataStoreResult result = Battles.Export();
			CMOptions.ToConsole("Result: {0}", result.ToString());

			switch (result)
			{
				case DataStoreResult.Null:
				case DataStoreResult.Busy:
				case DataStoreResult.Error:
					{
						if (Battles.HasErrors)
						{
							CMOptions.ToConsole("Battles database has errors...");

							Battles.Errors.ForEach(CMOptions.ToConsole);
						}
					}
					break;
				case DataStoreResult.OK:
					CMOptions.ToConsole("Battle count: {0:#,0}", Battles.Count);
					break;
			}
		}

		public static void Load()
		{
			VitaNexCore.TryCatch(LoadSeasons, CMOptions.ToConsole);
			VitaNexCore.TryCatch(LoadBattles, CMOptions.ToConsole);
			VitaNexCore.TryCatch(LoadProfiles, CMOptions.ToConsole);
		}

		public static void LoadSeasons()
		{
			DataStoreResult result = Seasons.Import();
			CMOptions.ToConsole("Result: {0}", result.ToString());

			switch (result)
			{
				case DataStoreResult.Null:
				case DataStoreResult.Busy:
				case DataStoreResult.Error:
					{
						if (Seasons.HasErrors)
						{
							CMOptions.ToConsole("Seasons database has errors...");

							Seasons.Errors.ForEach(CMOptions.ToConsole);
						}
					}
					break;
				case DataStoreResult.OK:
					CMOptions.ToConsole("Season count: {0:#,0}", Seasons.Count);
					break;
			}
		}

		public static void LoadProfiles()
		{
			DataStoreResult result = Profiles.Import();
			CMOptions.ToConsole("Result: {0}", result.ToString());

			switch (result)
			{
				case DataStoreResult.Null:
				case DataStoreResult.Busy:
				case DataStoreResult.Error:
					{
						if (Profiles.HasErrors)
						{
							CMOptions.ToConsole("Profiles database has errors...");

							Profiles.Errors.ForEach(CMOptions.ToConsole);
						}
					}
					break;
				case DataStoreResult.OK:
					CMOptions.ToConsole("Profile count: {0:#,0}", Profiles.Count);
					break;
			}
		}

		public static void LoadBattles()
		{
			DataStoreResult result = Battles.Import();
			CMOptions.ToConsole("Result: {0}", result.ToString());

			switch (result)
			{
				case DataStoreResult.Null:
				case DataStoreResult.Busy:
				case DataStoreResult.Error:
					{
						if (Battles.HasErrors)
						{
							CMOptions.ToConsole("Battles database has errors...");

							Battles.Errors.ForEach(CMOptions.ToConsole);
						}
					}
					break;
				case DataStoreResult.OK:
					CMOptions.ToConsole("Battle count: {0:#,0}", Battles.Count);
					break;
			}
		}

		public static void Sync()
		{
			VitaNexCore.TryCatch(SyncSeasons, CMOptions.ToConsole);
			VitaNexCore.TryCatch(SyncBattles, CMOptions.ToConsole);
			VitaNexCore.TryCatch(SyncProfiles, CMOptions.ToConsole);
		}

		public static void SyncSeasons()
		{
			foreach (var season in Seasons.Values)
			{
				VitaNexCore.TryCatch(
					season.Sync,
					ex =>
					{
						CMOptions.ToConsole("Failed to sync season #{0}", season.Number);
						CMOptions.ToConsole(ex);
					});
			}
		}

		public static void SyncProfiles()
		{
			foreach (var profile in Profiles.Values)
			{
				VitaNexCore.TryCatch(
					profile.Sync,
					ex =>
					{
						CMOptions.ToConsole("Failed to sync profile #{0} '{1}'", profile.Owner.Serial.Value, profile.Owner.RawName);
						CMOptions.ToConsole(ex);
					});
			}
		}

		public static void SyncBattles()
		{
			foreach (var battle in Battles.Values)
			{
				VitaNexCore.TryCatch(
					battle.Sync,
					ex =>
					{
						CMOptions.ToConsole("Failed to sync battle #{0} '{1}'", battle.Serial, battle.Name);
						CMOptions.ToConsole(ex);
					});
			}
		}

		private static bool SerializeSeasons(GenericWriter writer)
		{
			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.WriteBlockDictionary(
							Seasons,
							(key, val) =>
							{
								writer.Write(key);
								writer.WriteType(
									val,
									t =>
									{
										if (t != null)
										{
											val.Serialize(writer);
										}
									});
							});
					}
					break;
			}

			return true;
		}

		private static bool DeserializeSeasons(GenericReader reader)
		{
			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					{
						reader.ReadBlockDictionary(
							() =>
							{
								int key = reader.ReadInt();
								PvPSeason val = reader.ReadTypeCreate<PvPSeason>(reader) ?? new PvPSeason(key);
								return new KeyValuePair<int, PvPSeason>(key, val);
							},
							Seasons);
					}
					break;
			}

			return true;
		}

		private static bool SerializeProfiles(GenericWriter writer)
		{
			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.WriteBlockDictionary(
							Profiles,
							(key, val) =>
							{
								writer.Write(key);
								writer.WriteType(
									val,
									t =>
									{
										if (t != null)
										{
											val.Serialize(writer);
										}
									});
							});
					}
					break;
			}

			return true;
		}

		private static bool DeserializeProfiles(GenericReader reader)
		{
			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					{
						reader.ReadBlockDictionary(
							() =>
							{
								PlayerMobile key = reader.ReadMobile<PlayerMobile>();
								PvPProfile val = reader.ReadTypeCreate<PvPProfile>(reader);
								return new KeyValuePair<PlayerMobile, PvPProfile>(key, val);
							},
							Profiles);
					}
					break;
			}

			return true;
		}

		private static bool SerializeBattle(GenericWriter writer, PvPSerial key, PvPBattle val)
		{
			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.WriteBlock(
							() => writer.WriteType(
								key,
								t =>
								{
									if (t != null)
									{
										key.Serialize(writer);
									}
								}));

						writer.WriteBlock(
							() => writer.WriteType(
								val,
								t =>
								{
									if (t != null)
									{
										val.Serialize(writer);
									}
								}));
					}
					break;
			}

			return true;
		}

		private static Tuple<PvPSerial, PvPBattle> DeserializeBattle(GenericReader reader)
		{
			PvPSerial key = null;
			PvPBattle val = null;

			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					{
						reader.ReadBlock(() => key = reader.ReadTypeCreate<PvPSerial>(reader));
						reader.ReadBlock(() => val = reader.ReadTypeCreate<PvPBattle>(reader));
					}
					break;
			}

			if (key == null)
			{
				if (val != null && val.Serial != null)
				{
					key = val.Serial;
				}
				else
				{
					return null;
				}
			}

			return new Tuple<PvPSerial, PvPBattle>(key, val);
		}
	}
}