install: HelloWorld.dll
	cp *.dll ../.steam/steam/steamapps/common/Kerbal\ Space\ Program/GameData/Rosuav/Plugins/

HelloWorld.dll: HelloWorld.cs
	./build.py $<
