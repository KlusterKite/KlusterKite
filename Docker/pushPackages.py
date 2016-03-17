import platform
import subprocess
import shutil
import os
import re

shutil.rmtree('./packages', True);
os.mkdir('./packages');

#'''
packageDirs = os.listdir("../packages")
for dir in packageDirs:
	if (os.path.isdir(os.path.join("../packages",dir))):
		for file in os.listdir(os.path.join("../packages",dir)):
			if (file.endswith(".nupkg")):
				shutil.copyfile(os.path.join("../packages",dir, file), os.path.join(".","packages", file))
#'''

if platform.system() == 'Windows':
	subprocess.Popen("xcopy ..\\packageOut\\* .\\packages\\ /Y", shell=True, stdout=subprocess.PIPE).wait()
else:
	subprocess.Popen("cp ../packageOut/* ./packages/", shell=True, stdout=subprocess.PIPE).wait()


packageDirs = os.listdir("packages")
for file in packageDirs:
	if (file.endswith(".nupkg")):
		if platform.system() == 'Windows':
			subprocess.Popen("nuget push " + os.path.join("packages", file) + " -Source http://192.168.99.100:81/ -ApiKey ClusterKit", shell=False).wait()
		else:
			subprocess.Popen("mono nuget.exe push " + os.path.join("packages", file) + " -Source http://192.168.99.100:81/ -ApiKey ClusterKit", shell=False).wait()


#for file in packages/*; do mono nuget.exe push $file -Source http://localhost/; done