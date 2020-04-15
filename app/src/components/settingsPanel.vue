<template>
    <column align-h="center" align-v="top" :class="['settings-panel', { init }]" shadow="2">
        <!-- the 90° rotated title -->
        <div class=settings-panel-name>Settings Panel</div>
        
        <!-- Show Help -->
        <row>
            <ui-button size=large style='background-color: var(--gray); color: white' @click="showHelp('modal1')">
                Show Help
            </ui-button>
        </row>
        <ui-modal ref="modal1" title="Help / Show Controls" transition="scale-up">
            <helpMenu />
        </ui-modal>
        
        
        <!-- General Settings -->
        <br /><br />
        <h5>General Settings</h5>
        <column class="settings-bubble bubble" shadow="1" align-h="left">
            <ui-textbox label="Pick a Number" v-model="settings.numberSetting" />
            <br />
        </column>
        
    </column>
</template>

<script>
import helpMenu from '@/components/helpMenu'
import ytdl from 'ytdl-core'
import fs, { write } from "fs"
import path from 'path'
import { remote } from "electron"

let   util    = require("util")
const readdir = util.promisify(fs.readdir)
let   dialog  = remote.dialog
let   app     = remote.app

let localSettingsLocation = "videoLabelerSettings"

export let settingsPanelComponent = {}
export default {
    name: "settingsPanel",
    components: { helpMenu },
    beforeCreate() {
        settingsPanelComponent = this
    },
    data: () => ({
        settings: {
            numberSetting: 1,
        },
        init: true,
    }),
    mounted() {
        // have an initial value that gets turned to false (for css classes)
        setTimeout(_=>this.init = false, 1300)
        this.loadSettings()
    },
    watch: {
        settings: {
            deep: true,
            handler(val) {
                this.saveSettings()
            },
        },
    },
    methods: {
        showHelp(ref) {
            this.$refs[ref].open()
            window.document.body.appendChild(this.$refs[ref].$el)
        },
        saveSettings() {
            window.localStorage.setItem(localSettingsLocation, JSON.stringify(this.settings))
        },
        loadSettings() {
            let settings = window.localStorage.getItem(localSettingsLocation)
            if (typeof settings == "string") {
                let savedSettings = JSON.parse(settings)
                this.settings = { ...this.settings, ...savedSettings }
            }
        },
    },
}
</script>

<style lang="scss" scoped>
.settings-panel {
    --panel-width: 22rem;
    position: fixed;
    min-width: var(--panel-width);
    transform: translateX(calc(-100% + var(--unhovered-panel-amount) + 3px));
    transition: all 500ms ease-out, height 1ms;
    background-color: whitesmoke;
    height: 100vh;
    overflow: auto;
    left: 0;
    z-index: 11;
    padding: 2rem 3rem;
    padding-right: calc(var(--unhovered-panel-amount) + 1rem);
    
    --panel-name-height: 14pt;
    
    &:hover {
        transform: translateX(0);
        padding: 2rem 2rem;
        
        .settings-panel-name {
            margin-right: calc(var(--panel-name-height) * -4);
        }
    }
    
    .settings-panel-name {
        position: absolute;
        right: 0;
        transition: all 500ms ease-out;
        margin-right: 0;
        transform: translateX(calc(0.5 * calc(var(--unhovered-panel-amount) - var(--panel-name-height)))) rotate(90deg) translateX(50%);
        font-size: var(--panel-name-height);
        color: var(--gray);
    }

    .bubble {
        width: 100%;
        margin-top: 0.8rem;
        padding: 1.5rem;
        border-radius: 1rem;
    }

    .settings-bubble {
        background: white;
    }
}
.settings-panel.init {
    transform: translateX(0);
}
</style>
