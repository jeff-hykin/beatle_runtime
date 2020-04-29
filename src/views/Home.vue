<template>
    <column>
        <!-- Icon/Title -->
        <column class=title-section align-v=center width=100vw padding=2rem :wrap=true>
            <img class=our-logo src="../assets/logo.png" alt="logo">
            <div style='min-width: 0.5rem; width: 3rem;'></div>
            <column align-h=center padding-top=1rem>
                <h1 class=our-title>Beatle Control Center</h1>
            </column>
        </column>
        <row class=content-container width=100vw margin=2rem>
            <ui-button color='primary' @click="toggleStatus">
                Toggle Arm/Disarm
            </ui-button>
        </row>
    </column>
</template>
<script>
export default {
    components: {},
    data:()=>({
        
    }),
    methods: {
        toggleStatus() {
            if ($root.systemData.status == "disarmed") {
                $root.systemData.status = "armed"
            } else {
                $root.systemData.status = "disarmed"
            }
        }
    }
}
</script>
<style lang="scss" scoped>
.title-section {
    .our-logo {
        height: 10rem;
    }
    
    .our-title {
        font-size: 39pt;
        font-weight: 100;
    }
}
.content-container {
    max-width: 68rem;
}
</style>