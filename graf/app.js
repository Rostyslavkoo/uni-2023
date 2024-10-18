const canvas = document.getElementById('animationCanvas');
const ctx = canvas.getContext('2d');

const wrapper = document.querySelector('.canvas-wrapper');
canvas.width = wrapper.offsetWidth;
canvas.height = wrapper.offsetHeight;

let time = 0;
let circleCount = 10; 
let animationSpeed = 0.01;

const DEFAULT = {
    color: '#ff9494',
    animationSpeed: 0.01,
    circleCount: 10,
    time: 0
}
let color = DEFAULT.color; 
const maxRadius = Math.min(canvas.width, canvas.height) / 3; 

const circleCountInput = document.getElementById('circleCount');
const speedInput = document.getElementById('speed');
const colorInput = document.getElementById('circleColor');

function getColor(hue) {
    return `hsl(${hue % 360}, 100%, 50%)`;
}

function draw() {
    ctx.clearRect(0, 0, canvas.width, canvas.height); 

    const centerX = canvas.width / 2;
    const centerY = canvas.height / 2;

    for (let i = 0; i < circleCount; i++) {
        const radius = (i + 1) * (maxRadius / circleCount) * (Math.sin(time + i) + 1.5);
        const dynamicColor = getColor((time * 50 + i * 30) % 360);
        ctx.beginPath();
        ctx.arc(centerX, centerY, radius, 0, Math.PI * 2);
        ctx.strokeStyle = colorInput.value === DEFAULT.color ? dynamicColor : colorInput.value;
        ctx.lineWidth = 5;
        ctx.stroke();
    }

    time += animationSpeed;

    requestAnimationFrame(draw); 
}

draw(); 

circleCountInput.addEventListener('input', (e) => {
    circleCount = parseInt(e.target.value, 10);
});

speedInput.addEventListener('input', (e) => {
    animationSpeed = parseFloat(e.target.value);
});

window.addEventListener('resize', () => {
    canvas.width = wrapper.offsetWidth;
    canvas.height = wrapper.offsetHeight;
});
function resetFilters() {
    circleCount = DEFAULT.circleCount;
    animationSpeed = DEFAULT.animationSpeed;
    colorInput.value = DEFAULT.color;

    circleCountInput.value = DEFAULT.circleCount;
    speedInput.value = DEFAULT.animationSpeed;
}

resetButton.addEventListener('click', resetFilters);
