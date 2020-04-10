<template>
    <column>
            <ui-button color=primary raised @click="toggleStrobe">Toggle Strobe</ui-button>
            <column width=40rem height=20rem overflow=auto>
                {{eventStream}}
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
        mounted() {
            window.socket.on("keypad.keyPressed", (whichKey)=> {
                
            })
        }
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