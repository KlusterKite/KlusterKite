#!/bin/bash
export NETWORK_NAME=`ifconfig  -a|grep -o -P "inet addr:[\d\.]*"| grep -oP "[\d\.]*" | grep -v 127.0.0.1 | head -n 1`
mono ClusterKit.NodeManager.Launcher.exe
