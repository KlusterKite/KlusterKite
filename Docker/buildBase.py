import platform
import subprocess

#print platform.system() == 'Windows'
print subprocess.Popen("cd ClusterKitBaseWorkerNode && docker build -t clusterkit/baseworker:latest . && cd ..", shell=True, stdout=subprocess.PIPE).stdout.read()
print subprocess.Popen("cd ClusterKitBaseWebNode && docker build -t clusterkit/baseworker:latest . && cd ..", shell=True, stdout=subprocess.PIPE).stdout.read()