install: ArmstrongNav.dll VelocimeterModule.dll velocimeter.cfg
	cp *.dll ../.steam/steam/steamapps/common/Kerbal\ Space\ Program/GameData/Rosuav/Plugins/
	cp *.cfg ../.steam/steam/steamapps/common/Kerbal\ Space\ Program/GameData/Rosuav/Parts/Utility/

ArmstrongNav.dll: ArmstrongNav.cs
	./build.py $<

VelocimeterModule.dll: VelocimeterModule.cs
	./build.py $<
