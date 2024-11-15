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
const io = socketIo(server);
const port = process.env.PORT || 5002;
require('dotenv').config();

// Middleware
app.use(cors());
app.use(upload.none());
app.use('/uploads', express.static(path.join(__dirname, 'uploads')));

const storage = multer.diskStorage({
	destination: (req, file, cb) => {
		cb(null, './uploads/');
	},
	filename: (req, file, cb) => {
		cb(null, Date.now() + path.extname(file.originalname));
	},
});

const upload = multer({ storage });

if (!fs.existsSync('./uploads')) {
	fs.mkdirSync('./uploads');
}

const users = [
	{
		username: 'admin',
		password: 'admin123',
		token: 'V3ZI8IOjLxcDRoFhUV7Y3MKQ9yodGjCjCKCP6brt4ekL5tgD0DM0QBJvQZ7CHx5O',
	},
];

const authMiddleware = (req, res, next) => {
	const token = req.headers.authorization?.split(' ')[1];
	if (!token) return res.status(401).json({ message: 'Необхідна авторизація' });

	try {
		const user = users.find(u => u.token === token);
		if (!user)
			return res.status(403).json({ message: 'Невірний або прострочений токен' });
		next();
	} catch (error) {
		res.status(403).json({ message: 'Невірний або прострочений токен' });
	}
};

// Підключення до MongoDB
mongoose
	.connect(process.env.MONGODB_URI)
	.then(() => console.log('MongoDB connected'))
	.catch(err => console.error('MongoDB connection error:', err));

// Логіка обробки зображень
let shouldStop = false;
let shouldStopID = null;

const worker = async (newImage, inputImagePath, outputImagePath) => {
	const totalSteps = 10;
	let currentStep = 0;

	const randomProgress = () => Math.floor(Math.random() * 15) + 5; // випадковий приріст від 5 до 15

	if (currentStep <= totalSteps && !shouldStop) {
		const progress = Math.min(currentStep * 10 + randomProgress(), 100); // Гарантуємо, що прогрес не перевищує 100
		io.emit('progress', { progress });
		currentStep++;
		setTimeout(() => worker(newImage, inputImagePath, outputImagePath), 100);
	} else if (shouldStop) {
		await Image.findByIdAndUpdate(newImage._id, { status: 'stopped' });
		io.emit('updateHistory');
		io.emit('progress', { status: 'stopped', progress: currentStep * 10 });
	} else {
		if (shouldStopID !== newImage._id.toString()) {
			await Image.findByIdAndUpdate(newImage._id, {
				status: 'completed',
				progress: 100,
				processedImageUrl: `/uploads/${path.basename(outputImagePath)}`,
			});
			io.emit('updateHistory');
			io.emit('progress', {
				status: 'Обробка завершена!',
				originalImageUrl: `http://localhost:${port}/uploads/${path.basename(inputImagePath)}`,
				processedImageUrl: `http://localhost:${port}/uploads/${path.basename(outputImagePath)}`,
				progress: 100,
			});
		}
	}
};

// Маршрути
app.post('/upload', authMiddleware, upload.single('image'), async (req, res) => {
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

		if (inProgressCount >= 3) {
			io.emit('error', 'Не можна створити більше 3 задач підряд');
			return res.status(400).json({ message: 'Не можна створити більше 3 задач підряд' });
		}

		io.emit('updateHistory');
		await newImage.save();

		io.emit('progress', {
			status: 'Початок обробки зображення...',
			progress: 0,
		});
		shouldStop = false;

		worker(newImage, inputImagePath, outputImagePath);

		await sharp(inputImagePath).grayscale().toFile(outputImagePath);

		res.json({ message: 'Зображення успішно завантажено' });
	} catch (error) {
		console.error('Error processing image:', error);
		res.status(500).send('Error processing image');
	}
});

// WebSocket для зупинки обробки
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

// Історія оброблених зображень
app.get('/history', authMiddleware, async (req, res) => {
	try {
		const images = await Image.find().sort({ createdAt: -1 });
		res.json(images);
	} catch (err) {
		console.error(err);
		res.status(500).send('Помилка при отриманні історії');
	}
});

// Очистити задачі
app.delete('/api/clear-tasks', authMiddleware, async (req, res) => {
	try {
		const deletedTasks = await Image.deleteMany({});
		res.status(200).json({ message: `Видалено ${deletedTasks.deletedCount} задач` });
	} catch (error) {
		console.error('Помилка очищення задач:', error);
		res.status(500).json({ message: 'Не вдалося очистити задачі.' });
	}
});

// Логін користувача
app.post('/login', (req, res) => {
	const { username, password } = req.body;

	if (!username || !password) {
		return res.status(400).send('Missing username or password');
	}

	const user = users.find(u => u.username === username && u.password === password);

	if (user) {
		return res.json({ token: user.token });
	} else {
		return res.status(401).json({ message: 'Невірний логін або пароль' });
	}
});

// Зупинка обробки зображення
app.put('/stop/:id', authMiddleware, async (req, res) => {
	const { id } = req.params;
	await Image.findByIdAndUpdate(id, { status: 'stopped' });
	io.emit('updateHistory');
});

// Запуск сервера
server.listen(port, () => {
	console.log(`Server is running on http://localhost:${port}`);
});
