<template>

  <router-view @authenticated="checkAuth"></router-view>

</template>

<style>
  #app {
    height: 100%;
    width: 100%;
    background-color: #fff;
    box-shadow: 0 0 20px rgba(0, 0, 0, 0.1);
    border-radius: 10px;
    font-family: 'Ubuntu', sans-serif;
  }
</style>
