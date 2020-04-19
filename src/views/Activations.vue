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

        date1: 'HH:MM::SS MM:DD::YY',
        date2: 'HH:MM::SS MM:DD::YY',
        date3: 'HH:MM::SS MM:DD::YY',

        images: [
        {
          title: 'HH:MM::SS MM:DD::YY',
          
          poster: './public/omar.jpeg'
        },

        {
          title: 'HH:MM::SS MM:DD::YY',
          href: 'funniest.mp4',
          type: 'video/mp4',
        },

          'Activation001.png',
          'Activation002.png',
          'Activation003.png',
          'Activation004.png',

        {
          title: 'HH:MM::SS MM:DD::YY',
          poster: 'Activation001.png',  
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