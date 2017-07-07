# KlusterKite Docker demo application

You can start demo multinode application here

* buildBase.py - will prepare base docker images. This process is slow, but you need to do it just once (it will prepare two base images for future use)
* build.py - will build current sources and create (or update) fully functional application docker images with latest application version.

After you got all needed images just run `docker-compose up -d` from KlusterKiteDemoService folder
