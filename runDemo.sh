#!/bin/bash

killall mono
mono-service -d:/home/kantora/ClusterKitDemo/SeedNode1/ -l:/tmp/SeedNode1.lck /home/kantora/ClusterKitDemo/SeedNode2/ClusterKit.Core.Service.exe
mono-service -d:/home/kantora/ClusterKitDemo/SeedNode1/ -l:/tmp/SeedNode1.lck /home/kantora/ClusterKitDemo/SeedNode2/ClusterKit.Core.Service.exe