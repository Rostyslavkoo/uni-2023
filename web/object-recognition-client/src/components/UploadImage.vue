<template>
	<v-container class="text-center">
		<v-file-input
			v-model="selectedFile"
			label="Виберіть файл для завантаження"
			accept="image/*"
			prepend-icon="mdi-upload"
			outlined
		></v-file-input>
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

		<div v-if="progress !== null" class="mt-4">
			<v-progress-linear
				:model-value="progress"
				color="green"
				height="20"
			></v-progress-linear>
			<p>{{ progress }}%</p>
		</div>

		<div v-if="status" class="mt-4">
			<p>{{ status }}</p>
		</div>

		<v-btn
			v-if="progress !== null"
			color="red"
			@click="stopProcessing"
			:disabled="!progress || progress === 100"
			class="mt-4"
		>
			Stop
		</v-btn>

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
		<v-divider class="mt-6"></v-divider>
		<div class="d-flex align-center justify-center" v-if="history.length">
			<v-card-title> History </v-card-title>
			<div>
				<v-btn color="error" @click="handleClearAllTasks">Clear</v-btn>
			</div>
		</div>
		<v-row justify="space-around">
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
	</v-container>
</template>

<script>
import { ref, onMounted } from 'vue';
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
		const socket = io('http://localhost:5001'); // Підключення до WebSocket сервера
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
		socket.on('progress', data => {
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

		const stopProcessing = () => {
			showSuccess('Зупинено успішно');

			socket.emit('stopProcessing');
		};
		const stopInHistory = async item => {
			try {
				socket.emit('stopProcessingID', item._id);
				await axios.put(`http://127.0.0.1:5001/stop/${item._id}`);
				fetchHistory();
			} catch (e) {
				console.log(e);
			}
		};
		const fetchHistory = async () => {
			try {
				const response = await axios.get('http://127.0.0.1:5001/history');
				history.value = response.data;
			} catch (error) {
				console.error('Помилка отримання історії:', error);
			}
		};
		const handleClearAllTasks = async () => {
			try {
				await fetch('http://localhost:5001/api/clear-tasks', {
					method: 'DELETE',
				});
				showSuccess('Видалено успішно');

				fetchHistory();
			} catch (error) {
				console.error('Помилка запиту на очищення задач:', error);
			}
		};

		socket.on('updateHistory', fetchHistory);

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
			if (!selectedFile.value) return;

			loading.value = true;
			const formData = new FormData();
			formData.append('image', selectedFile.value);

			try {
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
		onMounted(() => {
			fetchHistory();
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
		};
	},
};
</script>

<style lang="scss" scoped>
img {
	object-fit: contain;
}
</style>
