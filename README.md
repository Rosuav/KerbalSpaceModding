How hard can rocket modding be?!?
- distance to nav mark

Hello World:
- OnFundsChanged https://wiki.kerbalspaceprogram.com/wiki/API:GameEvents
- onGamePause
- onTimeWarpRateChanged

Real project:
- https://wiki.kerbalspaceprogram.com/wiki/API:Vessel
- mainBody to get CelestialBody
- latitude, longitude
- srf_velocity (3D vector)

Basics:
- Compile using msc foo.cs
- Run using mono foo.exe, but maybe not if dll


Error:
   HelloWorld.cs(10,35): error CS1070: The type `UnityEngine.MonoBehaviour' has been forwarded to an
   assembly that is not referenced. Consider adding a reference to assembly `UnityEngine.CoreModule,
   Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'

Solution: Add UnityEngine.CoreModule to list of references

Great example: https://github.com/taraniselsu/TacExamples/blob/main/01-SimplePartlessPlugin/Source/SimplePartlessPlugin.cs
