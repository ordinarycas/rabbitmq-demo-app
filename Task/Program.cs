using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;  // 引用此命名空間
using Microsoft.Extensions.Configuration;
using TaskApp.Services;

var builder = WebApplication.CreateBuilder(args);

// 註冊服務
builder.Services.AddControllers();  // 註冊 API 控制器
builder.Services.AddSingleton<RabbitMQService>();  // 註冊 RabbitMQ 服務

var port = builder.Configuration.GetValue<int>("AppSettings:Port", 5000);
var app = builder.Build();

// 允許靜態文件服務，讓 HTML 頁面能夠被訪問
app.UseDefaultFiles();  // 設定預設檔案名稱 (通常是 index.html)
app.UseStaticFiles();   // 讓靜態文件可以被提供

// 設置路由
app.UseRouting();

// 設定 API 路由
app.MapControllerRoute(
    name: "task-api",
    pattern: "api/v1/task/{action}",
    defaults: new { controller = "Task" });

// 啟動應用程式
app.Run($"http://0.0.0.0:{port}");  // 动态绑定端口

