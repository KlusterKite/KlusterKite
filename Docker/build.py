import platform
import subprocess
import shutil
import os
import re

def copyLib(libName, dest):
	if platform.system() == 'Windows':
		dest = dest.replace("/", "\\")
		subprocess.Popen("xcopy ..\\build\\tmp\\" + libName + "\\*.dll " + dest +"\\ /Y", shell=True, stdout=subprocess.PIPE).wait()
		subprocess.Popen("xcopy ..\\build\\tmp\\" + libName + "\\*.xml " + dest +"\\ /Y", shell=True, stdout=subprocess.PIPE).wait()
		subprocess.Popen("xcopy ..\\build\\tmp\\" + libName + "\\*.exe " + dest +"\\ /Y", shell=True, stdout=subprocess.PIPE).wait()
		subprocess.Popen("xcopy ..\\build\\tmp\\" + libName + "\\*.config " + dest +"\\ /Y", shell=True, stdout=subprocess.PIPE).wait()
	else:
		subprocess.Popen("cp ../build/tmp/" + libName + "/*.dll " + dest + "/", shell=True, stdout=subprocess.PIPE).wait()
		subprocess.Popen("cp ../build/tmp/" + libName + "/*.xml " + dest + "/", shell=True, stdout=subprocess.PIPE).wait()
		subprocess.Popen("cp ../build/tmp/" + libName + "/*.exe " + dest + "/" , shell=True, stdout=subprocess.PIPE).wait()
		subprocess.Popen("cp ../build/tmp/" + libName + "/*.config " + dest + "/", shell=True, stdout=subprocess.PIPE).wait()

def copyWebContent(webSource, dest):
	staticRe = re.compile(r"(.*)((\.jpg)|(\.gif)|(\.png)|(\.jpeg)|(\.html)|(\.html)|(\.js)|(\.css))$", re.IGNORECASE)
	try:
		os.mkdir(dest)
	except Exception, e:
		pass
	
	for fileName in os.listdir(webSource):
		fullName = os.path.join(webSource, fileName)
		if os.path.isdir(fullName) == True:
			copyWebContent(fullName, os.path.join(dest, fileName))
		elif os.path.isfile(fullName) and staticRe.match(fileName):
			shutil.copyfile(fullName, os.path.join(dest, fileName))
	return

def sysCopy(source, dest):
	if platform.system() == 'Windows':
		dest = dest.replace("/", "\\")
		source = source.replace("/", "\\")
		subprocess.Popen("xcopy " + source + " " + dest +" /Y /S /Q", shell=False).wait()
	else:
		subprocess.Popen("cp -R " + source + " " + dest, shell=False).wait()



def correctAssemblyVersions(buildFolder):
	if platform.system() == 'Windows':	
		subprocess.call("..\\ClusterKit.Tools\\compiled\\assemblyVersion\\DependentAssemblyVersionCorrector.exe "
		 + buildFolder 
		 + " " 
		 + buildFolder
		 + "\\ClusterKit.Core.Service.exe.config ", 
		 shell=True)
	else:
		subprocess.call("mono ../ClusterKit.Tools/compiled/assemblyVersion/DependentAssemblyVersionCorrector.exe "
		 + buildFolder 
		 + " " 
		 + buildFolder
		 + "/ClusterKit.Core.Service.exe.config ", 
		 shell=True)
	return

def prepareLauncher(dockerFolder):
	buildDir = os.path.join('.', dockerFolder, 'build')
	packageCacheDir = os.path.join('.', dockerFolder, 'packageCache')

	shutil.rmtree(buildDir, True);
	shutil.rmtree(packageCacheDir, True);
	os.mkdir(buildDir);
	os.mkdir(packageCacheDir);

	packageDirs = os.listdir("../packages")
	for dir in packageDirs:
		if (os.path.isdir(os.path.join("../packages",dir))):
			for file in os.listdir(os.path.join("../packages",dir)):
				if (file.endswith(".nupkg") and not os.path.isfile(os.path.join("..","packageOut", file))):
					shutil.copyfile(os.path.join("../packages",dir, file), os.path.join(packageCacheDir, file))		

	copyLib("ClusterKit.NodeManager.Launcher", buildDir)
	shutil.copyfile(os.path.join(dockerFolder, 'ClusterKit.NodeManager.Launcher.exe.config'), os.path.join(buildDir, 'ClusterKit.NodeManager.Launcher.exe.config'))
	shutil.copyfile(os.path.join('utils','launcher', 'start.sh'), os.path.join(buildDir, 'start.sh'))	
	pass


print "Building sources, please wait"
if platform.system() == 'Windows':
	#subprocess.call("cd .. && build.bat", shell = True)
	pass
else:
	#subprocess.call("cd .. && ./build.sh", shell = True)
	pass 


# building ClusterKitWorker
print "Preparing clusterkit/worker"
prepareLauncher(os.path.join('.','ClusterKitWorker'))
subprocess.call("docker build -t clusterkit/worker:latest ./ClusterKitWorker/", shell=True)

# building ClusterKitSeed
print "Preparing clusterkit/seed"
prepareLauncher(os.path.join('.','ClusterKitSeed'))
shutil.rmtree('./ClusterKitSeed/web', True);
os.mkdir('./ClusterKitSeed/web');
copyWebContent("../ClusterKit.Monitoring/ClusterKit.Monitoring.Web", "./ClusterKitSeed/web/monitoring")
subprocess.call("docker build -t clusterkit/seed:latest ./ClusterKitSeed/", shell=True)

# building ClusterKitSeed
print "Preparing clusterkit/manager"
subprocess.call("docker build -t clusterkit/manager:latest ./ClusterKitManager/", shell=True)