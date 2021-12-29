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
			if ((Time.time - last_update) > 1.0f)
			{
				last_update = Time.time;
				Vessel self = FlightGlobals.ActiveVessel;
				if (!self) return;
				FinePrint.Waypoint waypoint = self.navigationWaypoint;
				CelestialBody surface = self.mainBody;
				if (waypoint == null || surface == null) return;
				double srfvel = self.GetSrfVelocity().magnitude;
				//Technique 1: Get surface-relative positions and do vector arithmetic
				Vector3d here = surface.GetRelSurfacePosition(self.latitude, self.longitude, 0.0);
				Vector3d there = surface.GetRelSurfacePosition(waypoint.latitude, waypoint.longitude, 0.0);
				double dist = Vector3d.Distance(here, there);
				print("ArmstrongNav here: " + here);
				print("ArmstrongNav there: " + there);
				print("ArmstrongNav diff: " + (there - here));
				print("ArmstrongNav time to arrival: " + dist + " at " + srfvel + " = " + (dist / srfvel));
				//Technique 2: Calculate great circle distance from lat/long and radius
				double lat1 = self.latitude * Math.PI / 180, lat2 = waypoint.latitude * Math.PI / 180;
				double longs = (self.longitude - waypoint.longitude) * Math.PI / 180;
				double angle = Math.Acos(Math.Sin(lat1) * Math.Sin(lat2) +
					Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(longs));
				double circle_dist = angle * surface.Radius;
				print("ArmstrongNav self: " + lat1 + " - " + lat2 + " - " + longs);
				print("ArmstrongNav angle: " + (angle * 180 / Math.PI));
				print("ArmstrongNav great circle: " + circle_dist + " at " + srfvel + " = " + (circle_dist / srfvel));
			}
		}
	}
}
