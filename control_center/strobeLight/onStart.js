let processManager = require("../processManager")
let setupProcess = require("../utils/commandlineHelper")

// import the official listener-names for this process
let listeners = processManager.processes.strobeLight.listensFor
let yell = processManager.processes.strobeLight.canYell
// start the process
let strobeProcess = setupProcess(`python ${__dirname}/process.py`)

// whenever dataShouldChange
listeners.turnOn = ()=> {
    console.log("attempting to turn on strobe")
    global.systemData.strobeIsOn = true
    strobeProcess.write("on")
}

// whenever someone asks for systemData
listeners.turnOff = () => {
    console.log("attempting to turn off strobe")
    global.systemData.strobeIsOn = false
    strobeProcess.write("off")
}

module.exports = listeners