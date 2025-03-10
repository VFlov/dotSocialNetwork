<template>
  <div class="video-call-wrapper custom-text-color">
    <div class="video-call-container">
      <h1>Voice Chat</h1>
      <div class="nav-buttons">
        <button @click="goToHome" class="btn btn-default">Return to Home</button>
      </div>

      <div v-if="showCreateRoomModal" class="modal-overlay">
        <div class="modal-content">
          <h2>Создать комнату</h2>
          <input type="text" v-model="roomName" placeholder="Введите имя комнаты" class="modal-input">
          <div class="modal-buttons">
            <button @click="createAndJoinRoom" class="btn btn-primary">Создать</button>
            <button @click="showCreateRoomModal = false" class="btn btn-secondary">Отмена</button>
          </div>
        </div>
      </div>

      <div v-if="!inRoom" class="room-form">
        <input type="text" v-model="roomName" placeholder="Enter Room Name" class="form-input">
        <button @click="joinRoom" class="btn btn-primary">Join Room</button>
      </div>

      <div v-else class="room-content">
        <p>В комнате: {{ roomName }}</p>
        <button @click="leaveRoom" class="btn btn-secondary">Leave Room</button>
      </div>

      <audio ref="localAudio" muted autoplay></audio>
      <audio v-for="peer in remotePeers" :key="peer.connectionId" :ref="`remoteAudio-${peer.connectionId}`" autoplay></audio>
    </div>

    <div class="card-grid">
      <div class="card-container">
        <template-card :item="createRoomCard" @my-event="showCreateRoomModal = true" />
      </div>
      <div v-for="item in items" :key="item.name" class="card-container">
        <template-card v-if="item && item.name" :item="item" @my-event="joinRoomFromCard" />
      </div>
    </div>
  </div>
</template>

