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
			/*
			FinePrint.WaypointManager mgr = FinePrint.WaypointManager.Instance();
			print(String.Format("[ArmstrongNav] Manager dist {0:0.00} lateral {1:0.00}",
				mgr.DistanceToVessel(waypoint), mgr.LateralDistanceToVessel(waypoint)));
			double now = Planetarium.GetUniversalTime(), nextorbit = now + self.orbit.period;
			1) Take the five points 'now', 'nextorbit', and 1/4, 2/4, 3/4 interpolations
			2) For each point, calculate its lateral distance to the waypoint
			3) For each pair (now, 1/4), (1/4, 2/4), etc, calculate the sum of distances
			4) Pick the lowest sum, and assign (t1, t2) = the two points (a quarter-orbit apart).
			5) Iterate until t1 and t2 are sufficiently close together:
			5a) Find point t3 = (t1+t2)/2
			5b) Calculate lateral distance from t3 to the waypoint
			5c) If it is higher than both t1 and t2, break, keep the lower distance of t1/t2, end
			5d) Otherwise, if t1 distance < t2 distance, take t1,t2 = t1,t3, else take t1,t2 = t3,t2
			6) Report the timestamp of closest lateral approach, and the altitude at which the vessel will be.
			To find distance at a given UT:
			1) Vector3d pos = self.orbit.getPositionAtUT(t)
			2) Vector2d latlon = surface.GetLatitudeAndLongitude(pos);
			3) Either use mgr or the great circle calculation as above
			*/
			/* Writing to screen instead of to a log file:
			https://www.kerbalspaceprogram.com/api/class_screen_message.html
			*/
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
