<template>
	<div>
		<v-scroll-x-reverse-transition hide-on-leave>
			<v-container v-if="!isAuthenticated">
				<div>
					<v-text-field
						v-model="username"
						label="Логін"
						outlined
					></v-text-field>
					<v-text-field
						v-model="password"
						label="Пароль"
						outlined
						type="password"
					></v-text-field>
					<v-btn @click="login" color="primary" class="mt-4" variant="plaint"
						>Увійти</v-btn
					>
				</div>
			</v-container>
		</v-scroll-x-reverse-transition>
		<v-scroll-x-reverse-transition hide-on-leave>
			<v-container class="text-center" v-if="isAuthenticated">
				<v-row>
					<v-col>
						<v-file-input
							v-model="selectedFile"
							label="Виберіть файл для завантаження"
							accept="image/*"
							prepend-icon="mdi-upload"
							outlined
							@change="checkFileSize"
						></v-file-input> </v-col
					><v-col cols="auto">
						<v-btn
							@click="logout"
							variant="plain"
							color="secondary"
							class="mt-4"
							>Вийти</v-btn
						>
					</v-col>
				</v-row>

				<v-btn
					color="primary"
					@click="uploadFile"
					:disabled="!selectedFile || loading"
					class="mt-4"
				>
					<template v-if="loading">
						<v-progress-circular indeterminate color="white" size="20" />
					</template>
					<template v-else> Upload </template>
				</v-btn>
				<v-scale-transition>
					<div v-if="progress !== null" class="mt-4">
						<v-progress-linear
							:model-value="progress"
							color="green"
							height="10"
							rounded
						></v-progress-linear>
						<p>{{ progress }}%</p>
					</div>
				</v-scale-transition>

				<div v-if="status" class="mt-4">
					<p>{{ status }}</p>
				</div>

				<v-btn
					v-if="progress !== null"
					color="error"
					@click="stopProcessing"
					:disabled="!progress || progress === 100"
					class="mt-4"
					variant="text"
				>
					Stop
				</v-btn>
				<v-scroll-x-reverse-transition>
					<div v-if="false" class="mt-6 upload-result">
						<v-row>
							<v-col cols="6">
								<v-card
									outlined
									class="d-flex flex-column align-center img__wrapper"
									height="300"
								>
									<v-card-title>Оригінальне зображення</v-card-title>
									<img
										:src="uploadResult.originalImageUrl"
										alt="Original Image"
										max-width="100%"
										max-height="300"
										class="mb-4 img"
									/>
								</v-card>
							</v-col>
							<v-col cols="6">
								<v-card
									outlined
									class="d-flex flex-column align-center img__wrapper"
									height="300"
								>
									<v-card-title>Оброблене зображення</v-card-title>
									<img
										:src="uploadResult.processedImageUrl"
										alt="Processed Image"
										max-width="100%"
										max-height="300"
										class="mb-4"
									/>
								</v-card>
							</v-col>
						</v-row>
					</div>
				</v-scroll-x-reverse-transition>

				<v-divider class="mt-6"></v-divider>
				<v-scroll-y-transition>
					<div
						class="d-flex align-center justify-center my-3"
						v-if="history.length"
					>
						<v-btn color="info" variant="plain" class="mr-2">History</v-btn>
						<div>
							<v-btn color="error" @click="handleClearAllTasks">Clear</v-btn>
						</div>
					</div>
				</v-scroll-y-transition>
				<v-row justify="start">
					<v-col v-for="item in history" :key="item._id" cols="4">
						<v-card class="mb-4">
							<div style="height: 210px">
								<v-img
									:src="`http://127.0.0.1:5001${
										item.status === 'completed'
											? item.processedImageUrl
											: item.originalImageUrl
									}`"
									alt="Processed Image"
									max-width="100%"
									max-height="300px"
									class="mb-4"
									style="height: 210px"
								/>
							</div>
							<v-card-subtitle>Status: {{ item.status }}</v-card-subtitle>
							<v-progress-linear
								:model-value="item.progress"
								:color="getColor(item)"
								height="10"
							/>
							<v-btn
								v-if="item.status !== 'processing'"
								:href="`http://127.0.0.1:5001${
									item.status === 'completed'
										? item.processedImageUrl
										: item.originalImageUrl
								}`"
								target="_blank"
								variant="plain"
								color="info"
								>View Image</v-btn
							>
							<v-btn
								v-if="item.status === 'processing'"
								color="red"
								variant="text"
								@click="stopInHistory(item)"
							>
								Зупинити обробку
							</v-btn>
						</v-card>
					</v-col>
				</v-row>
			</v-container>
		</v-scroll-x-reverse-transition>
		<v-snackbar
			v-model="snackbar.visible"
			:color="snackbar.color"
			:timeout="snackbar.timeout"
			top
			right
		>
			{{ snackbar.message }}
			<template v-slot:action>
				<v-btn color="white" text @click="snackbar.visible = false">
					Закрити
				</v-btn>
			</template>
		</v-snackbar>
	</div>
</template>

<script>
import { ref, onMounted, watch } from 'vue';
import axios from 'axios';
import io from 'socket.io-client';

export default {
	setup() {
		const selectedFile = ref(null);
		const loading = ref(false);
		const uploadResult = ref(null);
		const progress = ref(null); // Відсотковий прогрес
		const status = ref('');
		const history = ref([]);
		const socket = ref(null);
		const isAuthenticated = ref(false);
		const username = ref('');
		const password = ref('');
		const token = ref(null);
		const isFileSizeValid = ref(true);
		const MAX_FILE_SIZE = 2 * 1024 * 1024;

		const checkFileSize = () => {
			if (selectedFile.value) {
				if (selectedFile.value.size > MAX_FILE_SIZE) {
					isFileSizeValid.value = false;
					showError('Розмір файлу перевищує максимальний ліміт (2 MB)');
				} else {
					isFileSizeValid.value = true;
				}
			}
		};
		const snackbar = ref({
			visible: false,
			message: '',
			color: 'error', // дефолтний колір
			timeout: 6000,
		});
		// Метод для показу помилки
		const showError = message => {
			snackbar.value.message = message;
			snackbar.value.color = 'error';
			snackbar.value.visible = true;
		};

		// Метод для показу успіху
		const showSuccess = message => {
			snackbar.value.message = message;
			snackbar.value.color = 'success';
			snackbar.value.visible = true;
		};
		const login = async () => {
			try {
				const formData = new FormData();
				formData.append('username', username.value);
				formData.append('password', password.value);

				const response = await axios.post(
					'http://localhost:5001/login',
					formData
				);

				token.value = response.data.token;
				localStorage.setItem('authToken', token.value);
				socket.value = io('http://localhost:5001');
				fetchHistory();

				isAuthenticated.value = true;
				axios.defaults.headers.common[
					'Authorization'
				] = `Bearer ${token.value}`;
			} catch (error) {
				showError('Невірні дані для входу');
				console.error('Помилка входу:', error);
			}
		};
		watch(socket, newSocket => {
			if (newSocket) {
				newSocket.on('progress', data => {
					if (data.status) {
						status.value = data.status;
					}
					if (data.progress !== undefined) {
						progress.value = data.progress;
					}

					if (data.progress === 100) {
						uploadResult.value = {
							originalImageUrl: data.originalImageUrl,
							processedImageUrl: data.processedImageUrl,
						};
					}
				});

				newSocket.on('updateHistory', fetchHistory);
			}
		});
		if (localStorage.getItem('authToken')) {
			isAuthenticated.value = true;
			token.value = localStorage.getItem('authToken');
			socket.value = io('http://localhost:5001');
		}

		const stopProcessing = () => {
			showSuccess('Зупинено успішно');

			socket.value.emit('stopProcessing');
		};
		const stopInHistory = async item => {
			try {
				axios.defaults.headers.common[
					'Authorization'
				] = `Bearer ${localStorage.authToken}`;
				socket.value.emit('stopProcessingID', item._id);
				await axios.put(`http://127.0.0.1:5001/stop/${item._id}`);
				fetchHistory();
			} catch (e) {
				console.log(e);
			}
		};
		const fetchHistory = async () => {
			try {
				axios.defaults.headers.common[
					'Authorization'
				] = `Bearer ${localStorage.authToken}`;
				const response = await axios.get('http://127.0.0.1:5001/history');
				history.value = response.data;
			} catch (error) {
				console.error('Помилка отримання історії:', error);
			}
		};
		const handleClearAllTasks = async () => {
			try {
				axios.defaults.headers.common[
					'Authorization'
				] = `Bearer ${localStorage.authToken}`;
				await axios.delete('http://127.0.0.1:5001/api/clear-tasks');
				showSuccess('Видалено успішно');

				fetchHistory();
			} catch (error) {
				console.error('Помилка запиту на очищення задач:', error);
			}
		};

		const getColor = item => {
			if (item.status === 'processing') {
				return 'blue';
			}
			if (item.status === 'completed') {
				return 'green';
			}
			if (item.status === 'stopped') {
				return 'red';
			}
			return 'grey';
		};
		const uploadFile = async () => {
			if (!selectedFile.value || !isFileSizeValid.value) return;

			loading.value = true;
			const formData = new FormData();
			formData.append('image', selectedFile.value);

			try {
				axios.defaults.headers.common[
					'Authorization'
				] = `Bearer ${localStorage.authToken}`;

				await axios.post('http://127.0.0.1:5001/upload', formData, {
					headers: { 'Content-Type': 'multipart/form-data' },
				});

				showSuccess('Завантажено успішно');
			} catch (error) {
				console.error('Помилка завантаження файлу:', error);
				if (
					error.response &&
					error.response.data &&
					error.response.data.message
				) {
					showError(error.response.data.message); // Показуємо помилку з сервера
				} else {
					showError('Сталася помилка при завантаженні файлу!'); // Загальна помилка, якщо немає відповіді
				}
			} finally {
				loading.value = false;
			}
		};
		const logout = () => {
			localStorage.removeItem('authToken');
			isAuthenticated.value = false;
			socket.value.disconnect(); // Відключаємо сокет при виході
		};

		if (localStorage.getItem('authToken')) {
			isAuthenticated.value = true;
		}
		onMounted(() => {
			if (isAuthenticated.value) {
				fetchHistory();
			}
		});

		return {
			selectedFile,
			uploadFile,
			loading,
			uploadResult,
			progress,
			status,
			stopProcessing,
			history,
			getColor,
			handleClearAllTasks,
			stopInHistory,
			snackbar,
			isAuthenticated,
			login,
			logout,
			username,
			password,
			checkFileSize,
		};
	},
};
</script>

<style lang="scss" scoped>
img {
	object-fit: contain;
}
</style>
