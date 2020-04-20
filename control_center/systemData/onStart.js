let processManager = require("../processManager")
let audioManager = require("../utils/audioManager")
let fs = require("fs")

// import the official listener-names for this process
let listeners = processManager.processes.systemData.listensFor
let yell = processManager.processes.systemData.canYell

// whenever dataShouldChange
listeners.dataShouldChange = (newData) => {
    let oldData = global.systemData
    console.log('received dataChange request')
    global.systemData = {...global.systemData, ...newData}
    console.log(`systemData is now:`,global.systemData)
    let dataAfterChange = JSON.stringify(global.systemData)
    // if there was a change, tell everyone about it
    if (JSON.stringify(oldData) != dataAfterChange) {
        console.log("sending dataDidChange")
        yell.dataDidChange(global.systemData)
        
        // call armed
        if (oldData.status == "disarmed" && newData.status == "armed") {
            audioManager.systemArmedSound.play()
        } else if (oldData.status == "armed" && newData.status == "disarmed") {
            audioManager.systemDisarmedSound.play()
        }

        // save changes to permanent storage
        fs.writeFile(global.pathFor.systemDataStorage, dataAfterChange, (...args)=>{
            console.log(`file write args is:`,args)
        })
    }
}

// whenever someone asks for systemData
listeners.requestSystemData = (newData) => {
    console.log("found a requestSystemData")
    yell.providingSystemData(global.systemData)
}