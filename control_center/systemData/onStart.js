let mainInterface = require("../interfaceManager")
let systemData = require(global.pathFor.systemDataStoragePath)
let fs = require("fs")

// import the official listener-names for this process
let listeners = mainInterface.processes.systemData.listensFor
let yell = mainInterface.processes.systemData.canYell

// whenever dataShouldChange
listeners.dataShouldChange = (newData) => {
    let dataBeforeChange = JSON.stringify(systemData)
    console.log('received dataChange request')
    systemData = {...systemData, ...newData}
    let dataAfterChange = JSON.stringify(systemData)
    console.log(`systemData is now:`,systemData)
    // if there was a change, tell everyone about it
    if (dataBeforeChange != dataAfterChange) {
        console.log("sending dataDidChange")
        yell.dataDidChange(systemData)
        // save changes to permanent storage
        fs.writeFile(global.pathFor.systemDataStoragePath, dataAfterChange, (...args)=>{
            console.log(`file write args is:`,args)
        })
    }
}

// whenever someone asks for systemData
listeners.requestSystemData = (newData) => {
    console.log("found a requestSystemData")
    yell.providingSystemData(systemData)
}