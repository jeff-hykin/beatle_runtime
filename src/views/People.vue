<template>

<div>
<p contenteditable="true">Walter</p>
<p>Last seen: {{ date1 }}</p>
<img src="FacialRecTraining01.png"">

<h4></h4>

<p contenteditable="true">Omar</p>
<p>Last seen: {{ date2 }}</p>
<img src="omar.jpeg" alt="Example" width="130" height="100">
<p></p>

<p contenteditable="true">Jueff H</p>
<p>Last seen: {{ date3 }}</p>
<img src="FacialRecTraining17.png" alt="Example">

<p></p>

<p contenteditable="true">Sebastiant</p>
<p>Last seen: {{ date4 }}</p>
<img src="FacialRecTraining02.png">
<p></p>

</div>

 
</template>

<script>
  import VueGallery from 'vue-gallery';
  
  export default {
    data: function () {
      return {
        date1: '12:33::46 03:20::20',
        date2: '20:23::13 03:22::20',
        date3: '11:33::01 04:07::20',
        date4: '18:22::20 04:09::20',

        name1: 'Omar Santos',

        images: [

            
            'omar.jpeg',
            'omar.jpeg',
        
      ],
      index: null
      };
    },

    methods: {
  reloadPage(){
    window.location.reload()
  }
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