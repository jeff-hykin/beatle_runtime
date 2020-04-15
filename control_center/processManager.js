// 
// Create the skeleton of the processManager
// 
// this skeleton calls all of the processes and tells them "hey, put your functions on me so I'm not a skeleton" and then they do that
// then the setup() function in here (below) connects everything to sockets
let processManager
module.exports = processManager = {
    // these processes need to match the names of their folders
    processes:  {
        strobeLight: {
            listensFor: {
                turnOn: null, // no arguments
                turnOff: null, // no arguments
            },
            canYell: {},
        },
        keypad: {
            listensFor: {},
            canYell: {
                keyPressed: null, // returns: which key as a string
            }
        },
        motionSensor: {
            listensFor: {},
            canYell: {
                motionFound: null,
            }
        },
        kinect: {
            listensFor: {
                wakeUp: null, // no arguments
            },
            canYell: {
                foundPeople: null, // returns: list of people-objects (each with an id and maybe head location)
            }
        },
        systemData: {
            listensFor: {
                dataShouldChange: null,
                    // first (only) argument:
                    // a JSON object that includes at least 1 key=>value pair
                    // The value of the provided key/keys will be updated for everyone, and saved to a file
                    // After updating that value, the systemData process will immediatly yell in 
                    // order to give all other processes the most up-to-date information.
            
                requestSystemData: null, // no arguments
                    // this command is used when a process comes online and doesn't know
                    // what is going on. It asks the system for the most up-to-date information
            },
            canYell: {
                dataDidChange: null, // returns: JSON representing all system data
                    // this is only sent if data legitmately changed.
                    // If a change was requested (that didn't actually change anything)
                    // then this will not be yelled

                providingSystemData: null, // returns: JSON object representing all system data
                    // this is a response to a process asking for information
                    // all other processes will like    ly ignore this message
            }
        },
        
        interface: {
            listensFor: {
                attemptLogin: null,
            },
            canYell: {
                userAuthenticated: null,
                userAuthenticationFailed: null,
            },
        },
    },
    setupIo(io) {
        console.group(`\nsetupIo()`)
        // 
        // generate all of the yell function bodies
        // 
        for (let eachProcessName in processManager.processes) {
            let eachProcess = processManager.processes[eachProcessName]
            for (let eachYellableThing in eachProcess.canYell) {
                // turn it into a function
                eachProcess.canYell[eachYellableThing] = (returnValue)=>{
                    io.emit(`${eachProcessName}.${eachYellableThing}`, returnValue)
                }
            }
        }

        // 
        // start all of the processes
        // 
        for (let eachProcessName in processManager.processes) {
            console.group(`\nonStart.js: ${eachProcessName}`)
            // run each process file
            require(global.pathFor.processFolder+eachProcessName+"/onStart.js")
            console.groupEnd()
        }
        console.groupEnd()
    },
    setupNewSocket(socket) {
        console.group(`\nsetupNewSocket()`)
        // 
        // connect all of the listeners
        // 
        for (let eachProcessName in processManager.processes) {
            let eachProcess = processManager.processes[eachProcessName]
            for (let eachListenerName in eachProcess.listensFor) {
                // attach the listener to the socket callback based on its name
                let eventName = `${eachProcessName}.${eachListenerName}`
                socket.on(eventName,  (...args)=>{
                    // use console grouping to improve debugging
                    console.group("\n"+eventName)
                    try {
                        eachProcess.listensFor[eachListenerName](...args)
                    } catch (error) {
                        console.warn(`warning, failed to run ${eachListenerName}\nargs:${args}\n\nerror: ${error}`)
                    }
                    console.groupEnd()
                })
            }
        }
        console.groupEnd()
    }
}