install: ArmstrongNav.dll
	cp *.dll ../.steam/steam/steamapps/common/Kerbal\ Space\ Program/GameData/Rosuav/Plugins/

ArmstrongNav.dll: ArmstrongNav.cs
	./build.py $<
