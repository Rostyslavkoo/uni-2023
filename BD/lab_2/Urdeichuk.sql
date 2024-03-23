CREATE TABLE IF NOT EXISTS Users (
    user_id SERIAL PRIMARY KEY,
    name VARCHAR(50),
    surname VARCHAR(50),
    date_of_birth TIMESTAMP,
    location VARCHAR(100),
    phone VARCHAR(15),
    email VARCHAR(100)
);

CREATE TABLE IF NOT EXISTS Attached_Files (
    file_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES Users(user_id),
    file_type VARCHAR(20),
    date_time TIMESTAMP,
    url VARCHAR(200),
    message TEXT
);

CREATE TABLE IF NOT EXISTS Commentss (
    comment_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES Users(user_id),
    file_id INT REFERENCES Attached_Files(file_id),
    date_time TIMESTAMP,
    comment_text TEXT
);

INSERT INTO Users (name, surname, date_of_birth, location, phone, email)
VALUES ('Іван', 'Петров', '1990-01-01', 'Київ, Україна', '1234567890', 'ivan@example.com');

INSERT INTO Attached_Files (user_id, file_type, date_time, url, message)
VALUES (1, 'image', NOW(), 'http://example.com/image.jpg', 'Це зображення котика.');

INSERT INTO Commentss (user_id, file_id, date_time, comment_text)
VALUES (1, 1, NOW(), 'Чудове зображення! Мені сподобалось.');


UPDATE Commentss SET comment_text = 'круто' WHERE comment_id = 11;

DELETE FROM Commentss WHERE comment_text = 'не круто';

SELECT * FROM Commentss


