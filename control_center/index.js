let app = require("express")()
let http = require("http").createServer(app)
let io = require("socket.io")(http)
let fs = require("fs")
const systemDataStoragePath = __dirname + "/systemData.json"

app.get("/", (req, res) => {
    res.sendFile(__dirname + "/index.html")
})

let systemData = require(systemDataStoragePath)
io.on("connection",  (socket) => {
    
    // regular data changes
    socket.on("dataShouldChange",  (newData) => {
        let dataBeforeChange = JSON.stringify(systemData)
        console.log('received dataChange request')
        systemData = {...systemData, ...newData}
        let dataAfterChange = JSON.stringify(systemData)
        console.log(`systemData is now:`,systemData)
        // if there was a change, tell everyone about it
        if (dataBeforeChange != dataAfterChange) {
            console.log("sending dataDidChange")
            io.emit('dataDidChange', systemData)
            // save changes to permanent storage
            fs.writeFile(systemDataStoragePath, dataAfterChange, (...args)=>{
                console.log(`file write args is:`,args)
            })
        }
    })
    
    // refreshing data
    socket.on("requestSystemData",  (newData) => {
        console.log("found a requestSystemData")
        io.emit('providingSystemData', systemData)
    })
    
})

http.listen(3000,  () => {
    console.log("listening on *:3000")
})