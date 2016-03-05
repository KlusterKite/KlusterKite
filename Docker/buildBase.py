import platform
import subprocess

subprocess.call("cd ClusterKitBaseWorkerNode && docker build -t clusterkit/baseworker:latest . && cd ..", shell=True)
subprocess.call("cd ClusterKitBaseWebNode && docker build -t clusterkit/baseweb:latest . && cd ..", shell=True)
subprocess.call("cd ClusterKitBaseNugetNode && docker build -t clusterkit/basenuget:latest . && cd ..", shell=True)