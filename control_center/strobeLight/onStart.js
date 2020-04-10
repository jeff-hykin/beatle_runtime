let fs = require("fs")
let mainInterface = require("../interfaceManager")
let setupProcess = require("../commandlineHelper")

// import the official listener-names for this process
let listeners = mainInterface.processes.strobeLight.listensFor
let yell = mainInterface.processes.strobeLight.canYell
// start the process
let strobeProcess = setupProcess(`python ${__dirname}/process.py`, {})

// whenever dataShouldChange
listeners.turnOn = ()=> {
    console.log("attempting to turn on strobe")
    strobeProcess.write("on")
}

// whenever someone asks for systemData
listeners.turnOff = () => {
    console.log("attempting to turn off strobe")
    strobeProcess.write("off")
}