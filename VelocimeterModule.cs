//> path: ~/.steam/steam/steamapps/common/Kerbal Space Program/KSP_Data/Managed
//> -target:library
//> import: Assembly-CSharp
//> import: UnityEngine.CoreModule
//> import: UnityEngine.UI
using System;
using System.Linq;
using UnityEngine;

namespace Rosuav {
	public class VelocimeterModule : PartModule {
		//NOTE: This could potentially become annoying, auto-opening every time you switch to a vessel.
		//But having the Auto-Open toggle available during flight (guiActive = true) is itself annoying,
		//so for now, I've kept it editor-only despite having post-launch functionality.
		[KSPField(isPersistant = true, guiActiveEditor = true, guiName = "Auto-open"), UI_Toggle(enabledText = "On", disabledText = "Off")]
		public bool autoopen = false;
		[KSPField(isPersistant = true, guiActiveEditor = true, guiName = "Mission numbering"), UI_Toggle(enabledText = "Active", disabledText = "Inactive")]
		public bool mission_numbering = false;
		[KSPField(isPersistant = false, guiActive = true, guiName = "Display mode")]
		public string display_mode = "";
		[KSPField(isPersistant = false, guiActive = true, guiName = "Destination dist", guiFormat = "n/a", guiUnits = " m")]
		public double destination_dist = 0.0;
		[KSPField(isPersistant = false, guiActive = true, guiName = "Approach speed", guiFormat = "n/a", guiUnits = " m/s")]
		public double approach_velocity = 0.0;
		[KSPField(isPersistant = false, guiActive = true, guiName = "Time to arrival", guiFormat = "n/a", guiUnits = " sec")]
		public double arrival_time = 0.0;
		[KSPField(isPersistant = false, guiActive = true, guiName = "Time to atmo", guiFormat = "n/a", guiUnits = " sec")]
		public double atmosphere_time = 0.0;
		[KSPField(isPersistant = false, guiActive = true, guiName = "Situation")]
		public string situation = "";
		[KSPField(isPersistant = false, guiActive = true, guiName = "Biome")]
		public string biome = "";
		//TODO: Also show the surface normal, or at least the magnitude of its dot product with vertical.

