const mongoose = require('mongoose');

// Схема для збереження метаданих зображення
const imageSchema = new mongoose.Schema({
  originalImageUrl: {
    type: String,
    required: true,
  },
  processedImageUrl: {
    type: String,
    required: false,
  },
  status: {
    type: String,
    enum: ['processing', 'completed', 'stopped'],
    default: 'processing',
  },
  progress: {
    type: Number,
    default: 0, // прогрес виконання задачі в відсотках
  },
  createdAt: {
    type: Date,
    default: Date.now,
  },
});

const Image = mongoose.model('Image', imageSchema);

module.exports = Image;
