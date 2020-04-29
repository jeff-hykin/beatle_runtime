<template>
    <row :wrap=true>
        <VueGallery :images="imageData" :index="index" @close="index = null" />
        <div
            class="image"
            v-for="(imagePath, imageIndex) in fullImagePaths"
            :key="imageIndex"
            @click="index = imageIndex"
            :style='{ backgroundImage: `url("${imagePath}")`, width: "300px", height: "200px" }'
            />
        
    </row>
</template>

<script>
import VueGallery from "vue-gallery"

export default {
    components: {
        VueGallery,
    },
    data: ()=>({
        imagePaths: $root.systemData.galleryFiles,
        baseUrl: window.location.host,
        index: null,
    }),
    mounted() {
        console.log(`this.imageData is:`,this.imageData)
    },
    computed: {
        fullImagePaths() {
            return this.imagePaths.map(each=>`http://${this.baseUrl}/${each}`)
        },
        imageData() {
            return this.fullImagePaths.map(each=>{
                let type = "video/"
                let matchVideo = each.match(/.+\.(mp4|mov|avi)$/)
                let matchImage = each.match(/.+\.(png|jpg|jpeg)$/)
                if (matchVideo) {
                    type = `video/${matchVideo[1]}`
                } else {
                    type = `image/${matchImage[1]}`
                }
                return {
                    title: "",
                    href: each,
                    type
                }
            })
        }
    }
}
</script> 

<style scoped>
.image {
    float: left;
    background-size: cover;
    background-repeat: no-repeat;
    background-position: center center;
    border: 1px solid #ebebeb;
    margin: 5px;
}
</style>