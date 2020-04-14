let processManager = require("../processManager")
let Interface = require(global.pathFor.systemDataStoragePath)
let fs = require("fs")

// import the official listener-names for this process
let listeners = processManager.processes.systemData.listensFor
let yell = processManager.processes.interface.canYell

listeners.attemptLogin=function(newData){console.log(newData)}

