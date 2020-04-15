<template>
    <row class=wrapper align-h=left align-v=top>
        <!-- Settings Panel -->
        <settingsPanel>
            <!-- Labels -->
            <template v-slot:labels>
                <!-- Labels to display -->
                <column v-if="showLabels" class="labels-bubble bubble" shadow="1" align-h="left">
                    <ui-switch v-for="(eachLabel, eachIndex) in labelToggles" :key="eachIndex" v-model="labelToggles[eachIndex]" :label="eachIndex"></ui-switch>
                </column>
            </template>
        </settingsPanel>
        <!-- Settings panel gost -->
        <div class=panel-ghost ></div>
        <!-- Main Section -->
        <div class=main-area>
        </div>
    </row>
</template>
<script>
import { remote } from "electron"
import fs, { write } from "fs"
import path from 'path'

// components/mixins
import settingsPanel, {settingsPanelComponent} from "@/components/settingsPanel"


let   util    = require("util")
const readdir = util.promisify(fs.readdir)
let   dialog  = remote.dialog
let   app     = remote.app

// prevent scrollbars that shouldn't be there
document.body.style.overflow = 'hidden'

// 
// Controls
// 
let windowListeners$ = {
    mousemove(eventObj) {
    },
    keydown(eventObj) {
    },
    keyup(eventObj) {
    },
}

export default {
    name: "main-page",
    components: {
        settingsPanel,
    },
    data: ()=>({
        settings: {},
        windowListeners$
    }),
    mounted() {
        // debugging
        window.main = this 
        // connect the settings panel
        this.settings = settingsPanelComponent.settings
    },
    computed: {
    },
    methods: {
    },
}

</script>
<style lang='scss' scoped>
    @import url('https://fonts.googleapis.com/css?family=Source+Sans+Pro');
    @import url('https://fonts.googleapis.com/css?family=Roboto:100,300,400&display=swap');
    
    * {
        margin: 0;
        padding: 0;
    }
    
    .ui-button {
        padding: 0.8em 1.7em;
    }
    
    // variables for child elements
    .wrapper , ::v-deep {
        --unhovered-panel-amount: 4.2rem;
        --blue: #2196F3;
        --green: #64FFDA;
        --red: #EF5350;
    }
    
    .wrapper {
        background: radial-gradient(circle, rgba(255,255,255,1) 0%, rgba(227,227,227,1) 100%);
        width: 100vw;
        
        
        .bubble {
            width: 100%;
            margin-top: 0.8rem;
            padding: 1.5rem;
            border-radius: 1rem;
        }
        
        .panel-ghost {
            min-width: var(--unhovered-panel-amount);
        }
        
        .main-area {
            height: 100vh;
            flex-grow: 1;
            justify-content: space-between;
            align-items: flex-start;
            display: flex;
            flex-direction: column;
            flex-wrap: nowrap;
            margin: 0;
        }
    }
    
</style>
