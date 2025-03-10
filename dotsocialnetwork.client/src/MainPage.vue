<template>
  <div class="container">
    <!-- Hero Section -->
    <header class="hero">
      <img src="/src/assets/logo.svg" alt="Профиль" class="profile-img" />
      <h1>{{ name }}</h1>
      <p>{{ title }}</p>
    </header>

    <!-- Navigation -->
    <nav class="navbar">
      <ul>
        <li v-for="item in navItems" :key="item.href">
          <a :href="item.href">{{ item.label }}</a>
        </li>
      </ul>
    </nav>

    <!-- About Section -->
    <section class="section about" id="about">
      <h2>О себе</h2>
      <p>{{ aboutText }}</p>
    </section>

    <!-- Projects Section -->
    <section class="section projects" id="projects">
      <h2>Проекты</h2>
      <ul>
        <li v-for="project in projects" :key="project">
          <a href="#">{{ project }}</a>
        </li>
      </ul>
    </section>

    <!-- Skills Section -->
    <section class="section skills" id="skills">
      <h2>Навыки</h2>
      <div class="skill-container">
        <div class="skill-box" v-for="skill in skills" :key="skill.name">
          <div class="skill-icon">
            <img :src="skill.icon" :alt="skill.name" />
          </div>
          <h3>{{ skill.name }}</h3>
        </div>
      </div>
    </section>

    <!-- Contact Section -->
    <section class="section contact" id="contact">
      <h2>Контакты</h2>
      <ul>
        <li>Email: {{ email }}</li>
        <li>Телефон: {{ phone }}</li>
        <li>
          Ссылка на GitHub:
          <a :href="githubLink" target="_blank">{{ githubUsername }}</a>
        </li>
      </ul>
    </section>
  </div>
</template>

<script>
  export default {
    data() {
      return {
        name: "Василий",
        title: ".NET разработчик",
        aboutText: "Текст обо мне",
        email: "fefelov_vasiliy@bk.ru",
        phone: "+1234567890",
        githubUsername: "VFlov",
        githubLink: "https://github.com/VFlov",
        navItems: [
          { href: "chat", label: "Соц сеть" },
          { href: "examples", label: "Веб примеры" },
          { href: "map", label: "Карта" },
          { href: "videocall", label: "Calls" },
        ],
        projects: ["Проект 1", "Проект 2", "Проект 3"],
        skills: [
          { name: "HTML", icon: "/src/assets/icons/html.png" },
          { name: "CSS", icon: "/src/assets/icons/css.png" },
          { name: "JavaScript", icon: "/src/assets/icons/js.png" },
          { name: ".NET", icon: "/src/assets/icons/dotnet.png" },
          { name: "Unity", icon: "/src/assets/icons/unity.png" },
          { name: "C#", icon: "/src/assets/icons/cSharp.png" },
          { name: "Visual Studio", icon: "/src/assets/icons/vs.png" },
          { name: "ASP.NET", icon: "/src/assets/icons/asp.png" },
        ],
      };
    },
  };
</script>

<style scoped>
  @import url("https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600&display=swap");

  body {
    font-family: "Poppins", sans-serif;
    margin: 0;
    padding: 0;
    background-color: #f4f4f9;
    color: #333;
  }

  @keyframes fadeIn {
    from {
      opacity: 0;
    }

    to {
      opacity: 1;
    }
  }

  /* Base Layout */
  .container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 20px;
  }

  /* Hero Section */
  .hero {
    text-align: center;
    padding: 60px 0;
    background: linear-gradient(to right, #6a11cb, #2575fc);
    color: #fff;
    border-radius: 10px;
    margin-bottom: 40px;
    animation: fadeIn 2s ease-in-out forwards;
  }

  .profile-img {
    width: 150px;
    height: 150px;
    border-radius: 50%;
    margin-bottom: 20px;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
  }

  /* Navigation */
  .navbar {
    background-color: #333;
    padding: 10px 0;
    border-radius: 10px;
    margin-bottom: 40px;
  }

    .navbar ul {
      list-style-type: none;
      margin: 0;
      padding: 0;
      display: flex;
      justify-content: center;
    }

    .navbar li {
      margin: 0 15px;
    }

    .navbar a {
      color: #f2f2f2;
      padding: 14px 20px;
      text-decoration: none;
      transition: background-color 0.3s ease, transform 0.3s ease;
      border-radius: 5px;
      display: block;
    }

      .navbar a:hover {
        background-color: #444;
        transform: scale(1.05);
      }

  /* Common Section Styles */
  .section {
    margin-top: 40px;
    padding: 30px;
    border-radius: 10px;
    box-shadow: 0 0 15px rgba(0, 0, 0, 0.1);
    background-color: #fff;
    animation: fadeIn 2s ease-in-out forwards;
  }

    .section h2 {
      color: #333;
      margin-bottom: 20px;
      font-size: 28px;
    }

    .section p,
    .section ul {
      font-size: 16px;
      line-height: 1.6;
    }

  /* Projects */
  .projects li {
    margin-bottom: 10px;
  }

  /* Skills */
  .skill-container {
    display: flex;
    justify-content: space-around;
    flex-wrap: wrap;
    gap: 20px;
  }

  .skill-box {
    text-align: center;
    width: 200px;
    padding: 20px;
    background-color: #f9f9f9;
    border-radius: 10px;
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
    transition: transform 0.3s ease, box-shadow 0.3s ease;
  }

    .skill-box:hover {
      transform: translateY(-10px);
      box-shadow: 0 8px 20px rgba(0, 0, 0, 0.2);
    }

  .skill-icon img {
    width: 50px;
    height: 50px;
    margin-bottom: 15px;
  }

  .skill-box h3 {
    font-size: 18px;
    margin-bottom: 10px;
    color: #333;
  }

  /* Contact */
  .contact a {
    color: #6a11cb;
    text-decoration: none;
    transition: color 0.3s ease;
  }

    .contact a:hover {
      color: #2575fc;
    }

  /* Responsive Design */
  @media (max-width: 768px) {
    .navbar ul {
      flex-direction: column;
    }

    .navbar li {
      margin: 10px 0;
    }

    .skill-box {
      width: 45%;
    }
  }

  @media (max-width: 480px) {
    .skill-box {
      width: 100%;
    }
  }
</style>
