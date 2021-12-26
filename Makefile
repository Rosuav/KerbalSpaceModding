install: HelloWorld.dll
	cp *.dll ../.steam/steam/steamapps/common/Kerbal\ Space\ Program/GameData/Rosuav/Plugins/

HelloWorld.dll: HelloWorld.cs
	mcs -r:../.steam/steam/steamapps/common/Kerbal\ Space\ Program/KSP_Data/Managed/{Assembly-CSharp,UnityEngine.CoreModule}.dll HelloWorld.cs -t:library
