<template>
  <div id="map" style="width: 100%; height: 90vh;"></div>
</template>

<script>
export default {
  name: 'MapComponent',
  data() {
    return {
      houses: [],
      icons: {
        AAA: '/src/assets/icons/AAAA.png',
        AA: '/src/assets/icons/AA.png',
        А: '/src/assets/icons/A.png', 
        В: '/src/assets/icons/B.png', 
        С: '/src/assets/icons/C.png', 
        D: '/src/assets/icons/D.png', 
        E: '/src/assets/icons/E.png',
        F: '/src/assets/icons/F.png',
        G: '/src/assets/icons/G.png'
      }
    };
  },
  async mounted() {
    await this.fetchHouses();
    this.initMap();
  },
  methods: {
    async fetchHouses() {
      try {
        const response = await fetch('https://45.130.214.139:5020/api/Map/gethouses');
        const data = await response.json();
        this.houses = data;
        this.$emit('houses-loaded', this.houses.length); // Отправляем событие
      } catch (error) {
        console.error('Ошибка при загрузке данных:', error);
      }
    },
    initMap() {
      ymaps.ready(() => {
        const map = new ymaps.Map('map', {
          center: [55.76, 37.64],
          zoom: 10
        });

        // Создаем макет для иконки
        const iconLayout = ymaps.templateLayoutFactory.createClass(
          `<div style="position: relative; width: 30px; height: 44px;">
             <img src="{{ properties.icon }}" 
                  style="position: absolute; top: 0; left: 0; width: 30px; height: 44px;">
           </div>`
        );

        this.houses.forEach(house => {
          const coordinates = [house.hplGeoLatitude, house.hplGeoLongitude];
          const hint = house.ihsEeClass;
          const balloonContent = `Класс: ${house.ihsEeClass}`;

          // Выбираем иконку на основе ihsEeClass
          const icon = this.icons[house.ihsEeClass[0]] || 'path/to/default-icon.png'; // Используем иконку по умолчанию, если ihsEeClass не найден

          const placemark = new ymaps.Placemark(
            coordinates,
            {
              hintContent: hint,
              balloonContent: balloonContent,
              icon: icon // Передаем выбранную иконку в свойства метки
            },
            {
              iconLayout: iconLayout, // Используем созданный макет
              iconShape: {
                type: 'Circle',
                coordinates: [0, 0],
                radius: 16
              }
            }
          );

          map.geoObjects.add(placemark);
        });
      });
    }
  }
};
</script>

<style scoped>
#map {
  width: 100%;
  height: 600px;
  margin-top: 20px;
  border-radius: 10px;
  box-shadow: 0 0 15px rgba(0, 0, 0, 0.1);
}
</style>