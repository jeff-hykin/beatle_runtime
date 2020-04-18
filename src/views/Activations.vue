<template>
  <div>
    <gallery :images="images" :index="index" @close="index = null"></gallery>
    <div
      class="image"
      v-for="(image, imageIndex) in images"
      :key="imageIndex"
      @click="index = imageIndex"
      :style="{ backgroundImage: 'url(' + image + ')', width: '300px', height: '200px' }"
    ></div>
    </div>
 
</template>

<script>
  import VueGallery from 'vue-gallery';
  
  export default {
    data: function () {
      return {

        date1: '4/20/20',
        date2: '4/21/20',
        date3: '4/22/20',

        images: [
        {
          title: '4/22/21',
          href: 'https://www.youtube.com/watch?v=hNdlUHBJDKs',
          type: 'text/html',
          youtube: 'hNdlUHBJDKs',
          poster: './public/omar.jpeg'
        },

        {
          title: '4/23/20',
          href: 'funniest.mp4',
          type: 'video/mp4',
        },

          
        {
          title: '4/20/21',
          href: 'omar.jpeg',
          type: 'image/jpeg',
          poster: './public/omar.jpeg',  
      	}
      ],
      index: null
      };
    },

    components: {
      'gallery': VueGallery
    },
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