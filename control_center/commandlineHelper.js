const { spawn } = require("child_process")
let setupProcess = (commandString, eventListeners)=> {
    let [command, ...args ] = commandString.split(/\s+/)
    let aProcess = spawn(command, args)
    aProcess.stdout.on('data', (data) => {
        let action = `${data}`.trim()
        if (Object.keys(eventListeners).includes(action)) {
            eventListeners[action]
        } else {
            console.error(`unknown response from '${commandString}':\n\n${data}`)
        }
        console.log(`stdout: ${data}`)
    })
    aProcess.stderr.on('data', (data) => {
        console.error(`stderr from '${commandString}':\n\n ${data}`)
    })

    return {
        write(message) {
            aProcess.stdin.write(message+"\n")
        }
    }
}
module.exports = setupProcess