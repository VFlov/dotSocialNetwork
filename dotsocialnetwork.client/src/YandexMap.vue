<template>
  <div>
    <div class="stats">
      <p>Всего домов: {{ totalHouses }}</p>
      <p>Загружено домов: {{ loadedHouses }}</p>
    </div>
    <MapComponent @houses-loaded="updateLoadedHouses" />  <!-- Слушаем событие -->
  </div>
</template>

<script>
import MapComponent from './components/MapComponent.vue';

export default {
  components: {
    MapComponent
  },
  data() {
    return {
      totalHouses: 0,
      loadedHouses: 0
    };
  },
  async mounted() {
    await this.housesCount();
  },
  methods: {
    async housesCount() {
      try {
        const response = await fetch('https://45.130.214.139:5020/api/Map/housescount');
        const data = await response.json();
        this.totalHouses = data;
      } catch (error) {
        console.error('Ошибка при загрузке данных:', error);
      }
    },
    updateLoadedHouses(length) {
      this.loadedHouses = length;
    }
  }
}
</script>

<style scoped>
.stats {
  margin-bottom: 20px;
  font-size: 16px;
}

.stats p {
  margin: 5px 0;
}
</style>
