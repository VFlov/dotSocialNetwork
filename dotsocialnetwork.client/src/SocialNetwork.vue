<template>
  <div class="messenger">
    <aside class="sidebar">
      <header class="sidebar-header">
        <h3>Чаты</h3>
        <button @click="logout" class="logout-btn">Выйти</button>
      </header>
      <input v-model="searchQuery" placeholder="Поиск..." @input="searchUsers" class="search-input" />
      <ul v-if="searchedUsers.length" class="search-results">
        <li v-for="user in searchedUsers" :key="user.id" @click="createDialog(user.id)">
          {{ user.username }}
        </li>
      </ul>
      <ul class="dialogs">
        <li v-for="dialog in dialogs" :key="dialog.id" @click="selectDialog(dialog)"
            :class="{ active: selectedDialog?.id === dialog.id }">
          <div class="avatar" :style="{ backgroundColor: getAvatarColor(dialog) }"></div>
          <div class="dialog-info">
            <span class="username">{{ getDialogUsername(dialog) }}</span>
            <span class="last-message">{{ dialog.lastMessage || 'Нет сообщений' }}</span>
            <span v-if="getUnreadCount(dialog)" class="unread">{{ getUnreadCount(dialog) }}</span>
          </div>
        </li>
      </ul>
    </aside>
    <main class="chat" v-if="selectedDialog">
      <header class="chat-header">
        <div class="avatar" :style="{ backgroundColor: getAvatarColor(selectedDialog) }"></div>
        <span>{{ getDialogUsername(selectedDialog) }}</span>
      </header>
      <section class="messages" ref="chatMessages">
        <div v-for="message in messages" :key="message.id || message.tempId"
             :class="['message', { sent: message.senderId === currentUserId, received: message.senderId !== currentUserId, failed: message.failed }]">
          <div class="message-content">
            <span class="text">{{ message.text }}</span>
            <img v-if="message.attachmentUrl" :src="'https://45.130.214.139:5020' + message.attachmentUrl"
                 class="attachment" @click="openImage(message.attachmentUrl)" />
            <span class="time">{{ formatTime(message.time) }}</span>
          </div>
        </div>
      </section>
      <footer class="chat-footer">
        <input v-model="newMessage" @keypress.enter="sendMessage" placeholder="Сообщение..." />
        <button @click="$refs.fileInput.click()" class="attach-btn">📎</button>
        <input type="file" ref="fileInput" @change="handleFileUpload" hidden />
        <button @click="sendMessage" :disabled="isSending">➤</button>
      </footer>
    </main>
    <div v-if="selectedImage" class="image-modal" @click="closeImage">
      <img :src="'https://45.130.214.139:5020' + selectedImage" class="full-image" />
    </div>
  </div>
</template>

