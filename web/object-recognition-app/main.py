import os
import torch
from torchvision import models, transforms
from flask import Flask, request, jsonify
from PIL import Image

# Ініціалізація Flask
app = Flask(__name__)

# Завантаження попередньо навченого моделі ResNet50 з torchvision
model = models.resnet50(weights="IMAGENET1K_V1")
model.eval()

# Трансформація для підготовки зображення до введення в модель
transform = transforms.Compose([
    transforms.Resize(256),
    transforms.CenterCrop(224),
    transforms.ToTensor(),
    transforms.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225]),
])

# Функція для передбачення
def predict_image(image_path):
    # Завантаження зображення
    image = Image.open(image_path)
    image = image.convert("RGB")  # Переконатися, що зображення RGB

    # Трансформація зображення
    image_tensor = transform(image).unsqueeze(0)  # Додаємо розмір пакету

    # Виконання передбачення
    with torch.no_grad():
        outputs = model(image_tensor)

    # Отримання класу з максимальним ймовірністю
    _, predicted_class = torch.max(outputs, 1)
    return predicted_class.item()

# Маршрут для завантаження файлу
@app.route('/upload', methods=['POST'])
def upload_file():
    if 'file' not in request.files:
        return jsonify({'error': 'No file part'}), 400
    
    file = request.files['file']
    
    if file.filename == '':
        return jsonify({'error': 'No selected file'}), 400

    # Збереження файлу на сервері
    filepath = os.path.join("uploads", file.filename)
    file.save(filepath)

    # Передбачення зображення
    try:
        predicted_class = predict_image(filepath)
        return jsonify({
            'message': 'File successfully uploaded',
            'recognizedObject': predicted_class,  # Точний клас
            'fileName': file.filename,
            'fileSize': os.path.getsize(filepath)
        })
    except Exception as e:
        return jsonify({'error': str(e)}), 500

if __name__ == '__main__':
    if not os.path.exists('uploads'):
        os.makedirs('uploads')
    app.run(debug=True, host='0.0.0.0', port=5001)
