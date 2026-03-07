function playSound(type) {
    const audio = new Audio(`/sounds/${type}.mp3`);
    audio.play().catch(e => console.log('Audio play failed:', e));
}

// Функция для показа уведомлений
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} notification`;
    notification.textContent = message;
    document.body.appendChild(notification);

    setTimeout(() => {
        notification.remove();
    }, 3000);
}

// Инициализация маскота
document.addEventListener('DOMContentLoaded', function () {
    const mascot = document.getElementById('mascot');
    const speech = document.getElementById('mascotSpeech');

    if (mascot && speech) {
        const phrases = [
            'Привет! 👋',
            'Давай учиться! 📚',
            'Ты молодец! 🌟',
            'Попробуй еще! 💪',
            'Отлично! 🎉'
        ];

        setInterval(() => {
            speech.textContent = phrases[Math.floor(Math.random() * phrases.length)];
            speech.style.display = 'block';
            setTimeout(() => {
                speech.style.display = 'none';
            }, 3000);
        }, 8000);
    }
});