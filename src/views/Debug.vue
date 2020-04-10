<template>
    <column>
            <ui-button raised>Toggle Light</ui-button>
    </column>
</template>

<script>
const { spawn } = require('child_process')

function createProcess() {
    const ls = spawn('ls', ['-lh', '/usr'])

    ls.stdout.on('data', (data) => {
        console.log(`stdout: ${data}`)
    });

    ls.stderr.on('data', (data) => {
        console.error(`stderr: ${data}`)
    })

    ls.on('close', (code) => {
        console.log(`child process exited with code ${code}`)
    })
}



export default {
    data: ()=>({
        lightIsOn: false
    }),
    mounted() {
        
    },
    methods: {
        toggleLight() {
            this.lightIsOn = !this.lightIsOn
        }
    }
}
</script>

<style>

</style>