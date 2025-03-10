<template>
  <div class="container">
    <!-- Header -->
    <header class="py-4 text-center header-bg">
      <h1>Добро пожаловать на Valheim Server!</h1>
      <p>Лучший сервер для приключений в мире викингов</p>
    </header>

    <!-- News Section -->
    <section class="row">
      <h2>Новости</h2>
      <div class="col-md-8">
        <div v-for="news in newsItems" :key="news.id" class="card mb-3">
          <div class="card-body">
            <h5 class="card-title">{{ news.title }}</h5>
            <p class="card-text">{{ news.content }}</p>
            <p class="card-text"><small class="text-muted">{{ formatDate(news.date) }}</small></p>
          </div>
        </div>
      </div>
    </section>

    <!-- Launcher Download -->
    <section class="my-4">
      <h2>Скачать лаунчер</h2>
      <p>Скачайте наш кастомный лаунчер для быстрого подключения к серверу!</p>
      <a href="/downloads/launcher.exe" class="btn btn-primary">Скачать</a>
    </section>

    <!-- Forum -->
    <section class="my-4">
      <h2>Форум</h2>
      <div class="card">
        <div class="card-body">
          <form @submit.prevent="submitPost">
            <div class="mb-3">
              <textarea v-model="newPost" class="form-control" placeholder="Напишите сообщение..."></textarea>
            </div>
            <button type="submit" class="btn btn-primary">Отправить</button>
          </form>
        </div>
      </div>
      <div v-for="post in forumPosts" :key="post.id" class="card mt-3">
        <div class="card-body">
          <p>{{ post.content }}</p>
          <small class="text-muted">{{ formatDate(post.date) }}</small>
        </div>
      </div>
    </section>
  </div>
</template>

<script>
  import { ref, onMounted } from 'vue';

  export default {
    setup() {
      const newsItems = ref([]);
      const forumPosts = ref([]);
      const newPost = ref('');

      const fetchNews = async () => {
        try {
          const response = await fetch('https://45.130.214.139:5020/api/news');
          newsItems.value = await response.json();
        } catch (error) {
          console.error('Error fetching news:', error);
        }
      };

      const formatDate = (date) => {
        return new Date(date).toLocaleDateString('ru-RU');
      };

      const submitPost = () => {
        if (newPost.value.trim()) {
          forumPosts.value.unshift({
            id: Date.now(),
            content: newPost.value,
            date: new Date()
          });
          newPost.value = '';
        }
      };

      onMounted(() => {
        fetchNews();
      });

      return {
        newsItems,
        forumPosts,
        newPost,
        formatDate,
        submitPost,
        fetchNews
      };
    }
  };
</script>

<style scoped>
  /* Optional: Add component-specific styles here */
  body {
    background-color: #f8f9fa;
    font-family: Arial, sans-serif;
  }

  .header-bg {
    background-color: #343a40;
    color: white;
    border-radius: 5px;
  }

  .card {
    margin-bottom: 15px;
  }

  .btn-primary {
    background-color: #007bff;
    border-color: #007bff;
  }
</style>
