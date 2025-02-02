using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Worker.Services;
using Worker.Controllers;

var builder = WebApplication.CreateBuilder(args);

// 加载 appsettings.json 或环境变量中的配置
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// 註冊服務
builder.Services.AddSingleton<WebSocketService>();
builder.Services.AddSingleton<MessageService>();
builder.Services.AddControllers();

// 设置端口和 RabbitMQ 配置
var port = builder.Configuration.GetValue<int>("WebSocket:Port", 5000);

var app = builder.Build();

// 使用 WebSocket
app.UseWebSockets();

// 提供靜態文件 (index.html)
app.UseDefaultFiles();  // 設定預設檔案名稱 (通常是 index.html)
app.UseStaticFiles();   // 讓靜態文件可以被提供

// 設置路由
app.UseRouting();
app.MapControllers();

// 啟動 RabbitMQ 消費者
var messageService = app.Services.GetRequiredService<MessageService>();
messageService.StartListening();

app.Run($"http://0.0.0.0:{port}");  // 动态绑定端口
