//> -target:library
//> import: Assembly-CSharp
//> import: UnityEngine.CoreModule
//> import: UnityEngine.UI
using System;
using UnityEngine;

namespace Rosuav {
	public class VelocimeterModule : PartModule {
		[KSPField(isPersistant = false, guiActive = true, guiName = "Destination dist", guiFormat = "n/a", guiUnits = " m")]
		public double destination_dist = 0.0;
		[KSPField(isPersistant = false, guiActive = true, guiName = "Approach speed", guiFormat = "n/a", guiUnits = " m/s")]
		public double approach_velocity = 0.0;
		[KSPField(isPersistant = false, guiActive = true, guiName = "Time to arrival", guiFormat = "n/a", guiUnits = " sec")]
		public double arrival_time = 0.0;
		[KSPField(isPersistant = false, guiActive = true, guiName = "Location")]
		public string celestial_body_name = "";
		[KSPField(isPersistant = false, guiActive = true, guiName = "Situation")]
		public string situation = "";
		[KSPField(isPersistant = false, guiActive = true, guiName = "Biome")]
		public string biome = "";
		//TODO: Also show the surface normal, or at least the magnitude of its dot product with vertical.

		void FixedUpdate()
		{
			Vessel self = part.vessel;
			if (!self) return;
			//If there's a nav marker set, that is our goal; show the distance
			//to it and time to arrival.
			FinePrint.Waypoint waypoint = self.navigationWaypoint;
			CelestialBody surface = self.mainBody;
			if (waypoint != null && surface != null) {
				approach_velocity = self.GetSrfVelocity().magnitude;
				//Calculate great circle distance from lat/long and radius
				double lat1 = self.latitude * Math.PI / 180, lat2 = waypoint.latitude * Math.PI / 180;
				double longs = (self.longitude - waypoint.longitude) * Math.PI / 180;
				double angle = Math.Acos(Math.Sin(lat1) * Math.Sin(lat2) +
					Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(longs));
				destination_dist = angle * surface.Radius;
			} else {
				//No nav marker? Then our goal is to land immediately below us.
				//Useful esp on places with no atmosphere, where you need to use
				//rockets to descend (rather than just popping a parachute and
				//time-warping till you land). Track your descent rate.
				//self.terrainNormal could be useful - would presumably show the angle of the ground under us
				if (self.verticalSpeed < -1.0) { //Positive verticalSpeed means ascending
					destination_dist = self.heightFromTerrain;
					approach_velocity = -self.verticalSpeed;
				}
				else approach_velocity = 0.0;
			}
			if (approach_velocity < 0.0) {
				approach_velocity = destination_dist = arrival_time = 0.0;
				Fields["approach_velocity"].guiFormat = Fields["destination_dist"].guiFormat
					= Fields["arrival_time"].guiFormat = "n/a";
			}
			else {
				arrival_time = destination_dist / approach_velocity;
				if (approach_velocity >= 10.0) Fields["approach_velocity"].guiFormat = "####";
				else Fields["approach_velocity"].guiFormat = "#.#";
				if (arrival_time >= 10.0) Fields["arrival_time"].guiFormat = "####";
				else Fields["arrival_time"].guiFormat = "#.#";
			}
			//Borrowing some ideas from MechJeb here
			celestial_body_name = surface.name;
			if (self.landedAt != "") {
				situation = "Landed!";
				biome = self.landedAt;
			} else {
				switch (vessel.situation) {
					case Vessel.Situations.PRELAUNCH: situation = "Pre-launch"; break;
					case Vessel.Situations.LANDED: situation = "Landed?"; break; //Different from having LandedAt?
					case Vessel.Situations.SPLASHED: situation = "Splashed down"; break;
					case Vessel.Situations.FLYING:
						if (vessel.altitude < surface.scienceValues.flyingAltitudeThreshold)
							situation = "Flying low";
						else situation = "Flying high";
						break;
					default:
						if (vessel.altitude < surface.scienceValues.spaceAltitudeThreshold)
							situation = "In space low";
						else situation = "In space high";
						break;
				}
				biome = surface.BiomeMap.GetAtt(self.latitude * UtilMath.Deg2Rad, self.longitude * UtilMath.Deg2Rad).name;
			}
		}
	}
}
