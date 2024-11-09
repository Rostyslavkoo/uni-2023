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
			<template v-else> Завантажити </template>
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
			Зупинити обробку
		</v-btn>

		<div v-if="uploadResult" class="mt-6">
			<v-row>
				<v-col cols="6">
					<v-card outlined class="d-flex flex-column align-center img__wrapper">
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
					<v-card outlined class="d-flex flex-column align-center img__wrapper">
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
	</v-container>
</template>

<script>
import { ref } from 'vue';
import axios from 'axios';
import io from 'socket.io-client';

export default {
	setup() {
		const selectedFile = ref(null);
		const loading = ref(false);
		const uploadResult = ref(null);
		const progress = ref(null); // Відсотковий прогрес
		const status = ref('');
		const socket = io('http://localhost:5001'); // Підключення до WebSocket сервера

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

		// Зупинка обробки
		const stopProcessing = () => {
			socket.emit('stopProcessing');
		};

		const uploadFile = async () => {
			if (!selectedFile.value) return;

			loading.value = true;
			const formData = new FormData();
			formData.append('image', selectedFile.value);

			try {
				await axios.post(
					'http://127.0.0.1:5001/upload',
					formData,
					{
						headers: { 'Content-Type': 'multipart/form-data' },
					}
				);
			} catch (error) {
				console.error('Помилка завантаження файлу:', error);
			} finally {
				loading.value = false;
			}
		};

		return {
			selectedFile,
			uploadFile,
			loading,
			uploadResult,
			progress,
			status,
			stopProcessing,
		};
	},
};
</script>
