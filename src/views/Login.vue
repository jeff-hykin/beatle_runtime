<template>
    <column height=100vh>
        <h3>
            Beatle Login
        </h3>
        <column align-v=space-evenly shadow=2 padding='1.5rem' padding-bottom=1.3rem padding-top=1rem width=20rem height=14rem margin=2rem border-radius=10px>
        
            <ui-textbox
                floating-label
                label="Username"
                :autofocus="true"
                
                @keydown="onKeydown($event)"
                v-model="input.username"
            />
            <ui-textbox
                floating-label
                label="Password"
                type=password
                
                @keydown="onKeydown($event)"
                v-model="input.password"
            />
            <column height=1rem  />
            <ui-button
                :disabled="input.password.length == 0 || input.username.length == 0" color=primary
                @click="login"
                >
                    Login
            </ui-button>
        </column>
    </column>
</template>

<script>
    export default {
        name: 'Login',
        data() {
            return {
                input: {
                    username: "",
                    password: ""
                }
            }
        },
        methods: {
            onKeydown($event) {
                // if user presses the enter key, try to log them in
                if ($event.key == "Enter") {
                    this.login()
                }
            },
            login() {
                console.log(`this.input is:`,this.input)
                socket.emit("interface.attemptLogin", { username: this.input.username, password: this.input.password })
            }
        }
    }
</script>

<style scoped>
    #login {
        width: 500px;
        border: 1px solid #CCCCCC;
        background-color: #FFFFFF;
        margin: auto;
        margin-top: 200px;
        padding: 20px;
    }
</style>