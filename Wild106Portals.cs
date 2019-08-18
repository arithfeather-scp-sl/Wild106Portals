using ArithFeather.ArithSpawningKit.RandomPlayerSpawning;
using ArithFeather.ArithSpawningKit.SpawnPointTools;
using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Config;
using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections.Generic;

namespace ArithFeather.Wild106Portals
{
	[PluginDetails(
		author = "Arith",
		name = "Wild 106 Portals",
		description = "",
		id = "ArithFeather.Wild106Portals",
		configPrefix = "afwp",
		version = "1.0",
		SmodMajor = 3,
		SmodMinor = 4,
		SmodRevision = 0
		)]
	public class Wild106Portals : Plugin, IEventHandler106CreatePortal, IEventHandlerWaitingForPlayers, IEventHandlerSetRole, IEventHandlerLCZDecontaminate
	{
		private readonly Vector portalOffset = new Vector(0, -2f, 0);

		public override void Register()
		{
			AddEventHandlers(this);
		}
		public override void OnEnable() => Info("Wild106Portals Enabled");
		public override void OnDisable() => Info("Wild106Portals Disabled");

		[ConfigOption] private readonly bool disablePlugin = false;

		private List<SpawnPoint> portalData;
		public List<SpawnPoint> PortalData => portalData ?? (portalData = new List<SpawnPoint>());

		private List<PlayerSpawnPoint> portalLoadedSpawns;
		public List<PlayerSpawnPoint> PortalLoadedSpawns => portalLoadedSpawns ?? (portalLoadedSpawns = new List<PlayerSpawnPoint>());

		private List<PlayerSpawnPoint> deconLoadedSpawns;
		public List<PlayerSpawnPoint> DeconLoadedSpawns => deconLoadedSpawns ?? (deconLoadedSpawns = new List<PlayerSpawnPoint>());

		private bool decontaminationStarted;

		public void On106CreatePortal(Player106CreatePortalEvent ev)
		{
			if (decontaminationStarted && DeconLoadedSpawns.Count > 0)
			{
				ev.Position = DeconLoadedSpawns[UnityEngine.Random.Range(0, DeconLoadedSpawns.Count)].Position + portalOffset;
			}
			else if (!decontaminationStarted && PortalLoadedSpawns.Count > 0)
			{
				ev.Position = PortalLoadedSpawns[UnityEngine.Random.Range(0, PortalLoadedSpawns.Count)].Position + portalOffset;
			}
		}

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			if (disablePlugin)
			{
				PluginManager.DisablePlugin(this);
			}

			portalData = SpawnDataIO.Open("sm_plugins/PortalSpawnLocations.txt");

			PortalLoadedSpawns.Clear();
			decontaminationStarted = false;

			var playerPointCount = PortalData.Count;
			var rooms = CustomRoomManager.Instance.Rooms;
			var roomCount = rooms.Count;

			// Create player spawn points on map
			for (var i = 0; i < roomCount; i++)
			{
				var r = rooms[i];

				for (var j = 0; j < playerPointCount; j++)
				{
					var p = PortalData[j];

					if (p.RoomType == r.Name && p.ZoneType == r.Zone)
					{
						PortalLoadedSpawns.Add(new PlayerSpawnPoint(p.RoomType, p.ZoneType,
							Tools.Vec3ToVec(r.Transform.TransformPoint(Tools.VecToVec3(p.Position))) + new Vector(0, 0.3f, 0),
							Tools.Vec3ToVec(r.Transform.TransformDirection(Tools.VecToVec3(p.Rotation)))));
					}
				}
			}

			DeconLoadedSpawns.Clear();
			var spawnCount = PortalLoadedSpawns.Count;
			for (int i = 0; i < spawnCount; i++)
			{
				var spawn = PortalLoadedSpawns[i];

				if (spawn.ZoneType != ZoneType.LCZ)
				{
					DeconLoadedSpawns.Add(spawn);
				}
			}
		}

		public void OnSetRole(PlayerSetRoleEvent ev)
		{
			if (ev.Role == Role.SCP_106)
			{
				ev.Player.PersonalBroadcast(10, "SCP106 has been changed on this server. Your portals will now randomly teleport you almost anywhere in the facility.", false);
			}
		}

		public void OnDecontaminate() => decontaminationStarted = true;
	}
}
