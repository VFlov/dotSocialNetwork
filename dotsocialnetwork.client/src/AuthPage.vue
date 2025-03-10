<!-- components/AuthPage.vue -->
<template>
  <div class="auth-container">
    <div class="auth-card">
      <h2>{{ isLogin ? 'Вход' : 'Регистрация' }}</h2>

      <form @submit.prevent="submitForm">
        <div class="form-group">
          <label for="username">Имя пользователя</label>
          <input v-model="form.username"
                 id="username"
                 type="text"
                 placeholder="Введите имя пользователя"
                 :class="{ 'error': errors.username }"
                 @input="clearError('username')">
          <span v-if="errors.username" class="error-message">{{ errors.username }}</span>
        </div>

        <div class="form-group">
          <label for="password">Пароль</label>
          <input v-model="form.password"
                 id="password"
                 type="password"
                 placeholder="Введите пароль"
                 :class="{ 'error': errors.password }"
                 @input="clearError('password')">
          <span v-if="errors.password" class="error-message">{{ errors.password }}</span>
        </div>

        <button type="submit"
                :disabled="isLoading"
                class="auth-button">
          {{ isLoading ? 'Загрузка...' : isLogin ? 'Войти' : 'Зарегистрироваться' }}
        </button>
      </form>

      <div class="switch-mode">
        <p>
          {{ isLogin ? 'Нет аккаунта?' : 'Уже зарегистрированы?' }}
          <span @click="toggleMode" class="link">
            {{ isLogin ? 'Зарегистрируйтесь' : 'Войдите' }}
          </span>
        </p>
      </div>

      <div v-if="error" class="error-message main-error">{{ error }}</div>
    </div>
  </div>
</template>

<script>
  export default {
    data() {
      return {
        isLogin: true,
        isLoading: false,
        form: {
          username: '',
          password: ''
        },
        errors: {
          username: '',
          password: ''
        },
        error: ''
      };
    },
    methods: {
      toggleMode() {
        this.isLogin = !this.isLogin;
        this.clearErrors();
      },
      clearError(field) {
        this.errors[field] = '';
        this.error = '';
      },
      clearErrors() {
        this.errors = { username: '', password: '' };
        this.error = '';
      },
      validateForm() {
        this.clearErrors();
        let isValid = true;

        if (!this.form.username.trim()) {
          this.errors.username = 'Введите имя пользователя';
          isValid = false;
        } else if (this.form.username.length > 50) {
          this.errors.username = 'Имя пользователя не должно превышать 50 символов';
          isValid = false;
        }

        if (!this.form.password) {
          this.errors.password = 'Введите пароль';
          isValid = false;
        } else if (this.form.password.length < 6) {
          this.errors.password = 'Пароль должен содержать минимум 6 символов';
          isValid = false;
        }

        return isValid;
      },
      async submitForm() {
        if (!this.validateForm()) return;

        this.isLoading = true;

        try {
          if (this.isLogin) {
            await this.login();
          } else {
            await this.register();
          }
        } catch (err) {
          this.error = err.message || 'Произошла ошибка';
        } finally {
          this.isLoading = false;
        }
      },
      async login() {
        const response = await fetch('https://45.130.214.139:5020/api/auth/login', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(this.form)
        });

        if (!response.ok) {
          throw new Error('Неверные учетные данные');
        }

        const data = await response.json();
        localStorage.setItem('token', data.token);
        this.$emit('authenticated');
        this.$router.push('/chat');
      },
      async register() {
        // Для регистрации добавим endpoint на backend
        const response = await fetch('https://45.130.214.139:5020/api/auth/register', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(this.form)
        });

        if (!response.ok) {
          const error = await response.json();
          throw new Error(error.message || 'Ошибка регистрации');
        }

        // После успешной регистрации автоматически логинимся
        await this.login();
      }
    }
  };
</script>

<style scoped>
  .auth-container {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 100vh;
    background: #f0f2f5;
  }

  .auth-card {
    background: white;
    padding: 2rem;
    border-radius: 10px;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    width: 100%;
    max-width: 400px;
  }

  h2 {
    text-align: center;
    margin-bottom: 2rem;
    color: #333;
  }

  .form-group {
    margin-bottom: 1.5rem;
  }

  label {
    display: block;
    margin-bottom: 0.5rem;
    color: #666;
  }

  input {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 5px;
    font-size: 1rem;
    transition: border-color 0.2s;
  }

    input:focus {
      outline: none;
      border-color: #007bff;
    }

    input.error {
      border-color: #dc3545;
    }

  .error-message {
    color: #dc3545;
    font-size: 0.875rem;
    margin-top: 0.25rem;
    display: block;
  }

  .main-error {
    text-align: center;
    margin-top: 1rem;
  }

  .auth-button {
    width: 100%;
    padding: 0.75rem;
    background: #007bff;
    color: white;
    border: none;
    border-radius: 5px;
    font-size: 1rem;
    cursor: pointer;
    transition: background 0.2s;
  }

    .auth-button:hover:not(:disabled) {
      background: #0056b3;
    }

    .auth-button:disabled {
      background: #ccc;
      cursor: not-allowed;
    }

  .switch-mode {
    text-align: center;
    margin-top: 1.5rem;
  }

    .switch-mode p {
      color: #666;
      margin: 0;
    }

  .link {
    color: #007bff;
    cursor: pointer;
    margin-left: 0.25rem;
  }

    .link:hover {
      text-decoration: underline;
    }
</style>
