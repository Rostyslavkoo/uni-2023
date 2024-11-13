const path = require('path');
const fs = require('fs');
const sharp = require('sharp');
const express = require('express');
const multer = require('multer');
const cors = require('cors');
const http = require('http');
const socketIo = require('socket.io');
const mongoose = require('mongoose');
const Image = require('./image_model');

const app = express();
const server = http.createServer(app);
const io = socketIo(server); // Ініціалізація Socket.IO на сервері
const port = 5001;
require('dotenv').config();
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

mongoose
	.connect(process.env.MONGODB_URI)
	.then(() => console.log('MongoDB connected'))
	.catch(err => console.error('MongoDB connection error:', err));

let shouldStop = false;
let shouldStopID = null; // Флаг для зупинки прогресу

// Обробка запиту на завантаження зображення
app.post('/upload', upload.single('image'), async (req, res) => {
	if (!req.file) {
		return res.status(400).send('No file uploaded');
	}

	const inputImagePath = req.file.path;
	const outputImagePath = `./uploads/processed-${req.file.filename}`;

	try {
		const newImage = new Image({
			originalImageUrl: `/uploads/${req.file.filename}`,
			status: 'processing',
			progress: 0,
		});
		const inProgressCount = await Image.countDocuments({ status: 'processing' });

		if (inProgressCount >= 5) {
			io.emit('error', 'Не можна створити більше 5 задач підряд');
			return res.status(400).json({ message: 'Не можна створити більше 5 задач підряд' });
		}

		// io.emit('updateHistory');
		await newImage.save();

		io.emit('progress', {
			status: 'Початок обробки зображення...',
			progress: 0,
		});
		shouldStop = false;
		// Симулюємо затримку для прогресу
		const totalSteps = 10; // Кількість кроків для обробки
		let currentStep = 0;

		// Функція для симуляції затримки та оновлення прогресу
		const simulateProgress = async () => {
			if (currentStep <= totalSteps && !shouldStop) {
				const randomProgress = Math.floor(Math.random() * 15) + 5; // випадковий приріст від 5 до 15
				let progress = Math.min(currentStep * 10 + randomProgress, 100); // Гарантуємо, що прогрес не перевищує 100
				io.emit('progress', { progress });

				currentStep++;
				setTimeout(simulateProgress, 70); 
			} else if (shouldStop) {
				await Image.findByIdAndUpdate(newImage._id, { status: 'stopped' });
				io.emit('updateHistory');

				io.emit('progress', {
					status: 'stopped',
					progress: currentStep * 10,
				});
			} else {
				if (shouldStopID !== newImage._id.toString()) {
					console.log(123)
					console.log(shouldStopID)
					console.log(newImage._id.toString())
					await Image.findByIdAndUpdate(newImage._id, {
						status: 'completed',
						progress: 100,
						processedImageUrl: `/uploads/${path.basename(outputImagePath)}`,
					});
					io.emit('updateHistory');

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
	socket.on('stopProcessing', id => {
		shouldStop = true;
		console.log('Обробку зупинено');
	});
	socket.on('stopProcessingID', id => {
		shouldStopID = id;
		console.log('Обробку зупинено');
	});
});

// Статичні файли для доступу до зображень
app.use('/uploads', express.static(path.join(__dirname, 'uploads')));

app.get('/history', async (req, res) => {
	try {
		const images = await Image.find().sort({ createdAt: -1 }); // Отримуємо всі зображення, сортуємо за датою створення
		res.json(images);
	} catch (err) {
		console.error(err);
		res.status(500).send('Помилка при отриманні історії');
	}
});
app.delete('/api/clear-tasks', async (req, res) => {
	try {
		const deletedTasks = await Image.deleteMany({});
		res.status(200).json({
			message: `Видалено ${deletedTasks.deletedCount} задач`,
		});
	} catch (error) {
		console.error('Помилка очищення задач:', error);
		res.status(500).json({ message: 'Не вдалося очистити задачі.' });
	}
});

app.put('/stop/:id', async (req, res) => {
	const { id } = req.params;
	await Image.findByIdAndUpdate(id, { status: 'stopped' });
	io.emit('updateHistory');
});
// Запуск сервера
server.listen(port, () => {
	console.log(`Server is running on http://localhost:${port}`);
});
