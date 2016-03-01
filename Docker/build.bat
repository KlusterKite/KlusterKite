cd ../
call build.bat
cd Docker

cd ClusterKitDemoSeed
rmdir /S /Q build
mkdir build
rmdir /S /Q web
mkdir web
mkdir web\monitoring

xcopy ..\..\build\tmp\ClusterKit.Core.Service\*.dll build\ /Y
xcopy ..\..\build\tmp\ClusterKit.Core.Service\*.exe build\ /Y
xcopy ..\..\build\tmp\ClusterKit.Core.Service\*.config build\ /Y
xcopy ..\..\build\tmp\ClusterKit.Web.NginxConfigurator\*.dll build\ /Y
xcopy ..\..\build\tmp\ClusterKit.Web.NginxConfigurator\*.config build\ /Y
xcopy akka.hocon build\ /Y

xcopy ..\..\ClusterKit.Monitoring\ClusterKit.Monitoring.Web\*.html web\monitoring /Y
mkdir web\monitoring\Content
xcopy ..\..\ClusterKit.Monitoring\ClusterKit.Monitoring.Web\Content web\monitoring\Content\ /Y /E
mkdir web\monitoring\Scripts
xcopy ..\..\ClusterKit.Monitoring\ClusterKit.Monitoring.Web\Scripts web\monitoring\Scripts\ /Y /E


docker build -t clusterkit/seed:latest .
rmdir /S /Q build
rmdir /S /Q web

cd ..

cd ClusterKitDemoWorker
rmdir /S /Q build
mkdir build
xcopy ..\..\build\tmp\ClusterKit.Core.Service\*.dll build\ /Y
xcopy ..\..\build\tmp\ClusterKit.Core.Service\*.exe build\ /Y
xcopy ..\..\build\tmp\ClusterKit.Core.Service\*.config build\ /Y
xcopy ..\..\build\tmp\ClusterKit.Monitoring\*.dll build\ /Y
xcopy ..\..\build\tmp\ClusterKit.Monitoring\*.config build\ /Y
xcopy akka.hocon build\ /Y
docker build -t clusterkit/worker:latest .
rmdir /S /Q build
cd ..



