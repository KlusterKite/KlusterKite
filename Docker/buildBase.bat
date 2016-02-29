cd ClusterKitBaseWorkerNode 
docker build -t clusterkit/baseworker:latest .
cd ..

cd ClusterKitBaseWebNode 
docker build -t clusterkit/baseweb:latest .
cd ..

