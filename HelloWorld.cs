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
				print("Hello, world, tick, tock");
			}
		}
	}
}
