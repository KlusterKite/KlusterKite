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
copyLib("ClusterKit.Core.Service", './ClusterKitDemoSeed/build')
copyLib("ClusterKit.Web.NginxConfigurator", './ClusterKitDemoSeed/build')
shutil.copyfile('./ClusterKitDemoSeed/akka.hocon', './ClusterKitDemoSeed/build/akka.hocon')
correctAssemblyVersions('./ClusterKitDemoSeed/build')
os.mkdir('./ClusterKitDemoSeed/web');
copyWebContent("../ClusterKit.Monitoring/ClusterKit.Monitoring.Web", "./ClusterKitDemoSeed/web/monitoring")
subprocess.call("docker build -t clusterkit/seed:latest ./ClusterKitDemoSeed/", shell=True)
#shutil.rmtree('./ClusterKitDemoSeed/build', True);
shutil.rmtree('./ClusterKitDemoSeed/web', True);


# building ClusterKitDemoWorker
print "Preparing clusterkit/worker"
shutil.rmtree('./ClusterKitDemoWorker/build', True);
copyLib("ClusterKit.Core.Service", './ClusterKitDemoWorker/build')
copyLib("ClusterKit.Web", './ClusterKitDemoWorker/build')
copyLib("ClusterKit.Monitoring", './ClusterKitDemoWorker/build')
shutil.copyfile('./ClusterKitDemoWorker/akka.hocon', './ClusterKitDemoWorker/build/akka.hocon')
correctAssemblyVersions('./ClusterKitDemoWorker/build')
subprocess.call("docker build -t clusterkit/worker:latest ./ClusterKitDemoWorker/", shell=True)
#shutil.rmtree('./ClusterKitDemoWorker/build', True);





