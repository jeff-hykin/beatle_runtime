let processManager = require("../processManager")
let fs = require("fs")

// import the official listener-names for this process
let listeners = processManager.processes.systemData.listensFor
let yell = processManager.processes.systemData.canYell

// whenever dataShouldChange
listeners.dataShouldChange = (newData) => {
    let dataBeforeChange = JSON.stringify(global.systemData)
    console.log('received dataChange request')
    global.systemData = {...global.systemData, ...newData}
    let dataAfterChange = JSON.stringify(global.systemData)
    console.log(`systemData is now:`,global.systemData)
    // if there was a change, tell everyone about it
    if (dataBeforeChange != dataAfterChange) {
        console.log("sending dataDidChange")
        yell.dataDidChange(global.systemData)
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