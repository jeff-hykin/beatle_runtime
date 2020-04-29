<template>
    <row height=7rem>        
        <Slide :closeOnNavigation="true" >
            <router-link
                v-for='eachRoute in routes'
                :key='eachRoute.path'
                class='nav-link'
                :to='eachRoute.path'
                v-slot="{ href, route, navigate }"
                >
                <i :class="`fa fa-${eachRoute.icon}`" aria-hidden="true" />
                <a :href="href" @click="navigate">
                    {{ route.name }}
                </a>
            </router-link>
        </Slide>
        <LogoutButton />
    </row>
</template>

<script>

import LogoutButton from "@/components/LogoutButton.vue"
import { Slide } from 'vue-burger-menu'
import { routes } from "@/router"

export default {
    name: 'Navigation',
    components: {
        LogoutButton,
        Slide,
    },
    data: ()=> ({
        routes,
    }),
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
a {
    color: var(--grey-100);
}
</style>