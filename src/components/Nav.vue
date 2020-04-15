<template>
    <nav>
        <row align-h=left padding=1rem>
            <router-link
                v-for='page in pages'
                :key='page.location'
                class='nav-link'
                :to='page.location'
                v-slot="{ href, route, navigate }"
                >
                <ui-button color='primary' :href="href" @click="navigate">
                    {{ route.name }}
                </ui-button>
            </router-link>
        </row>
    </nav>
</template>

<script>
export default {
    name: 'Navigation',
    data: ()=> ({
        pages: [
            {
                name: 'Home',
                location: '/'
            },
            {
                name: 'Help',
                location: '/help'
            },
            {
                name: 'Debug',
                location: '/debug'
            },

            {
                name: 'Gallery',
                location: '/gallery'
            },
        ]
    })
}
</script>

<style lang="scss" scoped>
// spaces between each button
.good-row * {
    margin-left: 1rem;
}
</style>