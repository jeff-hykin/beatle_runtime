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
        changesAreUnconfirmed: true
    }),
    mounted() {
        window.systemData = this.$data.systemData
        window.$root = this
    },
    watch: {
        systemData: {
            deep: true,
            handler(value, oldValue) {
                console.log(`frontend systemData value is:`,value)
                // everytime something (anything) changes any part of a system value, tell the backend about it
                socket.emit('dataShouldChange', this.systemData)
                this.changesAreUnconfirmed = true
            },
        }
    },
    render: h => h(App),
}).$mount("#app")
