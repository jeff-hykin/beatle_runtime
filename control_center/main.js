let app = require("express")()
let http = require("http").createServer(app)
let io = require("socket.io")(http)
let bodyParser = require('body-parser')
let cors = require('cors')

// create application/json parser
let jsonParser = bodyParser.json()

// a single location for all javascript paths to prevent future breaking changes
global.pathFor = require("../pathFor")

let processManager = require(global.pathFor.processManager)
let packageJson = require(global.pathFor.package)
global.systemData = require(global.pathFor.systemDataStorage)

//
// setup routes
//
app.use(cors())
app.get("/", (req, res) => {
    res.sendFile(global.pathFor.homepage)
})
app.post("/sync", jsonParser, (req, res) => {
    console.log(`req.body is:`,req.body)
    res.send(systemData)
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
exec("npx vue-cli-service serve", (err, stdout, stderr) => {
    if (err) {
        console.error(err)
        return
    }
    console.log(stdout)
    console.log(stderr)
})
