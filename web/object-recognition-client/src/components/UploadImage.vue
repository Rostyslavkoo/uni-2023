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
      <template v-else>
        Завантажити
      </template>
    </v-btn>

    <!-- Виведення результату завантаження -->
    <div v-if="uploadResult" class="mt-4">
      <h5>Результат завантаження:</h5>
      <p><strong>Повідомлення:</strong> {{ uploadResult.message }}</p>
      <p><strong>Розпізнаний об'єкт:</strong> {{ uploadResult.recognizedObject }}</p>
      <p><strong>Ім'я файлу:</strong> {{ uploadResult.fileName }}</p>
      <p><strong>Розмір файлу:</strong> {{ uploadResult.fileSize }} байт</p>
    </div>
  </v-container>
</template>

<script>
import { ref } from 'vue';
import axios from 'axios';

export default {
  setup() {
    const selectedFile = ref(null);
    const loading = ref(false);
    const uploadResult = ref(null); // Для зберігання результату завантаження

    const uploadFile = async () => {
      if (!selectedFile.value) return;

      loading.value = true;
      const formData = new FormData();
      formData.append('file', selectedFile.value);

      try {
        const response = await axios.post("http://127.0.0.1:5001/upload", formData, {
          headers: { 
            "Content-Type": "multipart/form-data",
          },
        });
        
        // Зберігаємо результат завантаження
        uploadResult.value = {
          message: response.data.message,
          recognizedObject: response.data.recognizedObject, // Отримуємо назву об'єкта
          fileName: selectedFile.value.name,
          fileSize: selectedFile.value.size,
        };
      } catch (error) {
        console.error('Помилка завантаження файлу:', error);
        // alert('Не вдалося завантажити файл.');
      } finally {
        loading.value = false;
      }
    };

    return {
      selectedFile,
      uploadFile,
      loading,
      uploadResult, // Додаємо до повернення
    };
  },
};
</script>
