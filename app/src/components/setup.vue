<template>
    <column height=100vh>
        <h3>
            Lets get you setup
        </h3>
        <column v-if="activeStep == 1" class="step-container" align-v=space-evenly shadow=2>
            <ui-checkbox v-model="check1">This computer has been connected to WiFi (WiFi doesn't have to be internet-enabled)</ui-checkbox>
            <ui-checkbox v-model="check2">My phone (or other computer) is connected to the same WiFi</ui-checkbox>
            <transition name="fade" mode="out-in" >
                <ui-button
                    v-if="check1 && check2"
                    color=primary
                    @click="nextStep"
                    >
                        Next
                </ui-button>
            </transition>
        </column>
        
        <column v-if="activeStep == 2" class="step-container" align-v=space-evenly shadow=2 >
        
            <ui-textbox
                floating-label
                label="New Username"
                :autofocus="true"
                
                @keydown="onKeydown($event)"
                v-model="username"
            />
            <ui-textbox
                floating-label
                label="New Password"
                type=password
                
                @keydown="onKeydown($event)"
                v-model="password"
            />
            <ui-textbox
                floating-label
                label="New Keypad-Pin"
                type=password
                
                @keydown="onKeydown($event)"
                v-model="pin"
            />
            <column height=1rem  />
            <ui-button
                :disabled="password.length == 0 || username.length == 0 || !pinIsValid" color=primary
                @click="onSubmit"
                >
                    Submit
            </ui-button>
        </column>
    </column>
</template>

<script>
const { spawn } = require('child_process')

export default {
    data:()=>({
        check1: false,
        check2: false,
        activeStep: 1,
        username: "",
        password: "",
        pin: "",
    }),
    mounted() {
        // start turning on the full Beatle system after a sec
        setTimeout(() => {
            let shouldStart = true
            let timeout = 2000
            this.$toasted.show(`Starting up full Beatle System now`, {
                action:[
                    {
                        text : 'Cancel',
                        onClick : (eventData, toastObject) => {
                            shouldStart = false
                        },
                    },
                ]
            }).goAway(timeout)
            
            setTimeout(() => {
                if (shouldStart) {
                    // starts the centralServer
                    const subprocess = spawn("node", [pathFor.centralServer], {
                        detached: true,
                        stdio: 'ignore'
                    })
                    subprocess.unref()
                }
            }, timeout + 300)
        }, 1500)
    },
    computed: {
        pinIsValid() {
            if (this.pin.length >= 4 && this.pin.length <= 6) {
                if (this.pin.replace(/[^0-9]/g,"").length == this.pin.length) {
                    return true
                }
            }
            return false
        }
    },
    methods: {
        nextStep() {
            this.activeStep++
        },
        onKeydown($event) {
            if ($event.key == "Enter") {
                this.onSubmit()
            }
        },
        onSubmit() {
            console.log("submitting")
            // try to make the account (will throw error if bad data)
            passwordManager.setUsernameAndPassword({ username: this.username, password: this.password })
            passwordManager.setPin(this.pin)
            // tell the app to end setup process
            $root.needsSetup = false
            // tell the user about the success
            this.$toasted.show(`User account setup successfully`, {keepOnHover:true}).goAway(6500)
        }
    },
}
</script>

<style scoped>

.step-container {
    padding: 1.5rem;
    padding-bottom: 1.3rem;
    padding-top: 0.5rem;
    width: 20rem;
    height: 14rem;
    margin: 2rem;
    border-radius: 10px;
}

</style>