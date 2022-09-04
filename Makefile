install: VelocimeterModule.dll velocimeter.cfg velocimeter.png
	cp *.dll ../.steam/steam/steamapps/common/Kerbal\ Space\ Program/GameData/Rosuav/Plugins/
	cp *.cfg *.png ../.steam/steam/steamapps/common/Kerbal\ Space\ Program/GameData/Rosuav/Parts/Utility/

ArmstrongNav.dll: ArmstrongNav.cs
	./build.py $<

VelocimeterModule.dll: VelocimeterModule.cs
	./build.py $<

velocimeter.png: dinoart.pike
	pike $< $@