<script>
  import TemplateCard from './components/Card.vue';
  import * as signalR from "@microsoft/signalr";
  import * as adapter from 'webrtc-adapter';

  export default {
    name: 'VoiceChat',
    components: { TemplateCard },
    data() {
      return {
        items: [],
        showCreateRoomModal: false,
        hubConnection: null,
        roomName: '',
        userId: '',
        inRoom: false,
        localStream: null,
        peerConnections: {},
        remotePeers: [],
        originalBodyBackgroundColor: '',
        originalAppBackgroundColor: '',
        createRoomCard: { name: 'Создать комнату', participantCount: '' } // Фиктивная карточка
      };
    },
    mounted() {
      this.setupBackgroundColors();
      this.initializeSignalR();
      this.userId = Math.floor(Math.random() * 1000).toString();
    },
    beforeUnmount() {
      this.restoreBackgroundColors();
      this.cleanupConnections();
    },
    methods: {
      setupBackgroundColors() {
        const appElement = document.getElementById('app');
        this.originalAppBackgroundColor = appElement.style.backgroundColor;
        this.originalBodyBackgroundColor = document.body.style.backgroundColor;
        appElement.style.backgroundColor = '#1C1C1C';
        document.body.style.backgroundColor = '#1d031f';
      },
      restoreBackgroundColors() {
        document.body.style.backgroundColor = this.originalBodyBackgroundColor;
        document.getElementById('app').style.backgroundColor = this.originalAppBackgroundColor;
      },
      initializeSignalR() {
        const token = localStorage.getItem('token');
        if (!token) {
          this.$router.push('/auth');
          return;
        }
        this.hubConnection = new signalR.HubConnectionBuilder()
          .withUrl("https://45.130.214.139:5020/callHub", { accessTokenFactory: () => token })
          .withAutomaticReconnect()
          .build();

        this.hubConnection.on("UserJoined", this.handleUserJoined);
        this.hubConnection.on("UserLeft", this.handleUserLeft);
        this.hubConnection.on("ReceiveSignal", this.handleReceiveSignal);
        this.hubConnection.on("ReceiveRoomList", (rooms) => {
          console.log("Received room list:", rooms); // Добавляем лог
          this.items = rooms;
        });
        this.hubConnection.on("JoinedRoom", (roomName) => {
          console.log(`Successfully joined room: ${roomName}`);
        });
        this.hubConnection.on("LeftRoom", (roomName) => {
          console.log(`Successfully left room: ${roomName}`);
        });

        this.hubConnection.start()
          .then(() => console.log("SignalR Connected!"))
          .then(() => this.hubConnection.invoke("GetRoomList"))
          .catch(err => console.error("SignalR Error:", err));
      },
      goToHome() {
        this.$router.push('/');
      },
      async createAndJoinRoom() {
        try {
          this.localStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: false });
          if (this.$refs.localAudio) {
            this.$refs.localAudio.srcObject = this.localStream;
          }
          console.log("Creating and joining room:", this.roomName); // Лог для проверки
          await this.hubConnection.invoke("CreateRoom", this.roomName, this.userId); // Вызываем CreateRoom
          await this.hubConnection.invoke("JoinRoom", this.roomName, this.userId);
          this.inRoom = true;
          this.showCreateRoomModal = false;
        } catch (error) {
          console.error("Error creating and joining room:", error);
        }
      },
      async joinRoom() {
        try {
          this.localStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: false });
          if (this.$refs.localAudio) {
            this.$refs.localAudio.srcObject = this.localStream;
          }
          console.log("Invoking JoinRoom with roomName:", this.roomName, "userId:", this.userId);
          await this.hubConnection.invoke("JoinRoom", this.roomName, this.userId);
          this.inRoom = true;
        } catch (error) {
          console.error("Error joining room:", error);
        }
      },
      async joinRoomFromCard(roomName) {
        console.log("Received roomName in joinRoomFromCard:", roomName); // Лог входного параметра
        this.roomName = roomName;
        console.log("Assigned this.roomName:", this.roomName); // Лог после присваивания
        await this.joinRoom();
      },
      async leaveRoom() {
        await this.hubConnection.invoke("LeaveRoom");
        this.inRoom = false;
        if (this.localStream) {
          this.localStream.getTracks().forEach(track => track.stop());
          this.localStream = null;
        }
        Object.keys(this.peerConnections).forEach(this.closePeerConnection);
        this.remotePeers = [];
        await this.hubConnection.invoke("GetRoomList");
      },
      handleUserJoined(connectionId, userId) {
        console.log(`User ${userId} joined with ConnectionId ${connectionId}`);
        if (connectionId !== this.hubConnection.connectionId) {
          this.createPeerConnection(connectionId);
          this.createOffer(connectionId);
        }
      },
      handleUserLeft(connectionId) {
        console.log(`User ${connectionId} left`);
        this.closePeerConnection(connectionId);
      },
      handleReceiveSignal(sendingClientId, signal) {
        if (this.peerConnections[sendingClientId]) {
          this.processAnswer(sendingClientId, signal);
        } else {
          this.processOffer(sendingClientId, signal);
        }
      },
      async createPeerConnection(connectionId) {
        const pc = new RTCPeerConnection({
          iceServers: [
            { urls: "stun:stun.l.google.com:19302" },
            { urls: "stun:stun1.l.google.com:19302" }
          ]
        });

        pc.ontrack = event => this.handleTrackEvent(event, connectionId);
        pc.onicecandidate = event => this.handleIceCandidate(event, connectionId);
        pc.oniceconnectionstatechange = () => this.handleIceStateChange(connectionId);

        this.localStream?.getTracks().forEach(track => pc.addTrack(track, this.localStream));
        this.peerConnections[connectionId] = pc;
      },
      async createOffer(connectionId) {
        try {
          const offer = await this.peerConnections[connectionId].createOffer();
          await this.peerConnections[connectionId].setLocalDescription(offer);
          this.hubConnection.invoke("SendSignal", JSON.stringify({ "sdp": offer }), connectionId);
        } catch (error) {
          console.error("Error creating offer:", error);
        }
      },
      async processOffer(connectionId, signal) {
        await this.createPeerConnection(connectionId);
        const parsedSignal = JSON.parse(signal);

        if (parsedSignal.sdp) {
          await this.peerConnections[connectionId].setRemoteDescription(new RTCSessionDescription(parsedSignal.sdp));
          const answer = await this.peerConnections[connectionId].createAnswer();
          await this.peerConnections[connectionId].setLocalDescription(answer);
          this.hubConnection.invoke("SendSignal", JSON.stringify({ "sdp": answer }), connectionId);
        }
        if (parsedSignal.ice) {
          await this.peerConnections[connectionId].addIceCandidate(new RTCIceCandidate(parsedSignal.ice));
        }
      },
      async processAnswer(connectionId, signal) {
        const parsedSignal = JSON.parse(signal);
        if (parsedSignal.sdp) {
          await this.peerConnections[connectionId].setRemoteDescription(new RTCSessionDescription(parsedSignal.sdp));
        }
        if (parsedSignal.ice) {
          await this.peerConnections[connectionId].addIceCandidate(new RTCIceCandidate(parsedSignal.ice));
        }
      },
      handleTrackEvent(event, connectionId) {
        if (!this.remotePeers.some(p => p.connectionId === connectionId)) {
          this.remotePeers.push({ connectionId });
        }
        this.$nextTick(() => {
          const audioRef = this.$refs[`remoteAudio-${connectionId}`];
          if (audioRef && audioRef[0]) {
            audioRef[0].srcObject = event.streams[0];
          } else {
            console.warn(`Audio element for ${connectionId} not found`);
          }
        });
      },
      handleIceCandidate(event, connectionId) {
        if (event.candidate) {
          this.hubConnection.invoke("SendSignal", JSON.stringify({ "ice": event.candidate }), connectionId);
        }
      },
      handleIceStateChange(connectionId) {
        if (this.peerConnections[connectionId]?.iceConnectionState === 'disconnected') {
          this.closePeerConnection(connectionId);
        }
      },
      closePeerConnection(connectionId) {
        if (this.peerConnections[connectionId]) {
          this.peerConnections[connectionId].close();
          delete this.peerConnections[connectionId];
          this.remotePeers = this.remotePeers.filter(p => p.connectionId !== connectionId);
        }
      },
      cleanupConnections() {
        this.leaveRoom();
        this.hubConnection?.stop();
      }
    }
  };
