import platform
import subprocess
import shutil
import os
import re

def copyLib(libName, dest):
	if platform.system() == 'Windows':
		dest = dest.replace("/", "\\")
		subprocess.Popen("xcopy ..\\build\\tmp\\" + libName + "\\*.dll " + dest +"\\ /Y", shell=True, stdout=subprocess.PIPE).stdout.read()
		subprocess.Popen("xcopy ..\\build\\tmp\\" + libName + "\\*.exe " + dest +"\\ /Y", shell=True, stdout=subprocess.PIPE).stdout.read()
		subprocess.Popen("xcopy ..\\build\\tmp\\" + libName + "\\*.config " + dest +"\\ /Y", shell=True, stdout=subprocess.PIPE).stdout.read()
	else:
		subprocess.Popen("cp ../build/tmp/" + libName + "/*.dll " + dest + "/", shell=True, stdout=subprocess.PIPE).stdout.read()
		subprocess.Popen("cp ../build/tmp/" + libName + "/*.exe " + dest + "/" , shell=True, stdout=subprocess.PIPE).stdout.read()
		subprocess.Popen("cp ../build/tmp/" + libName + "/*.config " + dest + "/", shell=True, stdout=subprocess.PIPE).stdout.read()

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


print "Building sources, please wait"
if platform.system() == 'Windows':	
	subprocess.Popen("cd .. && build.bat", shell=True, stdout=subprocess.PIPE).stdout.read()
else:
	subprocess.Popen("cd .. && ./build.sh", shell=True, stdout=subprocess.PIPE).stdout.read()


# building ClusterKitDemoSeed
print "Preparing clusterkit/seed"
shutil.rmtree('./ClusterKitDemoSeed/build', True);
shutil.rmtree('./ClusterKitDemoSeed/web', True);
copyLib("ClusterKit.Core.Service", './ClusterKitDemoSeed/build')
copyLib("ClusterKit.Web.NginxConfigurator", './ClusterKitDemoSeed/build')
copyWebContent("../ClusterKit.Monitoring/ClusterKit.Monitoring.Web", "./ClusterKitDemoSeed/web")
print subprocess.Popen("docker build -t clusterkit/seed:latest ./ClusterKitDemoSeed/", shell=True, stdout=subprocess.PIPE).stdout.read()
shutil.rmtree('./ClusterKitDemoSeed/build', True);
shutil.rmtree('./ClusterKitDemoSeed/web', True);


# building ClusterKitDemoWorker
print "Preparing clusterkit/worker"
shutil.rmtree('./ClusterKitDemoWorker/build', True);
copyLib("ClusterKit.Core.Service", './ClusterKitDemoWorker/build')
copyLib("ClusterKit.Monitoring", './ClusterKitDemoWorker/build')
print subprocess.Popen("docker build -t clusterkit/worker:latest ./ClusterKitDemoWorker/", shell=True, stdout=subprocess.PIPE).stdout.read()
shutil.rmtree('./ClusterKitDemoWorker/build', True);





