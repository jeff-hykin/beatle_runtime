<template>
    <div id="app">
        <column v-if="loading">
            Loading
        </column>
        <column v-if="!loading">
            <column v-if="!needsSetup">
                <router-view></router-view>
            </column>
            <column v-if="needsSetup">
                Lets get you setup
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


// paths
window.pathFor = require("../../pathFor")

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

// let path = require("path")
// let resolved = path.relative(__dirname, pathFor.passwordManager)
// console.log(`resolved is:`,resolved)

// utils
let fs = require('fs')
let passwordManager = customRequire(pathFor.passwordManager)

// 
// App
// 
let App = {
    name: 'App',
    components: { App },
    router: new Router({ routes }),
    data: () => ({
        systemData: {
            status: "unknown"
        },
        connectedToBackend: false,
        changesAreUnconfirmed: true,
        loading: true,
        needsSetup: null,
    }),
    mounted() {
        window.systemData = this.$data.systemData
        window.$root = this
        this.needsSetup = passwordManager.doesAtLeastOneUserExist()
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
        }
    }
}
// create and attach app
setTimeout(()=>(new (Vue.extend(App))).$mount('#app'),0)
export default App
</script>

<style lang='scss'>
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

</style>
