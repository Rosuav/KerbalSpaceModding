#!/usr/bin/env python3
import os
import subprocess
import sys
fn = sys.argv[1]
cmd = ["mcs"]
path = os.path.expanduser("~/.steam/steam/steamapps/common/Kerbal Space Program/KSP_Data/Managed")
with open(fn) as f:
	for line in f:
		line = line.strip()
		if not line.startswith("//>"): break
		if line.startswith("//> -"): cmd.append(line[4:])
		if line.startswith("//> import: "):
			# Load up an assembly that is needed
			cmd.append("-r:" + path + "/" + line[12:] + ".dll")
cmd.append(fn)
subprocess.run(cmd, check=True)
