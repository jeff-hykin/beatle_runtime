let processManager = require("../processManager")
let setupProcess = require("../utils/commandlineHelper")

// import the official listener-names for this process
let listeners = processManager.processes.keypad.listensFor
let yell = processManager.processes.keypad.canYell
// start the process
let theProcess = setupProcess(`python ${__dirname}/process.py`, (response)=>{
    yell.keyPressed(response.trim())
})