let processManager = require("../processManager")
let audioManager = require("../utils/audioManager")
let fs = require('fs')
let util = require('util')
let path = require('path')


// import the official listener-names for this process
let listeners = processManager.processes.systemData.listensFor
let yell = processManager.processes.systemData.canYell

let previousUpdateKey = global.systemData.updateKey;

const readdir = util.promisify(fs.readdir)
const stat = util.promisify(fs.stat)
async function allFilesRecursively(directoryName) {
    let files = await readdir(directoryName)
    let allFiles = []
    // recursively explore the sub directories
    for (let each of files) {
        let fullPath = path.join(directoryName, each)
        let fileStats = await stat(fullPath)
        allFiles.push(fullPath)
        // explore all sub directories
        if (fileStats.isDirectory()) {
            allFiles = allFiles.concat(await allFilesRecursively(fullPath))
        }
    }
    return allFiles
}

// continually update the gallery files
setTimeout(async () => {
    let galleryFiles = await allFilesRecursively(pathFor.gallery)
    let mapped = galleryFiles.map(each=>path.relative(process.cwd()+"/public",each))
    listeners.dataShouldChange({galleryFiles: mapped}, "galleryUpdater")
}, 1000)

// whenever dataShouldChange
listeners.dataShouldChange = (newData, who) => {
    let oldData = global.systemData
    console.log(`received dataChange request from :${who}`)
    global.systemData = {...global.systemData, ...newData}
    newData = global.systemData
    let dataAfterChange = JSON.stringify(global.systemData)
    // receiving an old message
    if (newData.updateKey == null) {
        console.log(`newData doesn't have an update key`)
    }
    if (oldData.updateKey != newData.updateKey) {
        console.log(`received out of date message: ${newData}`)
        console.log(`oldData is: ${global.systemData}`)
        global.systemData = oldData
        return
    }
    // if there was a change, tell everyone about it
    if (JSON.stringify(oldData) != dataAfterChange) {
        console.log(`oldData is:`,oldData)
        console.log(`newData is:`,newData)
        
        console.log("sending dataDidChange")
        // update the key to recognize when incoming requests get this
        global.systemData.updateKey = Math.random()
        yell.dataDidChange(global.systemData)
        
        // call armed/disarmed
        if (oldData.status == "disarmed" && newData.status == "armed") {
            audioManager.systemArmedSound.play()
        } else if (oldData.status == "armed" && newData.status == "disarmed") {
            audioManager.systemDisarmedSound.play()
        }
        
        // 
        // handle lightOn/lightOff
        // 
        if (oldData.kinectData.numberOfPeople == 0 && newData.kinectData.numberOfPeople > 0 && newData.status == "armed") {
            processManager.processes.strobeLight.listensFor.turnOn()
        }
        if (newData.kinectData.numberOfPeople == 0 || newData.status == "disarmed") {
            processManager.processes.strobeLight.listensFor.turnOff()
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

listeners.fullShutdown = () => {
    console.log("received request for fullShutdown (shutting down now)")
    process.exit(0)
}

module.exports = listeners