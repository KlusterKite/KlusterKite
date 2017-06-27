cd Docker\ClusterKit
docker-compose down
docker-compose up -d
docker-compose stop manager publisher1 publisher2 worker seeder
cd ../../
fake run -t FinalPushAllPackages build.fsx
cd Docker\ClusterKit
docker-compose start manager publisher1 publisher2 worker seeder
cd ../../

