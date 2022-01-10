//> -target:library
//> import: Assembly-CSharp
//> import: UnityEngine.CoreModule
using System;
using UnityEngine;

namespace Rosuav {
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class ArmstrongNavPlugin : MonoBehaviour {
		private float last_update = 0.0f;
		void FixedUpdate()
		{
			if (Time.time - last_update < 1.0f) return;
			last_update = Time.time;
			Vessel self = FlightGlobals.ActiveVessel;
			if (!self) return;
			PatchedConicSolver solver = self.patchedConicSolver;
			if (solver) check_maneuver_nodes(solver);
			FinePrint.Waypoint waypoint = self.navigationWaypoint;
			CelestialBody surface = self.mainBody;
			if (waypoint == null || surface == null) return;
			double srfvel = self.GetSrfVelocity().magnitude;
			//Calculate great circle distance from lat/long and radius
			double lat1 = self.latitude * Math.PI / 180, lat2 = waypoint.latitude * Math.PI / 180;
			double longs = (self.longitude - waypoint.longitude) * Math.PI / 180;
			double angle = Math.Acos(Math.Sin(lat1) * Math.Sin(lat2) +
				Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(longs));
			double circle_dist = angle * surface.Radius;
			//TODO: Add the estimated time to the current mission time and report estimated arrival time
			print(String.Format("[ArmstrongNav] {0:0.00} deg or {1:0} m at {2:0.00} m/s = {3:0.00} sec - {4}",
				angle * 180 / Math.PI, circle_dist, srfvel, circle_dist / srfvel, waypoint.name));
		}
		void check_maneuver_nodes(PatchedConicSolver solver) {
			if (solver.maneuverNodes.Count == 0) return;
			print(String.Format("[ArmstrongNav] Solver has {0} maneuver nodes", solver.maneuverNodes.Count));
			ManeuverNode node = solver.maneuverNodes[0];
			Vector3d pos = node.patch.getPositionAtUT(node.UT); //NOTE: The coordinates keep changing, not sure why.
			print(String.Format("[ArmstrongNav] First node is at {0},{1},{2}", pos.x, pos.y, pos.z));
			double altitude = FlightGlobals.getAltitudeAtPos(pos); //NOTE: This isn't perfectly stable, but it is generally within a fraction of a meter.
			print(String.Format("[ArmstrongNav] Altitude {0}", altitude));
		}
	}
}
