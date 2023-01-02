import zipfile
with zipfile.ZipFile("Velocimeter.zip", "w") as zip:
	zip.write("VelocimeterModule.dll", "Rosuav/Plugins/VelocimeterModule.dll", compress_type=zipfile.ZIP_DEFLATED)
	zip.write("velocimeter.cfg", "Rosuav/Parts/Utility/velocimeter.cfg", compress_type=zipfile.ZIP_DEFLATED)
	zip.write("velocimeter.png", "Rosuav/Parts/Utility/velocimeter.png", compress_type=zipfile.ZIP_DEFLATED)
