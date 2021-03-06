let app = require("express")()
let http = require("http").createServer(app)
let io = require("socket.io")(http)
let cors = require('cors')
let fs = require('fs')

// a single location for all javascript paths to prevent future breaking changes
global.pathFor = require("../pathFor")
global.app = app

let processManager = require(global.pathFor.processManager)
let packageJson = require(global.pathFor.package)

// 
// load up system data
// 
global.systemData = {
    updateKey: Math.random(),
    status:"disarmed",
    kinectData: {
        numberOfPeople:0,
        people: {},
    },
    strobeIsOn: false,
    galleryFiles: [],
}
try {
    global.systemData = { ...global.systemData, ...require(global.pathFor.systemDataStorage) }
} catch (error) {
    
}
if (!(global.systemData.kinectData instanceof Object && global.systemData.kinectData.people instanceof Object)) {
    global.systemData.kinectData.people = {}
}
fs.writeFile(global.pathFor.systemDataStorage, JSON.stringify(global.systemData), (...args)=>{
    console.log(`file write args is:`,args)
})


//
// setup routes
//
app.use(cors())
app.get("/", (req, res) => {
    res.sendFile(global.pathFor.homepage)
})

//
// setup socket connections (all data transfer)
//
processManager.setupIo(io)
io.on("connection", processManager.setupNewSocket)

//
// start the central server
//
http.listen(packageJson.centralServerPort, () => {
    console.log(`\nsocket is listening on *:${packageJson.centralServerPort}`)
})

//
// start the client interface
//
const { exec, spawn } = require("child_process")
// go to root folder
process.chdir(__dirname+"/..")
exec("npx vue-cli-service serve", (err, stdout, stderr) => {
    if (err) {
        console.error(err)
        return
    }
    console.log(stdout)
    console.log(stderr)
})
