rem cd ../
rem call build.bat
rem cd Docker

cd ClusterKitDemoSeed
rmdir /S /Q build
mkdir build
xcopy ..\..\build\tmp\ClusterKit.Core.Service\*.dll build\ /Y
xcopy ..\..\build\tmp\ClusterKit.Core.Service\*.exe build\ /Y
xcopy ..\..\build\tmp\ClusterKit.Core.Service\*.config build\ /Y
xcopy ..\..\build\tmp\ClusterKit.Web.NginxConfigurator\*.dll build\ /Y
xcopy ..\..\build\tmp\ClusterKit.Web.NginxConfigurator\*.config build\ /Y
xcopy akka.hocon build\ /Y
docker build -t clusterkit/seed:latest .
cd ..

cd ClusterKitDemoWorker
rmdir /S /Q build
mkdir build
xcopy ..\..\build\tmp\ClusterKit.Core.Service\*.dll build\ /Y
xcopy ..\..\build\tmp\ClusterKit.Core.Service\*.exe build\ /Y
xcopy ..\..\build\tmp\ClusterKit.Core.Service\*.config build\ /Y
xcopy ..\..\build\tmp\ClusterKit.Monitoring \*.dll build\ /Y
xcopy ..\..\build\tmp\ClusterKit.Monitoring \*.config build\ /Y
xcopy akka.hocon build\ /Y
docker build -t clusterkit/worker:latest .
cd ..


