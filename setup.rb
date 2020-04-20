require "atk_toolbox"

if OS.is?("windows")
    system("choco install -y python3")
    system("choco install -y npm nodejs")
    # install all the node packages
    system("npm install")
    # install the pip modules
    system("pip install phidgit22")
    # notes
    puts "You'll need to install the Phigit Drivers manually from their website: https://www.phidgets.com/"
    puts "You'll need to install old Motor Drivers manually (TODO: add guide)"
    puts "You'll need to install visual studio, the kinect SDK, and probably some nuget modules"
    puts "You'll need to setup the app to start with system startup: https://www.windowscentral.com/how-launch-apps-automatically-during-startup-windows-10"
end


