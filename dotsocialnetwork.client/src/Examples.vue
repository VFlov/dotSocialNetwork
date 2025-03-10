<template>
  <div>
    <h1>Example</h1>
    <button @click="redirectToPixelFade">Перейти на HTML страницу</button>
    <h1>Example</h1>
    <button @click="redirectToBubbles">Перейти на HTML страницу</button>
    <h1>Example</h1>
    <button @click="redirectToCard">Перейти на HTML страницу</button>
  </div>
</template>
  
  <script scoped>
export default {
  methods: {
    redirectToPixelFade() {
      window.location.href = './src/html/PixelFade.html'; // Замените на ваш путь
    },
    redirectToBubbles() {
      window.location.href = './src/html/BackgroundBubbles.html'; // Замените на ваш путь
    },
    redirectToCard() {
      window.location.href = './src/html/Card.html'; // Замените на ваш путь
    }
  }
}
</script>