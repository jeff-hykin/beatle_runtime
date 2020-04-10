// 
// Create the skeleton of the main interface
// 
// this skeleton calls all of the processes and tells them "hey, put your functions on me so I'm not a skeleton" and then they do that
// then the setup() function in here (below) connects everything to sockets
let mainInterface
module.exports = mainInterface = {
    // these processes need to match the names of their folder
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
            canYell: {}
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
                foundPeople: null, // returns: list of people (each with an id and maybe head location)
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
                    // all other processes will likely ignore this message
            }
        },
    },
    setupIo(io) {
        console.group(`setupIo()`)
        // 
        // generate all of the yell function bodies
        // 
        for (let eachProcessName in mainInterface.processes) {
            let eachProcess = mainInterface.processes[eachProcessName]
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
        for (let eachProcessName in mainInterface.processes) {
            console.group(`onStart.js: ${eachProcessName}`)
            // run each process file
            require(global.pathFor.processFolder+eachProcessName+"/onStart.js")
            console.groupEnd()
        }
        console.groupEnd()
    },
    setupNewSocket(socket) {
        console.group(`setupNewSocket()`)
        // 
        // connect all of the listeners
        // 
        for (let eachProcessName in mainInterface.processes) {
            let eachProcess = mainInterface.processes[eachProcessName]
            for (let eachListenerName in eachProcess.listensFor) {
                // attach the listener to the socket callback based on its name
                let eventName = `${eachProcessName}.${eachListenerName}`
                socket.on(eventName,  (...args)=>{
                    // use console grouping to improve debugging
                    console.group(eventName)
                    eachProcess.listensFor[eachListenerName](...args)
                    console.groupEnd()
                })
            }
        }
        console.groupEnd()
    }
}