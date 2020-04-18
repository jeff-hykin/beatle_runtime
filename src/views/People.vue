<template>

<div>
<p contenteditable="true">Omar Santos</p>
<p>Last seen: {{ date1 }}</p>
<img src="baby.png" alt="Example" width="193" height="130">

<h4></h4>

<p contenteditable="true">Walter S</p>
<p>Last seen: {{ date2 }}</p>
<img src="baby.png" alt="Example" width="193" height="130">
<p></p>

<p contenteditable="true">Jueff H</p>
<p>Last seen: {{ date3 }}</p>
<img src="baby.png" alt="Example" width="193" height="130">

<p></p>

<p contenteditable="true">Sebastiant</p>
<p>Last seen: {{ date4 }}</p>
<img src="baby.png" alt="Example" width="193" height="130">
<p></p>


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
        date4: 'HH:MM::SS MM:DD::YY',
        images: [

            
            'omar.jpeg',
            'omar.jpeg',
        
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