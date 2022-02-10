//> -target:library
//> import: Assembly-CSharp
//> import: UnityEngine.CoreModule
//> import: UnityEngine.UI
using System;
using UnityEngine;

namespace Rosuav {
	public class VelocimeterModule : PartModule {
		void FixedUpdate()
		{
			//print("[Velocimeter] Hello world");
		}

		private int poke_count = 0;
		[KSPEvent(guiActive = true, guiName = "Poke")]
		public void PokeEvent()
		{
			++poke_count;
			print(String.Format("[Velocimeter] Poked! {0} {1}", poke_count, part.vessel.GetName()));
			Events["PokeEvent"].guiName = String.Format("Poked {0} times", poke_count);
		}
	}
}
