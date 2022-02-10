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
		}

		[KSPField(isPersistant = false, guiActive = true, guiName = "Poke count")]
		public int poke_count = 0;
		[KSPEvent(guiActive = true, guiName = "Poke")]
		public void PokeEvent()
		{
			++poke_count;
		}

	}
}
