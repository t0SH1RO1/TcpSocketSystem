# TcpSocketSystem

Клієнт-серверна система на основі TCP-сокетів із використанням C# та .NET.  
Сервер обробляє команди (`PING`, `CAR <brand>`, `LOGOUT`) від клієнта та відповідає, звертаючись до публічного автомобільного API.

---

# Архітектура

Система складається з трьох проектів:

- **TcpSocketServer**
    - Обробляє TCP-з'єднання.
    - Парсить команди.
    - Використовує `CarApiService` для запитів до зовнішнього API.

- **TcpSocketClient**
    - Підключається до сервера.
    - Відправляє команди вручну або автоматично.

- **TcpSocketServer.Tests**
    - Unit-тести для серверної логіки, зокрема `CommandHandler`.

---

# Збірка

Побудова всіх проєктів

`dotnet build`

Запуск сервера

`cd TcpSocketServer`
`dotnet run`

Запуск клієнта

`cd TcpSocketClient`
`dotnet run -- --host localhost --port 5000`

Тестування

`cd TcpSocketServer.Tests`
`dotnet test`
