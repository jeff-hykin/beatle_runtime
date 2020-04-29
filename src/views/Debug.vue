<template>
    <column>
        <ui-button :disable='$root.systemData.status=="disarmed"' color=primary raised @click="toggleStrobe">
            Toggle Strobe
        </ui-button>
        <column height=2rem>
            
        </column>
        <ui-button color=primary raised @click="causeFullShutdown">
            Shutdown All processes
        </ui-button>
        <column width=40rem height=20rem max-width=80vw align-v=top align-h=left background-color=white padding="1rem 2rem" margin=1.5rem>
            <h5 style="text-decoration: underline;">Event Console</h5>
            <div style="white-space: pre; width: 100%; overflow: auto;">{{$root.eventStream}}</div>
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
        
    },
    methods: {
        toggleStrobe() {
            if (!$root.systemData.strobeIsOn) {
                window.socket.emit('strobeLight.turnOn', {})
            } else {
                window.socket.emit('strobeLight.turnOff', {})
            }
        },
        logEvent(string) {
            let now = DateTime.now()
            this.$root.eventStream = `${now.time} ${now.date}: ${string}\n` + this.$root.eventStream
        },
        causeFullShutdown() {
            window.socket.emit('systemData.fullShutdown', {})
        }
    }
}
</script>

<style>

</style>