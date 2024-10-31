// Налаштування канвасів для кожної з проекцій
const canvasXY = document.getElementById('canvasXY');
const ctxXY = canvasXY.getContext('2d');
const canvasXZ = document.getElementById('canvasXZ');
const ctxXZ = canvasXZ.getContext('2d');
const canvasYZ = document.getElementById('canvasYZ');
const ctxYZ = canvasYZ.getContext('2d');

function drawSurface(ctx, projection) {
    const uSegments = 20;
    const vSegments = 20;
    const scale = 50;

    ctx.clearRect(0, 0, canvasXY.width, canvasXY.height);
    ctx.strokeStyle = 'blue';
    ctx.lineWidth = 1;

    for (let i = 0; i <= uSegments; i++) {
        for (let j = 0; j <= vSegments; j++) {
            const u = i / uSegments;
            const v = j / vSegments;

            const x = (u * 2 - 1) * 2; 
            const y = (v * 2 - 1) * 2; 
            const z = Math.sin(x) * Math.cos(y);

            let px, py;
            if (projection === 'xy') {
                px = canvasXY.width / 2 + x * scale;
                py = canvasXY.height / 2 - y * scale;
            } else if (projection === 'xz') {
                px = canvasXZ.width / 2 + x * scale;
                py = canvasXZ.height / 2 - z * scale;
            } else if (projection === 'yz') {
                px = canvasYZ.width / 2 + y * scale;
                py = canvasYZ.height / 2 - z * scale;
            }

            // Рисування точки
            ctx.beginPath();
            ctx.arc(px, py, 2, 0, 2 * Math.PI);
            ctx.fill();
        }
    }
}

drawSurface(ctxXY, 'xy'); 
drawSurface(ctxXZ, 'xz'); 
drawSurface(ctxYZ, 'yz'); 