<script>
  import { jwtDecode } from 'jwt-decode';
  import { HubConnectionBuilder, HttpTransportType } from '@microsoft/signalr';

  export default {
    data() {
      return {
        dialogs: [],
        selectedDialog: null,
        messages: [],
        newMessage: '',
        searchQuery: '',
        searchedUsers: [],
        currentUserId: 0,
        connection: null,
        file: null,
        isSending: false,
        selectedImage: null
      };
    },
    methods: {
      async fetchDialogs() {
        const response = await this.fetchWithAuth('https://45.130.214.139:5020/api/chat/dialogs');
        if (response.ok) this.dialogs = await response.json();
        else this.$router.push('/auth');
      },
      async selectDialog(dialog) {
        this.searchedUsers = [];
        this.selectedDialog = dialog;
        const response = await this.fetchWithAuth(`https://45.130.214.139:5020/api/chat/messages/${dialog.id}`);
        if (response.ok) {
          this.messages = await response.json();
          this.$nextTick(() => this.scrollToBottom());
        } else if (response.status === 401) {
          this.$router.push('/auth');
        }
      },
      async searchUsers() {
        if (!this.searchQuery.trim()) {
          this.searchedUsers = [];
          return;
        }
        const response = await this.fetchWithAuth(`https://45.130.214.139:5020/api/chat/search-users?query=${encodeURIComponent(this.searchQuery)}`);
        if (response.ok) this.searchedUsers = await response.json();
        else if (response.status === 401) this.$router.push('/auth');
      },
      async createDialog(userId) {
        const response = await this.fetchWithAuth('https://45.130.214.139:5020/api/chat/dialogs', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(userId)
        });
        if (response.ok) {
          const newDialog = await response.json();
          this.dialogs.push(newDialog);
          this.selectDialog(newDialog);
          this.searchQuery = '';
          this.searchedUsers = [];
        }
      },
      async sendMessage() {
        if (!this.newMessage.trim() && !this.file || this.isSending) return;
        this.isSending = true;
        const tempId = Date.now();
        this.messages.push({
          tempId,
          senderId: this.currentUserId,
          text: this.newMessage,
          time: new Date(),
          attachmentUrl: this.file ? URL.createObjectURL(this.file) : null
        });
        this.$nextTick(() => this.scrollToBottom());

        const formData = new FormData();
        formData.append('text', this.newMessage);
        if (this.file) formData.append('attachment', this.file);

        const response = await this.fetchWithAuth(`https://45.130.214.139:5020/api/chat/messages/${this.selectedDialog.id}`, {
          method: 'POST',
          body: formData
        });
        if (response.ok) {
          const message = await response.json();
          const index = this.messages.findIndex(m => m.tempId === tempId);
          if (index !== -1) this.messages.splice(index, 1, message);
        } else {
          const index = this.messages.findIndex(m => m.tempId === tempId);
          if (index !== -1) this.messages[index].failed = true;
        }
        this.isSending = false;
        this.newMessage = '';
        this.file = null;
      },
      connectSignalR() {
        const token = localStorage.getItem('token');
        if (!token) {
          this.$router.push('/auth');
          return;
        }
        this.connection = new HubConnectionBuilder()
          .withUrl('https://45.130.214.139:5020/chatHub', {
            accessTokenFactory: () => token,
            transport: HttpTransportType.WebSockets
          })
          .withAutomaticReconnect()
          .build();

        this.connection.on('ReceiveMessage', (message) => {
          if (message.dialogId === this.selectedDialog?.id) {
            const index = this.messages.findIndex(m => m.tempId === message.tempId);
            if (index === -1) this.messages.push(message);
            this.$nextTick(() => this.scrollToBottom());
          }
          const dialogIndex = this.dialogs.findIndex(d => d.id === message.dialogId);
          if (dialogIndex !== -1) {
            this.dialogs[dialogIndex].lastMessage = message.text;
            if (message.senderId !== this.currentUserId) {
              this.dialogs[dialogIndex].user1Id === this.currentUserId
                ? this.dialogs[dialogIndex].user1UnreadCount++
                : this.dialogs[dialogIndex].user2UnreadCount++;
            }
          }
        });

        this.connection.start().then(() => console.log('SignalR подключен')).catch(console.error);
      },
      fetchWithAuth(url, options = {}) {
        const token = localStorage.getItem('token');
        if (!token) {
          this.$router.push('/auth');
          return Promise.reject(new Error('No token'));
        }
        options.headers = { ...options.headers, 'Authorization': `Bearer ${token}` };
        return fetch(url, options);
      },
      logout() {
        localStorage.removeItem('token');
        if (this.connection) this.connection.stop();
        this.$router.push('/auth');
      },
      getDialogUsername(dialog) {
        return dialog?.user1Id === this.currentUserId ? dialog.user2?.username : dialog.user1?.username || 'Unknown';
      },
      getAvatarColor(dialog) {
        return dialog?.user1Id === this.currentUserId ? '#007bff' : '#28a745';
      },
      getUnreadCount(dialog) {
        return dialog?.user1Id === this.currentUserId ? dialog.user2UnreadCount : dialog.user1UnreadCount;
      },
      scrollToBottom() {
        this.$refs.chatMessages.scrollTop = this.$refs.chatMessages.scrollHeight;
      },
      formatTime(date) {
        return new Date(date).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
      },
      handleFileUpload(event) {
        this.file = event.target.files[0];
      },
      openImage(attachmentUrl) {
        this.selectedImage = attachmentUrl;
      },
      closeImage() {
        this.selectedImage = null;
      }
    },
    created() {
      const token = localStorage.getItem('token');
      if (!token) {
        this.$router.push('/auth');
        return;
      }
      try {
        const decoded = jwtDecode(token);
        this.currentUserId = parseInt(decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']);
        this.fetchDialogs();
        this.connectSignalR();
      } catch (error) {
        localStorage.removeItem('token');
        this.$router.push('/auth');
      }
    },
    beforeUnmount() {
      if (this.connection) this.connection.stop();
    }
  };
</script>

