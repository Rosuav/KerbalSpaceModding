using System;
using UnityEngine;

namespace Rosuav {
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class HelloWorldPlugin : MonoBehaviour {
		private float lastFixedUpdate = 0.0f;
		void Awake() {
			print("Hello, world, Awake!");
		}
		void Start() {
			print("Hello, world, Start!");
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
