<template>
  <div>
    <VueGallery :images="$root.systemData.galleryFiles" :index="index" @close="index = null" />
    <div
      class="image"
      v-for="(imagePath, imageIndex) in $root.systemData.galleryFiles"
      :key="imageIndex"
      @click="index = imageIndex"
      :style='{ backgroundImage: `url("http://${baseUrl}/${imagePath}")`, width: "300px", height: "200px" }'
    ></div>
  </div>
</template>

<script>
import VueGallery from "vue-gallery"

export default {
    components: {
        VueGallery,
    },
    data: ()=>({
        baseUrl: window.location.host,
        index: null,
    })
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