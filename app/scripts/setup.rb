require 'atk_toolbox'

puts "I'll try to install most of these things for you"
puts "to test you'll need:"
puts "    nodejs ≥ v13.7.0"
puts "    npm ≥ 6.13.6"
puts ""
puts "to export into an app/program file you'll need:"
puts "    python2 (needed by electron builder)"
puts "    you'll also need python2 mapped to the 'python' command when building"
puts ""
puts "    if you have asdf installed"
puts "    and have installed a version of python2 with asdf"
puts "    then there's no need to change the global version of python to python2"


puts "\nattempting to install node and npm"
if not Console.has_command("npm")
    if OS.is?("mac")
        system "brew install node"
    elsif OS.is?("windows")
        system "scoop install nodejs"
    elsif OS.is?("ubuntu")
        system "sudo apt install nodejs"
    end
end

system "npm install"