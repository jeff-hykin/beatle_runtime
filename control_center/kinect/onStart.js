let processManager = require("../processManager")
let setupProcess = require("../utils/commandlineHelper")

// import the official listener-names for this process
let listeners = processManager.processes.kinect.listensFor
let yell = processManager.processes.kinect.canYell

// FIXME: start the actual C# executable here
// "../Beatle_Defense_Kinect/bin/x64/Debug"

// 
// setup listener since C# doesn't use socket.io
// 
global.systemData.kinectData = { numberOfPeople: 0 }
let jsonParser = require('body-parser').json()
app.post("/sync", jsonParser, (req, res) => {
    let newData = req.body
    let oldData = global.systemData.kinectData
    // if there was a change
    if (JSON.stringify(oldData) != JSON.stringify(newData)) {
        
        // if there is a change in people
        if (oldData.numberOfPeople != newData.numberOfPeople) {

            if (newData.numberOfPeople == 0) {
                yell.lostEveryone()
            } else if (newData.numberOfPeople > oldData.numberOfPeople) {
                let placeHolderForPeopleObjects = ([...Array(req.body.numberOfPeople)]).map(each=>({})) // TODO: add information about each person
                yell.foundPeople(placeHolderForPeopleObjects)
            } else {
                yell.lostSomePeople()
            }
        }
        
        // update the data
        global.systemData.kinectData = req.body
    }
    // tell the kinect the info it needs to know (arm/disarm)
    res.send(global.systemData)
})