		//Optionally open the PAW when the vessel loads in. Note that attempting to
		//directly open the PAW from inside OnStart has had nothing but issues of
		//various kinds, and even doing it on the first FixedUpdate fails, so we
		//wait until it's been a little while.
		int countdown = 0;
		public override void OnStart(StartState state) {
			//NOTE: The StartState enumeration mentions an "Editor" state, but I've never seen it
			//actually trigger. It might be necessary to explicitly exclude this from the autoopen
			//behaviour, but for now I haven't bothered.
			if (autoopen) countdown = 32; //About a second's delay
			if ((state & StartState.PreLaunch) > 0) { //Only add mission numbering on first launch (there's no way to enable it post-editor anyway)
				if (mission_numbering) {
					//Allow Jebediah Kerman to keep track of mission numbering for us.
					//If he is dead, we record mission numbers on his tombstone - it
					//still works. Note: The career log assumes keywords. Spaces or
					//special characters in the vessel name break it. This filter means
					//the end result is always valid, but it can fold similar names to
					//each other, which would make them share mission numbering.
					string basename = new string(part.vessel.vesselName
						.Where(char.IsLetter).ToArray());
					//If the name ends with a digit, assume that we already gave this
					//vessel a mission number. Resetting to launch keeps the name as it
					//was during the flight. It may possibly be better to hold off the
					//rename until some number of ticks later? Not sure.
					if (basename != "" && !Char.IsDigit(part.vessel.vesselName[part.vessel.vesselName.Length - 1])) {
						FlightLog jeb = HighLogic.CurrentGame.CrewRoster[0].careerLog;
						jeb.AddEntry("MissionNumbering", basename);
						//int count = jeb.GetEntries("MissionNumbering", basename).Length; //Nope, always zero.
						//int count = jeb.GetEntries("MissionNumbering").Length; //Same problem.
						//foreach (FlightLog.Entry entry in jeb) ; //Not iterable.
						//So I guess we do this manually.
						int count = 0;
						for (int i = 0; i < jeb.Count; ++i) {
							FlightLog.Entry entry = jeb[i];
							if (entry.type == "MissionNumbering" && entry.target == basename) ++count;
						}
						string name = String.Format("{0} {1}", part.vessel.vesselName, count);
						if (Vessel.IsValidVesselName(name)) part.vessel.vesselName = name;
						//print(String.Format("[ArmstrongNav] LAUNCH {0} {1}", part.vessel.GetName(), part.vessel.GetDisplayName()));
					}
				}
			}
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
					//Can't really do much here. Or can we look for an SOI change?
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
					//WIP: Show your periapsis or apoapsis and the altitude at the nearest point
					//on the target orbit. Whichever is closer, or just trust to the SMA.
					//Test orbit: 27e9 meters 0.5 ecc, target Eeloo
					//Other test: 13e9 meters 0.05 ecc, target Eve, MNA 3.14
					//If working with peri, don't add 180°, and use PeR instead of ApR
					double self_apsis_long = (selforb.LAN + selforb.argumentOfPeriapsis + 180.0) % 360.0;
					double self_radius = selforb.ApR;
					string lbl = "apo";
					if (selforb.semiMajorAxis > targorb.semiMajorAxis) {
						self_apsis_long = (selforb.LAN + selforb.argumentOfPeriapsis) % 360.0;
						self_radius = selforb.PeR;
						lbl = "peri";
					}
					double targ_peri_long = targorb.LAN + targorb.argumentOfPeriapsis; //Never add 180° to this one
					double targ_anomaly = (self_apsis_long - targ_peri_long + 720.0) % 360.0;
					double targ_true_anom = targorb.GetTrueAnomaly(targ_anomaly);
					double targ_rad = targorb.RadiusAtTrueAnomaly(targ_true_anom);
					/*print(String.Format("[ArmstrongNav] My {0} long {1:0.00} rad {2:0.00} Targ long {3:0.00} anom {4:0.00} rad {5:0.00} Delta {6:0.00}",
						lbl, self_apsis_long, self_radius, targ_peri_long,
						targ_true_anom, targ_rad, targ_rad - self_radius));*/
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
				//It's not very useful to show "Plane Match" when you're so close that the
				//ascending/descending nodes show 0.0°, so indicate how far off you are.
				Orbit targorb = target.GetOrbit();
				double incl = self.orbit.GetRelativeInclination(targorb);
				Events["CreateNode"].guiName = String.Format("Plane Match: {0:0.0}°", incl);
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
			if (surface.atmosphere) {
				double atmo = surface.Radius + surface.atmosphereDepth;
				double now = Planetarium.GetUniversalTime();
				double next = self.orbit.GetNextTimeOfRadius(now, atmo);
				atmosphere_time = next - now;
			}
			else atmosphere_time = 0.0;
			if (atmosphere_time >= 10.0) Fields["atmosphere_time"].guiFormat = "####";
			else if (atmosphere_time > 0.0) Fields["atmosphere_time"].guiFormat = "0.0";
			else Fields["atmosphere_time"].guiFormat = "n/a";
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
		[KSPEvent(guiActive = true, guiName = "AutoThrust: idle", active = true)]
		public void AutoThrust() {
			if (AT_mode == AT.Idle) {
				AT_mode = AT.Wait;
				vessel.OnFlyByWire += new FlightInputCallback(burn); //Is it ever possible for this to add a duplicate?
			} else AT_mode = AT.Idle;
		}
		void check_autothrust(FlightCtrlState ctrl) {
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

				ManeuverNode node = solver.maneuverNodes[0];
				//If you manually raise the throttle, go into burn mode
				if (node.startBurnIn < 0.0 || ctrl.mainThrottle > 0.0) {AT_mode = AT.Burn; autothrust_last_dv = Double.PositiveInfinity;}
				else Events["AutoThrust"].guiName = String.Format("AutoThrust: wait {0:0.0}", node.startBurnIn);
			}
			if (AT_mode == AT.Burn && self.CurrentControlLevel == Vessel.ControlLevel.FULL) {
				ManeuverNode node = solver.maneuverNodes[0];
				double dv = node.GetPartialDv().magnitude;
				//Once the delta-V for the burn starts going up instead of down, cut the engines.
				if (dv > autothrust_last_dv + 0.001) AT_mode = AT.Idle;
				autothrust_last_dv = dv;
			}
		}
		void /* just want to watch the world*/ burn(FlightCtrlState ctrl) {
			check_autothrust(ctrl);
			switch (AT_mode) {
				case AT.Idle:
					vessel.OnFlyByWire -= new FlightInputCallback(burn); //Is this really right? Seems to work...
					ctrl.mainThrottle = 0.0f;
					Events["AutoThrust"].guiName = "AutoThrust: idle";
					break;
				case AT.Burn:
					ctrl.mainThrottle = 1.0f;
					Events["AutoThrust"].guiName = "AutoThrust: burn";
					break;
				case AT.Wait: ctrl.mainThrottle = 0.0f; break; //guiName is set inside check_autothrust here
			}
		}
	}
}
