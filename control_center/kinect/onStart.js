const { spawn } = require('child_process')
let processManager = require("../processManager")
let setupProcess = require("../utils/commandlineHelper")

// import the official listener-names for this process
let listeners = processManager.processes.kinect.listensFor
let yell = processManager.processes.kinect.canYell

// start the C# executable
const subprocess = spawn("powershell", ["-command", `Start-Process "${pathFor.kinectExecutable.replace(/\//,"\\")}" -WindowStyle Hidden`], {
    stdio: 'ignore'
})

// 
// setup listener since C# doesn't use socket.io
// 
global.systemData.kinectData = { numberOfPeople: 0 }
let jsonParser = require('body-parser').json()
app.post("/sync", jsonParser, (req, res) => {
    let newData = req.body
    let oldData = global.systemData.kinectData
    console.log(`kinect: oldData is:`,oldData)
    console.log(`kinect: newData is:`,newData)
    // if there was a change
    if (JSON.stringify(oldData) != JSON.stringify(newData)) {
        
        // if there is a change in people
        if (oldData.numberOfPeople != newData.numberOfPeople) {
            // 
            // find info for logs
            // 
            if (newData.numberOfPeople == 0) {
                yell.lostEveryone()
            } else if (newData.numberOfPeople > oldData.numberOfPeople) {
                let placeHolderForPeopleObjects = ([...Array(req.body.numberOfPeople)]).map(each=>({})) // TODO: add information about each person
                yell.foundPeople(placeHolderForPeopleObjects)
            } else {
                yell.lostSomePeople()
            }
        }
        
        // tell system to update the data
        processManager.processes.systemData.listensFor.dataShouldChange({ kinectData: req.body }, "kinect")
    }
    // tell the kinect the info it needs to know (arm/disarm)
    res.send(global.systemData)
})
