let processManager = require("../processManager")
let systemData = require(global.pathFor.systemDataStoragePath)
let fs = require("fs")

// import the official listener-names for this process
let listeners = processManager.processes.systemData.listensFor
let yell = processManager.processes.systemData.canYell