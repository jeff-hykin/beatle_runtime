<template>
    <column>
        <ui-button color=primary raised @click="toggleStrobe">Toggle Strobe</ui-button>
        <column width=40rem height=20rem max-width=80vw align-v=top align-h=left background-color=white padding="1rem 2rem" margin=1.5rem>
            <h5 style="text-decoration: underline;">Event Console</h5>
            <div style="white-space: pre; width: 100%; overflow: auto;">{{eventStream}}</div>
        </column>
    </column>
</template>

<script>
let DateTime = require("good-date")
export default {
    data: ()=>({
        lightIsOn: false,
        eventStream: "",
    }),
    mounted() {
        // events to keep track of 
        window.socket.on("keypad.keyPressed", (whichKey)=>this.logEvent(`key pressed: ${whichKey}`))
        window.socket.on("kinect.foundPeople", (people)=>this.logEvent(`kinect found people: ${people.length}`))
        window.socket.on("kinect.lostSomePeople", ()=>this.logEvent(`kinect lost tracking for some people`))
        window.socket.on("kinect.lostEveryone", ()=>this.logEvent(`kinect lost tracking of everyone`))
    },
    methods: {
        toggleStrobe() {
            this.lightIsOn = !this.lightIsOn
            if (this.lightIsOn) {
                window.socket.emit('strobeLight.turnOn', {})
            } else {
                window.socket.emit('strobeLight.turnOff', {})
            }
        },
        logEvent(string) {
            let now = DateTime.now()
            this.eventStream = `${now.time} ${now.date}: ${string}\n` + this.eventStream
        }
    }
}
</script>

<style>

</style>