let fs = require("fs")
let processManager = require(pathFor.processManager)
let passwordManager = require(pathFor.passwordManager)

// import the official listener-names for this process
let listeners = processManager.processes.interface.listensFor
let yell = processManager.processes.interface.canYell


listeners.attemptLogin = function(newData) {
    console.log(newData)

    if (passwordManager.checkUsernameAndPassword({ username: newData.username, password: newData.password })) {
        yell.userAuthenticated({ username: newData.username })
    } else {
        yell.userAuthenticationFailed({ username: newData.username })
    }
}

