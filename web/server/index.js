const path = require('path');
const fs = require('fs');
const sharp = require('sharp');
const express = require('express');
const multer = require('multer');
const cors = require('cors');
const http = require('http');
const socketIo = require('socket.io');

const app = express();
const server = http.createServer(app);
const io = socketIo(server); // Ініціалізація Socket.IO на сервері
const port = 5001;

// Дозволяємо всі запити з будь-якого джерела
app.use(cors());

// Налаштування для multer (збереження файлів на сервері)
const storage = multer.diskStorage({
	destination: (req, file, cb) => {
		cb(null, './uploads/');
	},
	filename: (req, file, cb) => {
		cb(null, Date.now() + path.extname(file.originalname));
	},
});

const upload = multer({ storage });

// Створюємо каталог для завантажених файлів, якщо його ще немає
if (!fs.existsSync('./uploads')) {
	fs.mkdirSync('./uploads');
}

let shouldStop = false; // Флаг для зупинки прогресу

// Обробка запиту на завантаження зображення
app.post('/upload', upload.single('image'), async (req, res) => {
	if (!req.file) {
		return res.status(400).send('No file uploaded');
	}

	const inputImagePath = req.file.path;
	const outputImagePath = `./uploads/processed-${req.file.filename}`;

	try {
		// Інформуємо клієнта через WebSocket про початок обробки
		io.emit('progress', {
			status: 'Початок обробки зображення...',
			progress: 0,
		});
		shouldStop = false;
		// Симулюємо затримку для прогресу
		const totalSteps = 10; // Кількість кроків для обробки
		let currentStep = 0;

		// Функція для симуляції затримки та оновлення прогресу
		const simulateProgress = () => {
			if (currentStep <= totalSteps && !shouldStop) {
				const randomProgress = Math.floor(Math.random() * 15) + 5; // випадковий приріст від 5 до 15
                let progress = Math.min(currentStep * 10 + randomProgress, 100); // Гарантуємо, що прогрес не перевищує 100
				io.emit('progress', { progress });

				currentStep++;
				setTimeout(simulateProgress, 70); // Затримка між кроками
			} else if (shouldStop) {
				io.emit('progress', {
					status: 'Обробку зупинено!',
					progress: currentStep * 10,
				});
			} else {
				io.emit('progress', {
					status: 'Обробка завершена!',
					originalImageUrl: `http://localhost:${port}/uploads/${path.basename(
						inputImagePath
					)}`,
					processedImageUrl: `http://localhost:${port}/uploads/${path.basename(
						outputImagePath
					)}`,
					progress: 100,
				});
			}
		};

		// Симулюємо прогрес
		simulateProgress();

		// Обробка зображення (змінюємо розмір і робимо чорно-білим)
		await sharp(inputImagePath)
			.grayscale() // Змінюємо зображення на чорно-біле
			.toFile(outputImagePath);

		// Відповідь з результатами
		res.json({
			message: 'Зображення успішно завантажено',
		});
	} catch (error) {
		console.error('Error processing image:', error);
		res.status(500).send('Error processing image');
	}
});

// Подія для зупинки обробки
io.on('connection', socket => {
	socket.on('stopProcessing', () => {
		shouldStop = true; // Зупиняємо обробку
		console.log('Обробку зупинено');
	});
});

// Статичні файли для доступу до зображень
app.use('/uploads', express.static(path.join(__dirname, 'uploads')));

// Запуск сервера
server.listen(port, () => {
	console.log(`Server is running on http://localhost:${port}`);
});
