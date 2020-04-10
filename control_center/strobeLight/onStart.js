let mainInterface = require("../interfaceManager")
let fs = require("fs")

// import the official listener-names for this process
let listeners = mainInterface.processes.strobeLight.listensFor
let yell = mainInterface.processes.strobeLight.canYell

// whenever dataShouldChange
listeners.turnOn = ()=> {
    console.log("attempting to turn off strobe")
}

// whenever someone asks for systemData
listeners.turnOff = () => {
    console.log("attempting to turn off strobe")
}

// let {controlCenter} = require("../interfaceManager")

// controlCenter.strobe.listenFor.turnOn

// const { exec } = require("child_process")
// const { spawn } = require('child_process')

// const filePath = "./PhidgetDemos-Py/test.py"

// const strobeCommand = spawn('python', [ filePath ])

// strobeCommand.stdout.on('data', (data) => {
//     console.log(`stdout: ${data}`)
// })

// strobeCommand.stderr.on('data', (data) => {
//     console.error(`stderr: ${data}`)
// })

// strobeCommand.on('close', (code) => {
//     console.log(`strobe process exited with code ${code}`)
// })
