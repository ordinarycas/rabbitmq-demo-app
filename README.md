# RabbitMQ 功能演示

除了官方基礎的 RabbitMQ 教學，實作了 Worker 模式使用 WebSocket 監看 Worker 接收的消息。

## 流程圖

### 圖 1：請求流程

```mermaid
graph TD;
    A(開始) --> B(第三方請求)
    B --> C(接收請求 Task)
    C --> D(RabbitMQ)
    D --> E(接收 Queue 後的請求 Worker)
    E --> F(第三方應用)
    F --> G(結束)
```

### 圖 2：系統架構

```mermaid
graph LR;
    subgraph 客戶端
        A1(使用者) -->|請求 API| A2(Web 服務)
    end
    subgraph 伺服器
        A2 -->|發送訊息| B1(RabbitMQ)
        B1 -->|接收並處理| B2(Worker)
        B2 -->|發送結果| C1(WebSocket)
    end
    subgraph WebSocket 用戶
        C1 --> D1(客戶端應用)
    end

```

### 圖 3：程式架構

```mermaid
graph TD;
    A(Program.cs) --> B(WebSocketController.cs)
    A --> C(MessageService.cs)
    C --> D(RabbitMQ 連接)
    B --> E(管理 WebSocket 連線)
    C --> F(處理 MQ 訊息)
    E --> G(WebSocket 廣播)
```

### 圖 4：應用程式架構

```mermaid
graph TD;
    A(前端應用) -->|發送 WebSocket 請求| B(後端 WebSocket 服務)
    A -->|發送 HTTP 請求| C(後端 API)
    C -->|發送至 RabbitMQ| D(MessageQueue)
    D -->|處理訊息| E(Worker)
    E -->|發送結果| B
    B -->|更新 UI| A
```

## 開始使用(Getting Started)

執行 Worker docker-compose 包含 RabbitMQ

```bash
docker-compose up -d --scale worker=1
```

執行完後先關掉 Worker container
![Worker container](https://github.com/ordinarycas/rabbitmq-demo-app/blob/main/Demo/docker-container-woker.png)

網址輸入 http://localhost:15672 登入RabbitMQ
![登入RabbitMQ](https://github.com/ordinarycas/rabbitmq-demo-app/blob/main/Demo/RabbitMQ-queues.png)

再執行 Task 網址輸入 http://localhost:5000 可以先發送10個請求
![Task](https://github.com/ordinarycas/rabbitmq-demo-app/blob/main/Demo/task.jpeg)

RabbitMQ 的 Messages 10 就有10筆正在 Ready
![RabbitMQ Ready](https://github.com/ordinarycas/rabbitmq-demo-app/blob/main/Demo/RabbitMQ-queues-ready.png)

可以再啟動 Worker container 就會把10筆消化了
![Worker container](https://github.com/ordinarycas/rabbitmq-demo-app/blob/main/Demo/docker-container-woker-1.png)

試試多一點 worker 可以下
```bash
docker-compose up -d --scale worker=5
```

![Worker container](https://github.com/ordinarycas/rabbitmq-demo-app/blob/main/Demo/docker-container-woker-all.png)


讓 Task 次數多一些，如：10000次

![Task](https://github.com/ordinarycas/rabbitmq-demo-app/blob/main/Demo/task-100000.png)


可以看到 worker 平均分散掉了請求
![Worker container](https://github.com/ordinarycas/rabbitmq-demo-app/blob/main/Demo/docker-container-woker-1-and-woker-2.png)


### 環境需求(Prerequisites)
- Visual Studio 2022 or later
- Basic knowledge of C#
- .NET 8
- Docker

## 貢獻(Contributing)

Contributions are welcome! Please fork the repository and create a pull request with your changes.

歡迎貢獻！請 fork 這個倉庫並創建 pull request 提交你的更改。
