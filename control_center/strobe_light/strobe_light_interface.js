const { exec } = require("child_process")
const { spawn } = require('child_process')

const filePath = "./PhidgetDemos-Py/test.py"

const strobeCommand = spawn('python', [ filePath ])

strobeCommand.stdout.on('data', (data) => {
    console.log(`stdout: ${data}`)
})

strobeCommand.stderr.on('data', (data) => {
    console.error(`stderr: ${data}`)
})

strobeCommand.on('close', (code) => {
    console.log(`strobe process exited with code ${code}`)
})
