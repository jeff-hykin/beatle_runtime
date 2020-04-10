<template>
    <column>
            <ui-button color=primary raised @click="toggleStrobe">Toggle Strobe</ui-button>
    </column>
</template>

<script>
export default {
    data: ()=>({
        lightIsOn: false
    }),
    mounted() {
        
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