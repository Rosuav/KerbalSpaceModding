//> -target:library
//> import: Assembly-CSharp
//> import: UnityEngine.CoreModule
using System;
using UnityEngine;

namespace Rosuav {
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class HelloWorldPlugin : MonoBehaviour {
		private float lastFixedUpdate = 0.0f;
		void Start() {
			print("Hello, world, Start!");
		}
		void OnDestroy() {
			print("Hello, world, or rather goodbye.");
		}
		void FixedUpdate()
		{
			if ((Time.time - lastFixedUpdate) > 1.0f)
			{
				lastFixedUpdate = Time.time;
				Vessel self = FlightGlobals.ActiveVessel;
				if (!self) {print("Hello, world, tick, tock"); return;}
				print("Hello, vessel named " + self.vesselName + " " + self.latitude + " " + self.longitude);
				FinePrint.Waypoint waypoint = self.navigationWaypoint;
				if (waypoint == null) {print("Hello waypoint, nope no waypoint"); return;}
				print("Hello waypoint: " + waypoint.name + " " + waypoint.latitude + " " + waypoint.longitude);
			}
		}
	}
}
