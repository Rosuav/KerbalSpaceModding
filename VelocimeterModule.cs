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
				Fields["destination_dist"].guiFormat = "####";
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

		[KSPEvent(guiActive = true, guiName = "Circularize", active = true)]
		public void CreateCircularizationNode() {
			print("[Circularize] Create circularization node!");
			//1. Find the next apoapsis or periapsis.
			//   Assume for now that there's enough time for the burn. If there isn't, the user will
			//   have to choose to wait until after that point and then do the opposite circularization.
			//2. Calculate the dV needed to circularize. For now, cheat and just get the direction right.
			//   An apoapsis burn is prograde, a periapsis burn is retrograde. This may happen "for free"
			//   if we just look at "velocity here" and "velocity on opposite of orbit".
			//3. Create a maneuver node precisely at apo/periapsis, specifying the burn required.
			Vessel self = part.vessel;
			if (!self) return;
			Orbit orbit = self.orbit;
			double now = Planetarium.GetUniversalTime();
			double apo = orbit.GetNextApoapsisTime(now);
			double peri = orbit.GetNextPeriapsisTime(now);
			double here = apo, there = peri;
			if (here > there) {here = peri; there = apo;}
			double velhere = orbit.getOrbitalVelocityAtUT(here).magnitude;
			double velthere = orbit.getOrbitalVelocityAtUT(there).magnitude;
			print(String.Format("[Circularize] Here {0:0.00} m/s There {1:0.00} m/s Now {2:0.00} m/s",
				velhere, velthere, orbit.vel.magnitude
			));
			//Calculate the velocity of a circular orbit at the given radius.
			//Note that a "launch safety" semi-circularization could aim for an elliptical
			//orbit with a periapsis of anything above atmosphere or the highest mountain,
			//but for this simplified version, we simply aim for the altitude of the current
			//apoapsis. You can always edit the node afterwards and weaken it to what you need.
			CelestialBody body = orbit.referenceBody;
			double rad = orbit.GetRadiusAtUT(here); //== orbital altitude plus the body's radius
			double grav = body.GeeASL * 9.807;
			double vel = Math.Sqrt(grav / rad) * body.Radius;
			print(String.Format("[Circularize] Altitude will be {0:0.00} Grav {1:0.00} Radius {2:0.00}",
				rad, grav, body.Radius
			));
			ManeuverNode node = self.patchedConicSolver.AddManeuverNode(here);
			node.DeltaV = new Vector3d(0, 0, vel - velhere);
			vessel.patchedConicSolver.UpdateFlightPlan();
			print(String.Format("[Circularize] Difference {0:0.00} m/s mag {1:0.00} m/s",
				vel - velhere,
				node.DeltaV.magnitude
			));
		}
	}
}
