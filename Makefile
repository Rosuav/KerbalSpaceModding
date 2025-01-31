all: install1 install2 Velocimeter.zip

install1: VelocimeterModule.dll velocimeter.cfg velocimeter.png AddNodesToServiceBays.cfg
	cp VelocimeterModule.dll ../.steam/steam/steamapps/common/Kerbal\ Space\ Program/GameData/Rosuav/Plugins/
	cp *.cfg *.png ../.steam/steam/steamapps/common/Kerbal\ Space\ Program/GameData/Rosuav/Parts/Utility/

install2: VelociTwo.dll
	cp VelociTwo.dll ../.steam/steam/steamapps/common/Kerbal\ Space\ Program\ 2/BepInEx/plugins/Rosuav/

ArmstrongNav.dll: ArmstrongNav.cs
	monobuild.py $<

VelocimeterModule.dll: VelocimeterModule.cs
	monobuild.py $<

VelociTwo.dll: VelociTwo.cs
	monobuild.py $<

velocimeter.png: dinoart.pike
	pike $< $@

Velocimeter.zip: VelocimeterModule.dll velocimeter.cfg velocimeter.png
	python3 makezip.py
