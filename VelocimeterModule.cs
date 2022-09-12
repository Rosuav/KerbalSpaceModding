//> -target:library
//> import: Assembly-CSharp
//> import: UnityEngine.CoreModule
//> import: UnityEngine.UI
using System;
using UnityEngine;

namespace Rosuav {
	public class VelocimeterModule : PartModule {
		[KSPField(isPersistant = true, guiActiveEditor = true, guiName = "Auto-open on launch"), UI_Toggle(enabledText = "On", disabledText = "Off")]
		public bool autoopen = false;
		[KSPField(isPersistant = false, guiActive = true, guiName = "Display mode")]
		public string display_mode = "";
		[KSPField(isPersistant = false, guiActive = true, guiName = "Destination dist", guiFormat = "n/a", guiUnits = " m")]
		public double destination_dist = 0.0;
		[KSPField(isPersistant = false, guiActive = true, guiName = "Approach speed", guiFormat = "n/a", guiUnits = " m/s")]
		public double approach_velocity = 0.0;
		[KSPField(isPersistant = false, guiActive = true, guiName = "Time to arrival", guiFormat = "n/a", guiUnits = " sec")]
		public double arrival_time = 0.0;
		[KSPField(isPersistant = false, guiActive = true, guiName = "Situation")]
		public string situation = "";
		[KSPField(isPersistant = false, guiActive = true, guiName = "Biome")]
		public string biome = "";
		//TODO: Also show the surface normal, or at least the magnitude of its dot product with vertical.

		//Optionally open the PAW when the vessel launches. Note that attempting to
		//directly open the PAW from inside OnStart has had nothing but issues of
		//various kinds, and even doing it on the first FixedUpdate fails, so we
		//wait until it's been a little while.
		int countdown = 0;
		public override void OnStart(StartState state) {
			if ((state & StartState.PreLaunch) > 0 && autoopen) countdown = 32; //About a second's delay
		}

