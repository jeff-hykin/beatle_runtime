let processManager = require("../processManager")
let Interface = require(global.pathFor.systemDataStoragePath)
let fs = require("fs")

// import the official listener-names for this process
<<<<<<< HEAD
let listeners = processManager.processes.systemData.listensFor
let yell = processManager.processes.interface.canYell

listeners.attemptLogin=function(newData){console.log(newData)}
=======
let listeners = processManager.processes.interface.listensFor
let yell = processManager.processes.interface.canYell
>>>>>>> 3cc3b4b9e094e2a3f64634ecb8f61ae608e3bc2f

listeners.attemptLogin = function(newData) {
    console.log(newData)
}