<style scoped>
  .messenger {
    display: flex;
    height: 100vh;
    max-width: 1200px;
    margin: 0 auto;
    background: #fff;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    overflow: hidden;
  }

  .sidebar {
    width: 300px;
    background: #f5f7fa;
    display: flex;
    flex-direction: column;
  }

  .sidebar-header {
    padding: 16px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-bottom: 1px solid #e9ecef;
  }

    .sidebar-header h3 {
      margin: 0;
      font-size: 1.2rem;
      font-weight: 600;
      color: #333;
    }

  .logout-btn {
    padding: 6px 12px;
    background: #dc3545;
    color: #fff;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-size: 0.9rem;
    transition: background 0.2s;
  }

    .logout-btn:hover {
      background: #c82333;
    }

  .search-input {
    margin: 12px 16px;
    padding: 8px 12px;
    border: 1px solid #ddd;
    border-radius: 20px;
    font-size: 0.9rem;
    outline: none;
  }

  .search-results {
    position: absolute;
    top: 70px;
    left: 16px;
    right: 16px;
    background: #fff;
    border: 1px solid #ddd;
    border-radius: 8px;
    max-height: 200px;
    overflow-y: auto;
    z-index: 10;
    list-style: none;
    padding: 0;
  }

    .search-results li {
      padding: 8px 12px;
      cursor: pointer;
      font-size: 0.9rem;
    }

      .search-results li:hover {
        background: #f0f2f5;
      }

  .dialogs {
    flex: 1;
    overflow-y: auto;
    list-style: none;
    padding: 0;
    margin: 0;
  }

    .dialogs li {
      display: flex;
      align-items: center;
      padding: 12px 16px;
      cursor: pointer;
      transition: background 0.2s;
    }

      .dialogs li:hover {
        background: #e9ecef;
      }

      .dialogs li.active {
        background: #e6f0ff;
      }

  .avatar {
    width: 36px;
    height: 36px;
    border-radius: 50%;
    margin-right: 12px;
  }

  .dialog-info {
    flex: 1;
    overflow: hidden;
    display: flex;
    flex-direction: column;
    /* Вертикальное расположение */
    gap: 4px;
    /* Отступ между username и last-message */
  }

  .username {
    font-weight: 500;
    font-size: 0.95rem;
    color: #333;
  }

  .last-message {
    font-size: 0.85rem;
    color: #666;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .unread {
    background: #007bff;
    color: #fff;
    font-size: 0.75rem;
    padding: 2px 6px;
    border-radius: 10px;
    margin-left: 8px;
  }

  .chat {
    flex: 1;
    display: flex;
    flex-direction: column;
  }

  .chat-header {
    padding: 16px;
    border-bottom: 1px solid #e9ecef;
    display: flex;
    align-items: center;
    gap: 12px;
    background: #fff;
  }

    .chat-header span {
      font-weight: 500;
      font-size: 1.1rem;
      color: #333;
    }

  .messages {
    flex: 1;
    padding: 16px;
    overflow-y: auto;
    background: #f0f2f5;
  }

  .message {
    display: flex;
    margin-bottom: 12px;
  }

    .message.sent {
      justify-content: flex-end;
    }

    .message.received {
      justify-content: flex-start;
    }

  .message-content {
    max-width: 70%;
    padding: 10px 14px;
    border-radius: 12px;
    background: #fff;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
  }

  .message.sent .message-content {
    background: #007bff;
    color: #fff;
  }

  .message.failed .message-content {
    opacity: 0.7;
    border: 1px dashed #dc3545;
  }

  .text {
    word-break: break-word;
  }

  .attachment {
    max-width: 180px;
    margin-top: 6px;
    border-radius: 8px;
    cursor: pointer;
  }

  .time {
    font-size: 0.75rem;
    color: #999;
    margin-top: 4px;
    display: block;
  }

  .chat-footer {
    padding: 12px;
    display: flex;
    gap: 8px;
    background: #fff;
    border-top: 1px solid #e9ecef;
  }

    .chat-footer input {
      flex: 1;
      padding: 8px 12px;
      border: 1px solid #ddd;
      border-radius: 20px;
      font-size: 0.9rem;
      outline: none;
    }

    .chat-footer button {
      padding: 8px 12px;
      background: #007bff;
      color: #fff;
      border: none;
      border-radius: 20px;
      cursor: pointer;
      transition: background 0.2s;
    }

      .chat-footer button:hover {
        background: #0056b3;
      }

  .attach-btn {
    background: none;
    color: #666;
    font-size: 1.2rem;
  }

  .image-modal {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.8);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 1000;
  }

  .full-image {
    max-width: 90%;
    max-height: 90%;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
  }
</style>
