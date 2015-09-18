#!/bin/bash
echo "Stopping"
killall mono

echo "Building"
xbuild /noconsolelogger /p:TargetFrameworkVersion="v4.5" ClusterKit.Core/ClusterKit.Core.sln

echo "Deploing"
rm -rf ~/ClusterKitDemo

mkdir -p ~/ClusterKitDemo/SeedNode1
cp  ClusterKit.Core/ClusterKit.Core.Service/bin/Debug/* ~/ClusterKitDemo/SeedNode1
sed -i 's/port = 0/port = 2551/g' ~/ClusterKitDemo/SeedNode1/akka.config
sed -i 's/seed-nodes = \[\]/seed-nodes = \["akka.tcp:\/\/ClusterKit@127.0.0.1:2551", "akka.tcp:\/\/ClusterKit@127.0.0.1:2552"\]/g' ~/ClusterKitDemo/SeedNode1/akka.config

mkdir -p ~/ClusterKitDemo/SeedNode2
cp  ClusterKit.Core/ClusterKit.Core.Service/bin/Debug/* ~/ClusterKitDemo/SeedNode2
sed -i 's/port = 0/port = 2552/g' ~/ClusterKitDemo/SeedNode2/akka.config
sed -i 's/seed-nodes = \[\]/seed-nodes = \["akka.tcp:\/\/ClusterKit@127.0.0.1:2551", "akka.tcp:\/\/ClusterKit@127.0.0.1:2552"\]/g' ~/ClusterKitDemo/SeedNode2/akka.config

mkdir -p ~/ClusterKitDemo/Node1
cp  ClusterKit.Core/ClusterKit.Core.Service/bin/Debug/* ~/ClusterKitDemo/Node1
sed -i 's/seed-nodes = \[\]/seed-nodes = \["akka.tcp:\/\/ClusterKit@127.0.0.1:2551", "akka.tcp:\/\/ClusterKit@127.0.0.1:2552"\]/g' ~/ClusterKitDemo/Node1/akka.config

