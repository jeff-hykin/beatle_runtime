<template>
    <div>
        <ui-button @click="onLogout">
            Logout
        </ui-button>
    </div>
</template>

<script>
export default {
    methods: {
        onLogout() {
            $root.loggedIn = false
            localStorage.removeItem("loggedIn")
        }
    }
}
</script>

<style scoped>
.ui-button {
    position: fixed;
    top: 2rem;
    right: 2rem;
    background-color: var(--grey-700) !important;
    color: white !important;
}
</style>