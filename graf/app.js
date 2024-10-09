const canvas = document.getElementById('animationCanvas');
const ctx = canvas.getContext('2d');

// Налаштування розмірів канви відповідно до врапера
const wrapper = document.querySelector('.canvas-wrapper');
canvas.width = wrapper.offsetWidth;
canvas.height = wrapper.offsetHeight;

let time = 0;
let circleCount = 10;  // Кількість кіл
let animationSpeed = 0.01; // Швидкість анімації

const DEFAULT = {
    color: '#ff9494',
    animationSpeed: 0.01,
    circleCount: 10,
    time: 0
}
let color = DEFAULT.color; // Початковий колір
const maxRadius = Math.min(canvas.width, canvas.height) / 3; // Максимальний радіус

// Вибір елементів управління
const circleCountInput = document.getElementById('circleCount');
const speedInput = document.getElementById('speed');
const colorInput = document.getElementById('circleColor');

// Функція для створення кольорів в HSL форматі
function getColor(hue) {
    return `hsl(${hue % 360}, 100%, 50%)`;
}

function draw() {
    ctx.clearRect(0, 0, canvas.width, canvas.height); // Очищуємо попереднє зображення

    // Центр канви
    const centerX = canvas.width / 2;
    const centerY = canvas.height / 2;

    // Малюємо концентричні кола
    for (let i = 0; i < circleCount; i++) {
        const radius = (i + 1) * (maxRadius / circleCount) * (Math.sin(time + i) + 1.5);
        const dynamicColor = getColor((time * 50 + i * 30) % 360); // Кольори змінюються
        ctx.beginPath();
        ctx.arc(centerX, centerY, radius, 0, Math.PI * 2);
        ctx.strokeStyle = colorInput.value === DEFAULT.color ? dynamicColor : colorInput.value;
        ctx.lineWidth = 5;
        ctx.stroke();
    }

    time += animationSpeed; // Зміна часу для анімації

    requestAnimationFrame(draw); // Запускаємо наступний кадр анімації
}

draw(); // Запускаємо анімацію

// Обробка змін кількості кіл
circleCountInput.addEventListener('input', (e) => {
    circleCount = parseInt(e.target.value, 10);
});

// Обробка змін швидкості
speedInput.addEventListener('input', (e) => {
    animationSpeed = parseFloat(e.target.value);
});

// Обробка зміни розміру вікна
window.addEventListener('resize', () => {
    canvas.width = wrapper.offsetWidth;
    canvas.height = wrapper.offsetHeight;
});
function resetFilters() {
    circleCount = DEFAULT.circleCount;
    animationSpeed = DEFAULT.animationSpeed;
    colorInput.value = DEFAULT.color;

    // Оновлюємо значення на інтерфейсі
    circleCountInput.value = DEFAULT.circleCount;
    speedInput.value = DEFAULT.animationSpeed;
}

resetButton.addEventListener('click', resetFilters);
