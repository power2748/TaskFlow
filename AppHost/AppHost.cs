var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL
var postgres = builder.AddPostgres("taskflow-db")
    .WithPgAdmin().WithDataVolume(); // удобная UI для БД в Dashboard

var authDb = postgres.AddDatabase("authdb");
var taskDb = postgres.AddDatabase("taskdb");

// Сервисы
var authService = builder.AddProject<Projects.AuthService>("authservice")
                     .WithHttpEndpoint(port: 5001, name: "http") // <-- Фиксируем порт бэкенда!
                     .WithReference(authDb);

var taskService = builder.AddProject<Projects.TaskService>("taskservice")
    .WithHttpEndpoint(port: 5002, name: "http") // <-- Фиксируем порт бэкенда!
    .WithReference(taskDb)
    .WithReference(authDb);

// Мы жестко говорим Aspire: "Всегда запускай Gateway на порту 5000 для HTTP"
var gateway = builder.AddProject<Projects.Gateway>("gateway")
                     .WithHttpEndpoint(port: 5000, name: "http") // <-- Фиксируем порт шлюза!
                     .WithReference(authService);

// Передаем фронтенду
builder.AddProject<Projects.BlazorFrontend>("blazorfrontend")
       .WithReference(gateway);

builder.Build().Run();
