let processManager = require("../processManager")
let setupProcess = require("../commandlineHelper")

// import the official listener-names for this process
let listeners = processManager.processes.motionSensor.listensFor
let yell = processManager.processes.motionSensor.canYell
// start the process
let theProcess = setupProcess(`python ${__dirname}/process.py`, (response)=>{
    if (response.trim() == "motionFound") {
        yell.motionFound()
    }
})