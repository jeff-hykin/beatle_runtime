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
        } else {
            systemIsBeingArmed = true
            audioManager.systemArmed10Seconds.play()
            setTimeout(() => {
                if (!stopSystemArmingProcess) {
                    global.systemData.status = "armed"
                    // tell everyone the systemData changed
                    processManager.processes.systemData.canYell.dataDidChange(global.systemData)
                    // say the system is armed
                    audioManager.systemArmedSound.play()
                }
                // always reset the variables
                stopSystemArmingProcess = false 
                systemIsBeingArmed = false
            }, 10000)
        }
    }

    // TODO: disarming process

    yell.keyPressed(response.trim())
})