#!/bin/bash

killall mono
mono-service -d:/home/kantora/TaxiKitDemo/SeedNode1/ -l:/tmp/SeedNode1.lck /home/kantora/TaxiKitDemo/SeedNode2/TaxiKit.Core.Service.exe
mono-service -d:/home/kantora/TaxiKitDemo/SeedNode1/ -l:/tmp/SeedNode1.lck /home/kantora/TaxiKitDemo/SeedNode2/TaxiKit.Core.Service.exe