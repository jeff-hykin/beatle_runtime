const { spawn } = require("child_process")
let setupProcess = (commandString, listener=()=>0)=> {
    let [command, ...args ] = commandString.split(/\s+/)
    let aProcess = spawn(command, args)
    aProcess.stdout.on('data', (data) => {
        listener(`${data}`)
        console.log(`stdout from '${commandString}': ${data}`)
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