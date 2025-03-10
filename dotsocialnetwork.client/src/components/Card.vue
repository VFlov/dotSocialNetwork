<template>
  <div>
    <div class="container">
      <div class="box" :style="gradientStyle" @click="handleClick">
        <b class="name">
          <span v-if="item.name === 'Создать комнату'">{{ item.name }}</span>
          <span v-else>Комната<br><h2>{{ item.name }}</h2></span>
        </b>
        <div v-if="item.name === 'Создать комнату'" class="image-placeholder"></div>
        <p v-else class="count">Участников {{ item.participantCount }}</p>
      </div>
    </div>
  </div>
</template>

<script>
  export default {
    props: {
      item: {
        type: Object,
        required: true,
        default: () => ({ name: '', participantCount: 0 })
      }
    },
    data() {
      return {
        gradientStyle: {}
      };
    },
    mounted() {
      console.log("Card item:", this.item);
      this.generateRandomGradient();
    },
    methods: {
      getRandomColor() {
        const letters = '0123456789ABCDEF';
        let color = '#';
        for (let i = 0; i < 6; i++) {
          color += letters[Math.floor(Math.random() * 16)];
        }
        return color;
      },
      generateRandomGradient() {
        const color1 = this.getRandomColor();
        const color2 = this.getRandomColor();
        const angle = Math.floor(Math.random() * 360);
        this.gradientStyle = {
          background: `linear-gradient(${angle}deg, ${color1}, ${color2})`
        };
      },
      handleClick() {
        if (!this.item || !this.item.name) {
          console.error("item or item.name is undefined in TemplateCard:", this.item);
          return;
        }
        console.log("Emitting my-event with room name:", this.item.name);
        this.$emit('my-event', this.item.name);
      }
    }
  };
</script>

<style scoped>
  * {
    font-size: 24px;
    margin: 0px;
    padding: 0px;
    box-sizing: border-box;
    font-family: "Poppins", serif;
  }

  .container {
    position: relative;
    display: flex;
    justify-content: center;
    align-items: center;
    flex-wrap: wrap;
    padding: 50px;
    gap: 50px;
  }

  .box {
    position: relative;
    height: 400px;
    width: 280px;
    background: #fff;
    border-radius: 20px;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    text-align: center;
    cursor: pointer; /* Указывает, что карточка кликабельна */
    transition: transform 0.3s ease, box-shadow 0.3s ease; /* Анимация */
  }

    .box:hover {
      transform: scale(1.05); /* Легкое увеличение при наведении */
      box-shadow: 0 10px 20px rgba(0, 0, 0, 0.4); /* Тень при наведении */
    }

    .box:active {
      transform: scale(0.95); /* Уменьшение при клике */
      box-shadow: 0 5px 10px rgba(0, 0, 0, 0.2); /* Меньшая тень при клике */
    }

    .box::before {
      content: "";
      position: absolute;
      inset: 0;
      border-radius: 20px;
      background: v-bind(gradientStyle);
    }

    .box::after {
      content: "";
      position: absolute;
      inset: 0;
      border-radius: 20px;
      background: v-bind(gradientStyle);
      filter: blur(16px);
    }

    .box::before,
    .box::after {
      z-index: 1;
    }

    .box b {
      padding: 30px;
      position: absolute;
      display: block;
      inset: 4px;
      border-radius: 16px;
      background: rgba(0, 0, 0, 0.75);
      z-index: 2;
    }

      .box b p {
        font-weight: 200;
        text-shadow: 0 0 15px #fff;
        z-index: 2;
      }

  .name {
    padding: 30px;
    position: relative;
    z-index: 2;
    border-radius: 16px;
    background: rgba(0, 0, 0, 0.75);
    color: #fff;
    font-weight: bold;
    display: block;
    text-shadow: 0 0 5px #fff;
  }

  .count {
    position: relative;
    z-index: 2;
    color: #ddd;
    font-weight: 200;
    text-shadow: 0 0 5px #ddd;
  }

  .image-placeholder {
    width: 150px;
    height: 150px;
    border-radius: 10px;
    margin-top: 20px;
    z-index: 2;
    background-image: url('../assets/icons/AddRoom.png'); /*  Добавляем изображение */
    background-size: contain; /*  Масштабируем изображение, чтобы оно поместилось внутри */
    background-repeat: no-repeat; /*  Не повторяем изображение */
    background-position: center; /*  Центрируем изображение */
  }
</style>
