let processManager = require("../processManager")
let Interface = require(global.pathFor.systemDataStorage)
let fs = require("fs")

// import the official listener-names for this process
let listeners = processManager.processes.interface.listensFor
let yell = processManager.processes.interface.canYell


listeners.attemptLogin = function(newData) {
    console.log(newData)

    if (newData.username == "user1" && newData.password == "password1") {
        yell.userAuthenticated({ username:"user1" })
    }
}

