<template>
    <column height=100vh>
        <h3>
            Lets get you setup
        </h3>
        <column align-v=space-evenly shadow=2 padding='1.5rem' padding-bottom=1.3rem padding-top=0.5rem width=20rem height=14rem margin=2rem border-radius=10px>
        
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
export default {
    data:()=>({
        username: "",
        password: "",
        pin: "",
    }),
    mounted() {
        
    },
    computed: {
        pinIsValid() {
            if (this.pin.length >= 4) {
                if (this.pin.replace(/[^0-9]/g,"").length == this.pin.length) {
                    return true
                }
            }
            return false
        }
    },
    methods: {
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

<style>

</style>