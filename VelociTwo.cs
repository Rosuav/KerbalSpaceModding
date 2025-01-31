//> path: ~/.steam/steam/steamapps/common/Kerbal Space Program 2/KSP2_x64_Data/Managed
//> path: ~/.steam/steam/steamapps/common/Kerbal Space Program 2/BepInEx/core
//> -target:library
//> import: BepInEx
//> import: Assembly-CSharp
//> import: UnityEngine
//> import: UnityEngine.CoreModule
//> import: UnityEngine.UI
//> import: netstandard
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
