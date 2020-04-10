let app = require("express")()
let http = require("http").createServer(app)
let io = require("socket.io")(http)

// a single location for all javascript paths to prevent future breaking changes
global.pathFor = {
    package: __dirname+"/../package.json",
    processFolder: __dirname+"/",
    systemDataStoragePath: __dirname+"/systemData.json",
    homepage: __dirname + "/index.html",
    interfaceManager: __dirname+"/interfaceManager.js",
}

let mainInterface = require(global.pathFor.interfaceManager)
let packageJson = require(global.pathFor.package)

// 
// setup homepage (no main use, just for tests)
// 
app.get("/", (req, res) => {
    res.sendFile(global.pathFor.homepage)
})


// 
// setup socket connections (all data transfer)
// 
mainInterface.setupIo(io)
io.on("connection",  mainInterface.setupNewSocket)

// 
// start the server 
// 
http.listen(packageJson.centralServerPort,  () => {
    console.log(`\nlistening on *:${packageJson.centralServerPort}`)
})