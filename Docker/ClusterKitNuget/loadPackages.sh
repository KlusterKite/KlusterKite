mono nuget.exe setApiKey -Source http://localhost/ ChangeThisKey
for file in packages/*; do mono nuget.exe push $file -Source http://localhost/; done