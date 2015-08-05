!#/bin/bash
echo "Building"
xbuild /noconsolelogger /p:TargetFrameworkVersion="v4.5" TaxiKit.Core/TaxiKit.Core.sln

echo "Deploing"
rm -rf ~/TaxiKitDemo

mkdir -p ~/TaxiKitDemo/SeedNode1
cp  TaxiKit.Core/TaxiKit.Core.Service/bin/Debug/* ~/TaxiKitDemo/SeedNode1
sed -i 's/port = 0/port = 2551/g' ~/TaxiKitDemo/SeedNode1/akka.config
sed -i 's/seed-nodes = \[\]/seed-nodes = \["akka.tcp:\/\/TaxiKit@127.0.0.1:2551", "akka.tcp:\/\/TaxiKit@127.0.0.1:2552"\]/g' ~/TaxiKitDemo/SeedNode1/akka.config

mkdir -p ~/TaxiKitDemo/SeedNode2
cp  TaxiKit.Core/TaxiKit.Core.Service/bin/Debug/* ~/TaxiKitDemo/SeedNode2
sed -i 's/port = 0/port = 2552/g' ~/TaxiKitDemo/SeedNode2/akka.config
sed -i 's/seed-nodes = \[\]/seed-nodes = \["akka.tcp:\/\/TaxiKit@127.0.0.1:2551", "akka.tcp:\/\/TaxiKit@127.0.0.1:2552"\]/g' ~/TaxiKitDemo/SeedNode2/akka.config

mkdir -p ~/TaxiKitDemo/Node1
cp  TaxiKit.Core/TaxiKit.Core.Service/bin/Debug/* ~/TaxiKitDemo/Node1
sed -i 's/seed-nodes = \[\]/seed-nodes = \["akka.tcp:\/\/TaxiKit@127.0.0.1:2551", "akka.tcp:\/\/TaxiKit@127.0.0.1:2552"\]/g' ~/TaxiKitDemo/Node1/akka.config

