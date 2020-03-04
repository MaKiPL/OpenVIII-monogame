# wip ubuntu linux build script
# uncomment lines to install monodevelop 
#sudo apt --assume-yes install apt-transport-https dirmngr
#sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
#echo "deb https://download.mono-project.com/repo/ubuntu vs-bionic main" | sudo tee /etc/apt/sources.list.d/mono-official-vs.list
#sudo apt update
#sudo apt-get --assume-yes install monodevelop

# Install MonoGame depenencies
sudo apt update
sudo apt-get --assume-yes install nuget mono-complete mono-devel gtk-sharp3 ffmpeg
echo ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true | sudo debconf-set-selections
sudo apt-get --assume-yes install ttf-mscorefonts-installer
# MonoGame
wget https://github.com/MonoGame/MonoGame/releases/download/v3.7.1/monogame-sdk.run
# Extract contents and overwrite postinstall.sh to not prompt for input
# See: https://github.com/MonoGame/MonoGame/issues/5879
chmod +x monogame-sdk.run
sudo ./monogame-sdk.run --noexec --keep --target ./monogame
cd monogame
echo Y | sudo ./postinstall.sh
cd ..  
sudo apt --assume-yes install git
git clone https://github.com/makipl/openviii
cd openviii
nuget restore
msbuild OpenGLLinux  /property:Configuration=DebugLinux /property:Platform=x64