</script>

<style scoped>
  .video-call-wrapper {
    color: #e5dcdc;
    min-height: 100vh;
    background: #1d031f;
    padding: 20px;
  }

  .video-call-container {
    max-width: 800px;
    margin: 40px auto;
    padding: 25px;
    border-radius: 12px;
    background: #252525; /* Более темный и профессиональный фон */
    box-shadow: 0 6px 12px rgba(0, 0, 0, 0.3);
    border: 1px solid #333; /* Тонкая граница для четкости */
  }

  h1 {
    text-align: center;
    font-size: 2.2rem; /* Уменьшил для компактности */
    margin-bottom: 20px;
    color: #70abaf;
    font-weight: 600;
    letter-spacing: 1px; /* Добавил для профессиональности */
  }

  .nav-buttons {
    display: flex;
    justify-content: center;
    gap: 15px;
    margin-bottom: 20px;
  }

  .btn {
    padding: 10px 20px; /* Уменьшил для лаконичности */
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-size: 1rem;
    font-weight: 500;
    transition: background-color 0.2s ease, transform 0.2s ease, box-shadow 0.2s ease;
  }

  .btn-default {
    background-color: #4a4e69;
    color: #e5dcdc;
  }

    .btn-default:hover {
      background-color: #5a6268;
      transform: translateY(-1px);
      box-shadow: 0 3px 6px rgba(0, 0, 0, 0.2);
    }

  .btn-primary {
    background-color: #007bff;
    color: #fff;
  }

    .btn-primary:hover {
      background-color: #0056b3;
      transform: translateY(-1px);
      box-shadow: 0 3px 6px rgba(0, 0, 0, 0.2);
    }

  .btn-secondary {
    background-color: #6c757d;
    color: #fff;
  }

    .btn-secondary:hover {
      background-color: #5a6268;
      transform: translateY(-1px);
      box-shadow: 0 3px 6px rgba(0, 0, 0, 0.2);
    }

  .room-form, .room-content {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 15px; /* Компактное расстояние между элементами */
    margin-top: 20px;
  }

  .form-input {
    width: 100%;
    max-width: 300px; /* Ограничил ширину для аккуратности */
    padding: 10px;
    border-radius: 6px;
    border: 1px solid #444;
    background: #333;
    color: #e5dcdc;
    font-size: 1rem;
    transition: border-color 0.2s ease, box-shadow 0.2s ease;
  }

    .form-input:focus {
      border-color: #70abaf;
      box-shadow: 0 0 6px rgba(112, 171, 175, 0.4);
      outline: none;
    }

  .room-content p {
    margin: 0;
    font-size: 1.1rem;
    color: #e5dcdc;
  }

  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background-color: rgba(0, 0, 0, 0.7);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 1000;
  }

  .modal-content {
    background-color: #252525;
    padding: 20px;
    border-radius: 12px;
    box-shadow: 0 6px 12px rgba(0, 0, 0, 0.4);
    width: 100%;
    max-width: 320px; /* Уменьшил для компактности */
    text-align: center;
    color: #e5dcdc;
    border: 1px solid #333;
  }

    .modal-content h2 {
      font-size: 1.5rem;
      margin-bottom: 15px;
      color: #70abaf;
      font-weight: 500;
    }

  .modal-input {
    width: 100%;
    padding: 10px;
    margin-bottom: 15px;
    border: 1px solid #444;
    border-radius: 6px;
    background: #333;
    color: #e5dcdc;
    font-size: 1rem;
    transition: border-color 0.2s ease;
  }

    .modal-input:focus {
      border-color: #70abaf;
      outline: none;
    }

  .modal-buttons {
    display: flex;
    justify-content: space-between;
    gap: 10px;
  }

  .card-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
    gap: 25px;
    padding: 30px;
  }
</style>
