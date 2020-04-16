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
        images: [
        {
          href: 'https://www.youtube.com/watch?v=hNdlUHBJDKs',
          type: 'text/html',
          youtube: 'hNdlUHBJDKs',
          poster: 'https://img.youtube.com/vi/hNdlUHBJDKs/maxresdefault.jpg'
        },
        {
          
          href: 'https://www.youtube.com/watch?v=s5iUsaPPtnk',
          type: 'text/html',
          youtube: 's5iUsaPPtnk',
          poster: 'https://img.youtube.com/vi/s5iUsaPPtnk/maxresdefault.jpg'
        },

        {
          title: 'Video from files',
          href: 'funniest.mp4',
          type: 'video/mp4',
          
        },

            'omar.jpeg',
        {
          title: 'Image',
          href: 'omar.jpeg',
          type: 'image/jpeg',
          poster: 'omar.jpeg',  
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