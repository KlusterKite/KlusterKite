import platform
import subprocess
import shutil
import os
import re

imagesList = subprocess.Popen("docker images", shell=True, stdout=subprocess.PIPE).stdout.read()
for image in imagesList.split("\n"):
	parts = re.split("[\t ]+", image)
	if (parts[0] == "<none>"):		
		subprocess.call("docker rmi " + parts[2]);
		pass