		void FixedUpdate()
		{
			Vessel self = part.vessel;
			if (!self) return;
			if (countdown > 0) {
				if (--countdown == 0) UIPartActionController.Instance.SpawnPartActionWindow(part);
			}
			//If there's a nav marker set, that is our goal; show the distance
			//to it and time to arrival.
			FinePrint.Waypoint waypoint = self.navigationWaypoint;
			CelestialBody surface = self.mainBody;
			ITargetable target = self.targetObject;
			if (target != null) {
				Orbit targorb = target.GetOrbit(), selforb = self.orbit;
				if (targorb.referenceBody != selforb.referenceBody) {
					//Can't really do much here.
					approach_velocity = 0.0;
					display_mode = "Target (too far)";
				} else {
					display_mode = "Target";
					//Show the current phase angle as "distance" that we are apart (negated -
					//the distance we have to close is the opposite of our phase angle)
					double now = Planetarium.GetUniversalTime();
					destination_dist = (selforb.PhaseAngle(now) - targorb.PhaseAngle(now)) * 180 / Math.PI;
					//"Velocity" is now measured in degrees per orbit and shows how much the
					//destination distance will change each time we go around the planet.
					approach_velocity = (selforb.period - targorb.period) / targorb.period * 360 % 360;
					//Make the sign of the destination distance match that of approach velocity
					if (approach_velocity < 0 && destination_dist > 0) destination_dist -= 360;
					if (approach_velocity > 0 && destination_dist < 0) destination_dist += 360;
					if (destination_dist < 0) {destination_dist = -destination_dist; approach_velocity = -approach_velocity;}
				}
			}
			else if (waypoint != null && surface != null) {
				approach_velocity = self.GetSrfVelocity().magnitude;
				//Calculate great circle distance from lat/long and radius
				double lat1 = self.latitude * Math.PI / 180, lat2 = waypoint.latitude * Math.PI / 180;
				double longs = (self.longitude - waypoint.longitude) * Math.PI / 180;
				double angle = Math.Acos(Math.Sin(lat1) * Math.Sin(lat2) +
					Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(longs));
				destination_dist = angle * surface.Radius;
				display_mode = "Navigation";
			} else {
				//No nav marker? Then our goal is to land immediately below us.
				//Useful esp on places with no atmosphere, where you need to use
				//rockets to descend (rather than just popping a parachute and
				//time-warping till you land). Track your descent rate.
				//self.terrainNormal could be useful - would presumably show the angle of the ground under us
				destination_dist = self.heightFromTerrain;
				approach_velocity = -self.verticalSpeed; //Positive verticalSpeed means ascending
				display_mode = "Descent";
			}
			if (display_mode == "Target") {
				//Orbital approaches are measured angularly
				Fields["destination_dist"].guiUnits = "°";
				Fields["approach_velocity"].guiUnits = "°/orb";
				Fields["arrival_time"].guiUnits = " orb";
				Events["CreateNode"].guiName = "Plane Match";
			} else {
				//Direct approaches are measured linearly
				Fields["destination_dist"].guiUnits = " m";
				Fields["approach_velocity"].guiUnits = " m/sec";
				Fields["arrival_time"].guiUnits = " sec";
				Events["CreateNode"].guiName = "Circularize";
			}
			if (display_mode != "Target" && approach_velocity < 1.0) { //Below 1 m/s, the calculations tend to just show noise.
				approach_velocity = destination_dist = arrival_time = 0.0;
				Fields["approach_velocity"].guiFormat = Fields["destination_dist"].guiFormat
					= Fields["arrival_time"].guiFormat = "n/a";
			}
			else {
				arrival_time = destination_dist / approach_velocity;
				if (Math.Abs(destination_dist) >= 10.0) Fields["destination_dist"].guiFormat = "####"; //Dedup these?
				else Fields["destination_dist"].guiFormat = "0.0";
				if (Math.Abs(approach_velocity) >= 10.0) Fields["approach_velocity"].guiFormat = "####";
				else Fields["approach_velocity"].guiFormat = "0.0";
				if (arrival_time >= 10.0) Fields["arrival_time"].guiFormat = "####";
				else Fields["arrival_time"].guiFormat = "0.0";
			}
			//Borrowing some ideas from MechJeb here
			if (self.landedAt != "") {
				situation = "Landed";
				biome = self.landedAt;
			} else {
				switch (vessel.situation) {
					case Vessel.Situations.PRELAUNCH: situation = "Pre-launch"; break;
					case Vessel.Situations.LANDED: situation = "Landed"; break; //Different from having LandedAt?
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
			check_autothrust();
		}

		[KSPEvent(guiActive = true, guiName = "Circularize", active = true)]
		public void CreateNode() {
			//1. Find the next apoapsis or periapsis.
			//2. Calculate the dV needed to circularize.
			//3. Create a maneuver node precisely at apo/periapsis, specifying the burn required.
			Vessel self = part.vessel;
			if (!self) return;
			Orbit orbit = self.orbit;
			double now = Planetarium.GetUniversalTime();
			ITargetable target = self.targetObject;
			if (target != null) {
				Orbit targorb = target.GetOrbit();
				if (targorb.referenceBody == orbit.referenceBody) {
					//Plane match. Find the next asc/desc node and create a burn that will
					//match inclination with the target. This should be equal to 2*v*sin(incl/2)
					//where v is the orbital velocity at the node.
					double incl = orbit.GetRelativeInclination(targorb);
					double ta_asc = Orbit.AscendingNodeTrueAnomaly(orbit, targorb);
					double ta_dsc = Orbit.DescendingNodeTrueAnomaly(orbit, targorb);
					double node = orbit.GetDTforTrueAnomaly(ta_dsc, 0.0) + now;
					double asc_node = orbit.GetDTforTrueAnomaly(ta_asc, 0.0) + now;
					if (asc_node < node) {node = asc_node; incl = -incl;}
					//node is the UT until the next node, at which we'll have to planeshift by
					//incl degrees. This plane shift requires dV that depends on the velocity
					//at that point on the orbit.
					//NOTE: I'm assuming that the inclination will never exceed 180°, and thus
					//that its sine will always be positive. Ergo, by negating the inclination
					//above, we also negate the sine, and thus the burn, putting it the other
					//direction.
					double vel = orbit.getOrbitalVelocityAtUT(node).magnitude;
					double shift = 2 * vel * Math.Sin(incl / 2 * Math.PI / 180);
					self.patchedConicSolver.AddManeuverNode(node).DeltaV = new Vector3d(0, shift, 0);
					vessel.patchedConicSolver.UpdateFlightPlan();
					return;
				}
			}
			double apo = orbit.GetNextApoapsisTime(now);
			double peri = orbit.GetNextPeriapsisTime(now);
			double apsis = peri;
			//If flyby, we may not have an apoapsis. Otherwise, take whichever is sooner.
			if (apo > now && apo < peri) apsis = apo;
			double curvel = orbit.getOrbitalVelocityAtUT(apsis).magnitude;
			//Calculate the velocity of a circular orbit at the given radius.
			//Note that a "launch safety" semi-circularization could aim for an elliptical
			//orbit with a periapsis of anything above atmosphere or the highest mountain,
			//and a "flyby capture" one could aim for any apoapsis within the body's SOI,
			//but for this simplified version, we simply aim for the altitude of the current
			//apoapsis. You can always edit the node afterwards and weaken it to what you need.
			CelestialBody body = orbit.referenceBody;
			double rad = orbit.GetRadiusAtUT(apsis); //== orbital altitude plus the body's radius
			double needvel = Math.Sqrt(9.807 * body.GeeASL / rad) * body.Radius;
			self.patchedConicSolver.AddManeuverNode(apsis).DeltaV = new Vector3d(0, 0, needvel - curvel);
			vessel.patchedConicSolver.UpdateFlightPlan();
		}

		enum AT {Idle, Wait, Burn}; AT AT_mode;
		double autothrust_last_dv;
		string[] AT_ModeDesc = {"AutoThrust: idle", "AutoThrust: wait", "AutoThrust: burn"};
		[KSPEvent(guiActive = true, guiName = "AutoThrust: inactive", active = true)]
		public void AutoThrust() {
			AT_mode = AT_mode == AT.Idle ? AT.Wait : AT.Idle;
			check_autothrust();
		}
		void check_autothrust() {
			//Ideally this should be stateless, save for the mode selection.
			//Currently we're also tracking the node's delta-V though.
			Vessel self = part.vessel;
			PatchedConicSolver solver = self.patchedConicSolver;
			//Deactivation triggers happen even if we don't have full control.
			//If you have no maneuver node, or if you activate time warp, AT deactivates.
			if (solver.maneuverNodes.Count == 0) AT_mode = AT.Idle;
			if (TimeWarp.CurrentRateIndex > 0) AT_mode = AT.Idle;
			if (AT_mode == AT.Wait && self.CurrentControlLevel == Vessel.ControlLevel.FULL) {
				//We have full control. Either it's manned (by a competent pilot - if
				//the only pilot goes EVA, control level becomes NONE), or it has a
				//probe core, electric charge, and a commlink to KSC (or possibly a
				//drone control unit nearby - haven't checked). AutoThrust is active.

				FlightCtrlState ctrl = self.ctrlState;
				ManeuverNode node = solver.maneuverNodes[0];
				//Start burning half way before the maneuver node
				if (node.startBurnIn < 0.0 || ctrl.mainThrottle > 0.0) {AT_mode = AT.Burn; autothrust_last_dv = Double.PositiveInfinity;}
			}
			if (AT_mode == AT.Burn && self.CurrentControlLevel == Vessel.ControlLevel.FULL) {
				FlightCtrlState ctrl = self.ctrlState;
				ManeuverNode node = solver.maneuverNodes[0];
				//double time = node.UT - Planetarium.GetUniversalTime(); //Time to *middle* of burn
				//double halfburn = time - node.startBurnIn; //The burn begins half a burn before the node's specified time. Ergo it ends half a burn after.
				//Calculating based on burn time doesn't seem to work though
				double dv = node.GetPartialDv().magnitude;
				//Once the delta-V for the burn starts going up instead of down, cut the engines.
				if (dv > autothrust_last_dv + 0.001) {
					AT_mode = AT.Idle;
					print(String.Format("[ArmstrongNav] dV increased {0:0.000} -> {1:0.000}", autothrust_last_dv, dv));
				}
				else ctrl.mainThrottle = 1.0f;
				autothrust_last_dv = dv;
			}
			Events["AutoThrust"].guiName = AT_ModeDesc[(int)AT_mode];
		}
	}
}
