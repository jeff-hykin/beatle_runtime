<template>
    <nav>
        <row align-h=space-between>
            <!-- regular nav buttons -->
            <row align-h=left :flex-wrap='true'>
                <router-link
                    v-for='route in routes'
                    :key='route.path'
                    class='nav-link'
                    :to='route.path'
                    v-slot="{ href, route, navigate }"
                    >
                    <ui-button color='primary' :href="href" @click="navigate">
                        {{ route.name }}
                    </ui-button>
                </router-link>
            </row>
            <!-- Logout -->
            <row align-h=left>
                <LogoutButton />
            </row>
        </row>
    </nav>
</template>

<script>

import LogoutButton from "@/components/LogoutButton.vue"
import { routes } from "@/router"

export default {
    name: 'Navigation',
    components: {
        LogoutButton,
    },
    data: ()=> ({
        routes,
    })
}
</script>

<style lang="scss" scoped>
.ui-button {
    margin: 1rem;
}
// spaces between each button
.good-row * {
    margin-left: 1rem;
    margin-right: 1rem;
}
</style>