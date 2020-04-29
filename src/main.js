/* eslint-disable */
import Vue from "vue"
import App from "./App.vue"
import router from "./router"

import "./plugins/css-baseline"
import "./plugins/good-vue"
import "./plugins/keen-ui"
import "./plugins/vue-toasted"
import "./plugins/markdown"
import socket from "./plugins/socket-io"
import { Router } from "./plugins/vue-router"

// global components
import Markdown from "@/components/Markdown"

Vue.config.productionTip = false


// allow global access to root from non-vue files
window.Vue = Vue
new Vue({
    components: {
        Markdown,
    },
    router,
    data: () => ({
        systemData: {
            status: "unknown"
        },
        connectedToBackend: false,
        changesAreUnconfirmed: true,
        loggedIn: localStorage.getItem("loggedIn"),
        eventStream: "",
    }),
    mounted() {
        window.systemData = this.$data.systemData
        window.$root = this
        window.$toasted = this.$toasted

        // events to keep track of 
        window.socket.on("keypad.keyPressed", (whichKey)=>this.logEvent(`key pressed: ${whichKey}`))
        window.socket.on("kinect.foundPeople", (people)=>this.logEvent(`kinect found people: ${people.length}`))
        window.socket.on("kinect.lostSomePeople", ()=>this.logEvent(`kinect lost tracking for some people`))
        window.socket.on("kinect.lostEveryone", ()=>this.logEvent(`kinect lost tracking of everyone`))
    },
    watch: {
        systemData: {
            deep: true,
            handler(value, oldValue) {
                if (!window.receivingBackendData) {
                    console.log(`frontend systemData value is:`,value)
                    // everytime something (anything) changes any part of a system value, tell the backend about it
                    socket.emit('systemData.dataShouldChange', { status:  this.systemData.status })
                    this.changesAreUnconfirmed = true
                }
                window.receivingBackendData = false
            },
        }
    },
    methods: {
        logEvent(string) {
            let now = DateTime.now()
            this.$root.eventStream = `${now.time} ${now.date}: ${string}\n` + this.$root.eventStream
        },
    },
    render: h => h(App),
}).$mount("#app")
