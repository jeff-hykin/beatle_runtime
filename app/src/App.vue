<template>
    <div id="app">
        <!-- loader -->
        <column v-if='loading' height=100vh width=100vw position=fixed>
            <ui-progress-circular color="primary" />
        </column>
        <!-- main content -->
        <column v-if="!loading">
            <column v-if="!needsSetup">
                <transition name="fade" mode="out-in" >
                    <router-view></router-view>
                </transition>
                <ui-confirm
                    ref="confirmationBox"
                    :title="confirmationBox.title"

                    @confirm="confirmationBox.action"
                    >
                    {{confirmationBox.message}}
                </ui-confirm>
            </column>
            <column v-if="needsSetup"  >
                <transition name="fade" mode="out-in" >
                    <setup />
                </transition>
            </column>
        </column>
    </div>
</template>

<script>
// 
// Setup Vue
// 
import Vue from 'vue'
if (!process.env.IS_WEB) Vue.use(require('vue-electron'))
Vue.config.productionTip = false
Vue.config.devtools = process.env.NODE_ENV === 'development'

// make http requests work
import axios from 'axios'
Vue.http = Vue.prototype.$http = axios

// in renderer process
import { webFrame } from 'electron'
// allow zooming in with Mac Trackpad
webFrame.setVisualZoomLevelLimits(1, 3)


// utils 
const { lookup } = require('dns').promises
const { hostname } = require('os')

// paths
window.pathFor = require("../../pathFor")
window.passwordManager = customRequire(pathFor.passwordManager)

// 
// Plugins
// 
import './plugins/css-baseline'
import './plugins/good-vue'
import './plugins/keen-ui'
import './plugins/vue-toasted'
import './plugins/window-listener'
import './plugins/custom-require'
import './plugins/simple-functional-components'
import socket from "./plugins/socket-io"
import { Router } from './plugins/vue-router'

// routes
import routes from './routes'

// components
import setup from './components/setup'

// 
// App
// 
let App = {
    name: 'App',
    components: { App, setup },
    router: new Router({ routes }),
    data: () => ({
        systemData: {
            status: "unknown"
        },
        connectedToBackend: false,
        changesAreUnconfirmed: true,
        loading: true,
        needsSetup: null,
        localIpAddress: null,
        confirmationBox: {},
    }),
    async mounted() {
        window.systemData = this.$data.systemData
        window.$root = this
        this.needsSetup = !passwordManager.doesAtLeastOneUserExist()
        this.localIpAddress = (await lookup(hostname(), {})).address
        // finished loading
        this.loading = false
    },
    watch: {
        systemData: {
            deep: true,
            handler(value, oldValue) {
                console.log(`frontend systemData value is:`,value)
                // everytime something (anything) changes any part of a system value, tell the backend about it
                socket.emit('systemData.dataShouldChange', this.systemData)
                this.changesAreUnconfirmed = true
            },
        },
    },
    methods: {
        confirmDialogue({title, action, message}) {
            // set everything
            this.confirmationBox = { title, action, message }
            // then display
            this.$refs.confirmationBox.open()
        }
    }
}
// create and attach app
setTimeout(()=>(new (Vue.extend(App))).$mount('#app'),0)
export default App
</script>

<style lang='scss'>

.ui-button {
    cursor: pointer;
}

:root {
    --blue: #007bff;
    --indigo: #6610f2;
    --purple: #6f42c1;
    --pink: #e83e8c;
    --red: #dc3545;
    --orange: #fd7e14;
    --yellow: #ffc107;
    --green: #28a745;
    --teal: #20c997;
    --cyan: #64ffda;
    --white: #fff;
    --gray: #6c757d;
    --gray-dark: #343a40;
    --primary: #007bff;
    --secondary: #6c757d;
    --success: #28a745;
    --info: #17a2b8;
    --warning: #ffc107;
    --danger: #dc3545;
    --light: #f8f9fa;
    --dark: #343a40;
}

/* fallback */
@font-face {
  font-family: 'Material Icons';
  font-style: normal;
  font-weight: 400;
  src: url(https://fonts.gstatic.com/s/materialicons/v48/flUhRq6tzZclQEJ-Vdg-IuiaDsNcIhQ8tQ.woff2) format('woff2');
}

.material-icons {
  font-family: 'Material Icons';
  font-weight: normal;
  font-style: normal;
  font-size: 24px;
  line-height: 1;
  letter-spacing: normal;
  text-transform: none;
  display: inline-block;
  white-space: nowrap;
  word-wrap: normal;
  direction: ltr;
  -webkit-font-feature-settings: 'liga';
  -webkit-font-smoothing: antialiased;
}

.fade-enter-active,
.fade-leave-active {
    transition-duration: 0.3s;
    transition-property: opacity;
    transition-timing-function: ease;
}

.fade-enter,
.fade-leave-active {
    opacity: 0
}

</style>
