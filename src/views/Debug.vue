<template>
    <column>
            <ui-button color=primary raised @click="toggleStrobe">Toggle Strobe</ui-button>
            <column width=40rem height=20rem align-v=top align-h=left background-color=white padding="1rem 2rem" margin=1.5rem>
                <h5 style="text-decoration: underline;">Event Console</h5>
                <div style="white-space: pre; width: 100%; overflow: auto;">
                    {{eventStream}}
                </div>
            </column>
    </column>
</template>

<script>
export default {
    data: ()=>({
        lightIsOn: false,
        eventStream: "",
    }),
    mounted() {
        window.socket.on("keypad.keyPressed", (whichKey)=> {
            this.eventStream += `\nkey pressed: ${whichKey}`
        })
    },
    methods: {
        toggleStrobe() {
            this.lightIsOn = !this.lightIsOn
            if (this.lightIsOn) {
                window.socket.emit('strobeLight.turnOn', {})
            } else {
                window.socket.emit('strobeLight.turnOff', {})
            }
        }
    }
}
</script>

<style>

</style>