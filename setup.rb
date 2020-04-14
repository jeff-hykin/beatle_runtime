require "atk_toolbox"

if OS.is?("windows")
    system("choco install -y python3")
    system("choco install -y npm nodejs")
    # install all the node packages
    system("npm install")
    # install the pip modules
    system("pip install python-socketio")
    system("pip install requests")
    # notes
    puts "You'll need to install the Phigit Drivers manually from their website"
    puts "You'll need to install old Motor Drivers manually (TODO: add guide)"
end


