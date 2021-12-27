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
				Vector3d here = surface.GetRelSurfacePosition(self.latitude, self.longitude, 0.0);
				Vector3d there = surface.GetRelSurfacePosition(waypoint.latitude, waypoint.longitude, 0.0);
				double dist = Vector3d.Distance(here, there);
				double srfvel = self.GetSrfVelocity().magnitude;
				print("ArmstrongNav here: " + here);
				print("ArmstrongNav there: " + there);
				print("ArmstrongNav diff: " + (there - here));
				print("ArmstrongNav time to arrival: " + dist + " at " + srfvel + " = " + (dist / srfvel));
			}
		}
	}
}
