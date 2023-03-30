//> path: KSP2
//> -target:library
//> import: Assembly-CSharp
//> import: UnityEngine
//> import: UnityEngine.CoreModule
//> import: UnityEngine.UI
using BepInEx;

namespace Rosuav {
	[BepInPlugin("com.rosuav.velocitwo", "VelociTwo", "1.0.0.0")]
	public class VelociTwoPlugin : BaseUnityPlugin
	{
		public VelociTwoPlugin()
		{
			Logger.LogError("Hello, world from constructor!");
			print("[VelociTwo] Hello, world 1!");
		}
		private void Start()
		{
			Logger.LogError("Hello, world from Start()!");
			print("[VelociTwo] Hello, world 2!");
		}
		private void Awake()
		{
			Logger.LogError("Hello, world from Rosuav VelociTwo!");
		}
	}
}
