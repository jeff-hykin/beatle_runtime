let processManager = require("../processManager")
let setupProcess = require("../utils/commandlineHelper")
let audioManager = require("../utils/audioManager")

// import the official listener-names for this process
let listeners = processManager.processes.keypad.listensFor
let yell = processManager.processes.keypad.canYell

let keypressQue = []

let systemIsBeingArmed = false
let stopSystemArmingProcess = false

// start the process
let theProcess = setupProcess(`python ${__dirname}/process.py`, (response)=>{
    response = response.trim()

    // 
    // setup key history (Que)
    // 
    keypressQue.push(response)
    // limit the que to 7 keypresses (star + 6 digits)
    if (keypressQue.length > 7) {
        keypressQue.shift()
    }

    // 
    // keypad * arming process
    // 
    if (response == "*" && global.systemData.status == "disarmed") {
        // if the system was being armed, then this will cancel it in the first 10 seconds
        if (systemIsBeingArmed == true) {
            stopSystemArmingProcess = true
            audioManager.systemDisarmedSound.play()
        // if the system wasn't armed, then it is now being armed (and is prepared to be cancelled if needed)
        } else {
            systemIsBeingArmed = true
            audioManager.systemArmed10Seconds.play()
            setTimeout(() => {
                if (!stopSystemArmingProcess) {
                    // tell system to update the data
                    processManager.processes.systemData.listensFor.dataShouldChange({ status: "armed" })
                }
                // always reset the variables
                stopSystemArmingProcess = false 
                systemIsBeingArmed = false
            }, 10000) // 10 seconds
        }
    }

    // TODO: disarming process

    yell.keyPressed(response.trim())
})