from fastapi import FastAPI, File, UploadFile, BackgroundTasks
from pydantic import BaseModel
from uuid import uuid4
import tensorflow as tf
from PIL import Image
from fastapi.middleware.cors import CORSMiddleware
import asyncio

app = FastAPI()

# Завантаження попередньо навченої моделі
model = tf.keras.applications.MobileNetV2(weights="imagenet")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # або вкажіть конкретне походження, наприклад, ["http://localhost:8080"]
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Словник для зберігання статусів задач
task_statuses = {}

# Функція для обробки зображення та розпізнавання об'єктів
def process_image_task(task_id: str, file: UploadFile):
    try:
        
        # Змінюємо статус задачі на "Обробка"
        task_statuses[task_id] = {"status": "Processing", "progress": 50.0, "result": None}
        
        # Читання та обробка зображення
        image = Image.open(file.file)
        image = image.resize((224, 224))  # Змінюємо розмір зображення до необхідного
        img_array = tf.keras.preprocessing.image.img_to_array(image)
        img_array = tf.expand_dims(img_array, 0)  # Додаємо batch-розмір

        # Розпізнавання об'єктів
        predictions = model.predict(img_array)
        decoded_predictions = tf.keras.applications.mobilenet_v2.decode_predictions(predictions, top=3)[0]

        # Форматування результатів
        result = ", ".join([f"{pred[1]} ({pred[2]*100:.2f}%)" for pred in decoded_predictions])
        
        # Оновлення статусу задачі до "Завершено"
        task_statuses[task_id] = {"status": "Completed", "progress": 100.0, "result": result}
    
    except Exception as e:
        # У разі помилки оновлюємо статус задачі
        task_statuses[task_id] = {"status": "Failed", "progress": 100.0, "result": str(e)}

# Завантаження зображення для розпізнавання
@app.post("/upload")
async def upload_image(background_tasks: BackgroundTasks, file: UploadFile = File(...)):
    if file.file._file.size > 10 * 1024 * 1024:  # Обмеження в 10 МБ
        return {"error": "File too large. Maximum size is 10MB."}
    task_id = str(uuid4())
    # Ініціалізація статусу задачі як "В очікуванні"
    task_statuses[task_id] = {"status": "Pending", "progress": 0.0, "result": None}
    
    # Додавання задачі для обробки у фоновий процес
    background_tasks.add_task(process_image_task, task_id, file)

    # Чекаємо завершення обробки
    while task_statuses[task_id]["status"] == "Pending":
        await asyncio.sleep(1)  # Затримка для уникнення блокування

    # Повертаємо результат одразу після завершення обробки
    return task_statuses[task_id]

# Перевірка статусу задачі
@app.get("/status/{task_id}")
def check_status(task_id: str):
    # Повертаємо статус задачі
    status = task_statuses.get(task_id, {"status": "Not found"})
    return status
