//> -target:library
//> import: Assembly-CSharp
//> import: UnityEngine.CoreModule
//> import: UnityEngine.UI
using System;
using UnityEngine;

namespace Rosuav {
	public class VelocimeterModule : PartModule {
		[KSPField(isPersistant = false, guiActive = true, guiName = "Horiz velo", guiFormat = "n/a", guiUnits = " m/s")]
		public double horizontal_velocity = 0.0;
		[KSPField(isPersistant = false, guiActive = true, guiName = "Descent velo", guiFormat = "n/a", guiUnits = " m/s")]
		public double descent_velocity = 0.0;
		[KSPField(isPersistant = false, guiActive = true, guiName = "Time to contact", guiFormat = "n/a", guiUnits = " sec")]
		public double impact_time = 0.0;
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
			//self.terrainNormal could be useful - would presumably show the angle of the ground under us
			if (self.verticalSpeed < -1.0) { //Positive verticalSpeed means ascending
				descent_velocity = -self.verticalSpeed;
				Fields["descent_velocity"].guiFormat = "####";
				impact_time = self.heightFromTerrain / -self.verticalSpeed;
				if (impact_time >= 10.0) Fields["impact_time"].guiFormat = "####";
				else Fields["impact_time"].guiFormat = "#.#";
			}
			else {
				descent_velocity = impact_time = 0.0;
				Fields["descent_velocity"].guiFormat = Fields["impact_time"].guiFormat = "n/a";
			}
			//Borrowing some ideas from MechJeb here
			celestial_body_name = self.mainBody.name;
			if (self.landedAt != "") {
				situation = "Landed!";
				biome = self.landedAt;
			} else {
				switch (vessel.situation) {
					case Vessel.Situations.PRELAUNCH: situation = "Pre-launch"; break;
					case Vessel.Situations.LANDED: situation = "Landed?"; break; //Different from having LandedAt?
					case Vessel.Situations.SPLASHED: situation = "Splashed down"; break;
					case Vessel.Situations.FLYING:
						if (vessel.altitude < self.mainBody.scienceValues.flyingAltitudeThreshold)
							situation = "Flying low";
						else situation = "Flying high";
						break;
					default:
						if (vessel.altitude < self.mainBody.scienceValues.spaceAltitudeThreshold)
							situation = "In space low";
						else situation = "In space high";
						break;
				}
				biome = self.mainBody.BiomeMap.GetAtt(self.latitude * UtilMath.Deg2Rad, self.longitude * UtilMath.Deg2Rad).name;
			}
		}
	}
}
