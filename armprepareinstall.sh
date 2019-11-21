#!/bin/bash
ARCH=`dpkg --print-architecture`
ARMARCH="arm64"

if [ $ARCH = $ARMARCH ]; then
	echo "WE ARE ON ARM"
	curl -SL -o dotnet.tar.gz https://dotnetcli.blob.core.windows.net/dotnet/Sdk/master/dotnet-sdk-latest-linux-arm64.tar.gz
	sudo mkdir -p /usr/share/dotnet
	sudo tar -zxf dotnet.tar.gz -C /usr/share/dotnet
	sudo ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet
	echo $HOME
	echo $PATH
else
	wget -q https://packages.microsoft.com/config/ubuntu/19.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
	sudo dpkg -i packages-microsoft-prod.deb
	sudo apt-get update
	sudo apt-get install apt-transport-https
	sudo apt-get update
	sudo apt-get install -qq dotnet-sdk-3.0
	echo "WE ARE NOT ON ARM"
	echo $ARCH
fi
