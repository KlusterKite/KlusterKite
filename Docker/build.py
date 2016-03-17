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


print "Building sources, please wait"
if platform.system() == 'Windows':
	subprocess.call("cd .. && build.bat", shell = True)
	pass
else:
	subprocess.call("cd .. && ./build.sh", shell = True)
	pass 


# building ClusterKitDemoSeed
print "Preparing clusterkit/seed"
shutil.rmtree('./ClusterKitDemoSeed/build', True);
shutil.rmtree('./ClusterKitDemoSeed/web', True);
os.mkdir('./ClusterKitDemoSeed/build');
os.mkdir('./ClusterKitDemoSeed/web');
copyLib("ClusterKit.Core.Service", './ClusterKitDemoSeed/build')
copyLib("ClusterKit.Web.Swagger", './ClusterKitDemoSeed/build')
copyLib("ClusterKit.Web.Swagger.Monitor", './ClusterKitDemoSeed/build')
copyLib("ClusterKit.Web.NginxConfigurator", './ClusterKitDemoSeed/build')
copyLib("ClusterKit.NodeManager.Client", './ClusterKitDemoSeed/build')
shutil.copyfile('./ClusterKitDemoSeed/akka.hocon', './ClusterKitDemoSeed/build/akka.hocon')
correctAssemblyVersions('./ClusterKitDemoSeed/build')

copyWebContent("../ClusterKit.Monitoring/ClusterKit.Monitoring.Web", "./ClusterKitDemoSeed/web/monitoring")
subprocess.call("docker build -t clusterkit/seed:latest ./ClusterKitDemoSeed/", shell=True)
#shutil.rmtree('./ClusterKitDemoSeed/build', True);
shutil.rmtree('./ClusterKitDemoSeed/web', True);


# building ClusterKitDemoWorker
print "Preparing clusterkit/demoworker"
shutil.rmtree('./ClusterKitDemoWorker/build', True);
os.mkdir('./ClusterKitDemoWorker/build');
copyLib("ClusterKit.Core.Service", './ClusterKitDemoWorker/build')
copyLib("ClusterKit.Web", './ClusterKitDemoWorker/build')
copyLib("ClusterKit.Web.Swagger", './ClusterKitDemoWorker/build')
copyLib("ClusterKit.Monitoring", './ClusterKitDemoWorker/build')
copyLib("ClusterKit.Core.EF.Npgsql", './ClusterKitDemoWorker/build')
copyLib("ClusterKit.NodeManager", './ClusterKitDemoWorker/build')

shutil.copyfile('./ClusterKitDemoWorker/akka.hocon', './ClusterKitDemoWorker/build/akka.hocon')
correctAssemblyVersions('./ClusterKitDemoWorker/build')
subprocess.call("docker build -t clusterkit/demoworker:latest ./ClusterKitDemoWorker/", shell=True)
#shutil.rmtree('./ClusterKitDemoWorker/build', True);

# building ClusterKitWorker
print "Preparing clusterkit/worker"
shutil.rmtree('./ClusterKitWorker/build', True);
shutil.rmtree('./ClusterKitWorker/preinstalled', True);
shutil.rmtree('./ClusterKitWorker/packageCache', True);
os.mkdir('./ClusterKitWorker/build');
os.mkdir('./ClusterKitWorker/preinstalled');
os.mkdir('./ClusterKitWorker/packageCache');

for dir in os.listdir("../packages"):
	if (dir.startswith("System.")):
		os.mkdir('./ClusterKitWorker/preinstalled/' + dir + "/");		
		sysCopy("../packages/" + dir , './ClusterKitWorker/preinstalled/' + dir + "/")

packageDirs = os.listdir("../packages")
for dir in packageDirs:
	if (os.path.isdir(os.path.join("../packages",dir))):
		for file in os.listdir(os.path.join("../packages",dir)):
			if (file.endswith(".nupkg") and not os.path.isfile(os.path.join("..","packageOut", file))):
				shutil.copyfile(os.path.join("../packages",dir, file), os.path.join(".","ClusterKitWorker", "packageCache", file))		

copyLib("ClusterKit.NodeManager.Launcher", './ClusterKitWorker/build')
shutil.copyfile('./ClusterKitWorker/ClusterKit.NodeManager.Launcher.exe.config', './ClusterKitWorker/build/ClusterKit.NodeManager.Launcher.exe.config')
subprocess.call("docker build -t clusterkit/worker:latest ./ClusterKitWorker/", shell=True)
