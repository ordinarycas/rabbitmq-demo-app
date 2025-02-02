# Worker 設定與測試指南

這個專案包含了一個 Worker 程式，能夠監聽 RabbitMQ 訊息佇列並透過 WebSocket 廣播訊息。如果 WebSocket 連線中斷，訊息會被暫存並在有 WebSocket 連線時進行發送。

## 步驟 1: 環境設定

1. **安裝 RabbitMQ**  
   若尚未安裝 RabbitMQ，請依照以下步驟安裝：
   - [RabbitMQ 安裝指南](https://www.rabbitmq.com/download.html)

   若是本地測試，可以使用 Docker 快速啟動 RabbitMQ：
   ```bash
   docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management

2. **安裝 .NET SDK**
確保已經安裝 .NET SDK 6.0 或以上版本。

建立與設定專案
複製或克隆專案的程式碼，並進入專案目錄：


## 資料夾結構

```
MyRabbitMQWebApp/
├── Controllers/
│   └── WebSocketController.cs        # WebSocket 控制器，處理 WebSocket 連接
├── Services/
│   └── MessageService.cs             # 處理 RabbitMQ 的消息服務
├── wwwroot/                          # 靜態文件夾，存放 HTML、CSS、JS 等
│   └── index.html                    # 前端頁面，顯示 WebSocket 接收到的消息
├── Program.cs                        # 主要入口，配置和啟動服務
├── MyRabbitMQWebApp.csproj           # 專案檔
└── appsettings.json                  # 配置檔（如果需要，可以用於 RabbitMQ 配置等）
```