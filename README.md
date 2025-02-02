# RabbitMQ �\��t��

���F�x���¦�� RabbitMQ �оǡA��@�F Worker �Ҧ��ϥ� WebSocket �ʬ� Worker �����������C

## �y�{��

### �� 1�G�ШD�y�{

```mermaid
graph TD;
    A(�}�l) --> B(�ĤT��ШD)
    B --> C(�����ШD Task)
    C --> D(RabbitMQ)
    D --> E(���� Queue �᪺�ШD Worker)
    E --> F(�ĤT������)
    F --> G(����)
```

### �� 2�G�t�ά[�c

```mermaid
graph LR;
    subgraph �Ȥ��
        A1(�ϥΪ�) -->|�ШD API| A2(Web �A��)
    end
    subgraph ���A��
        A2 -->|�o�e�T��| B1(RabbitMQ)
        B1 -->|�����óB�z| B2(Worker)
        B2 -->|�o�e���G| C1(WebSocket)
    end
    subgraph WebSocket �Τ�
        C1 --> D1(�Ȥ������)
    end

```

### �� 3�G�{���[�c

```mermaid
graph TD;
    A(Program.cs) --> B(WebSocketController.cs)
    A --> C(MessageService.cs)
    C --> D(RabbitMQ �s��)
    B --> E(�޲z WebSocket �s�u)
    C --> F(�B�z MQ �T��)
    E --> G(WebSocket �s��)
```

### �� 4�G���ε{���[�c

```mermaid
graph TD;
    A(�e������) -->|�o�e WebSocket �ШD| B(��� WebSocket �A��)
    A -->|�o�e HTTP �ШD| C(��� API)
    C -->|�o�e�� RabbitMQ| D(MessageQueue)
    D -->|�B�z�T��| E(Worker)
    E -->|�o�e���G| B
    B -->|��s UI| A
```

## �}�l�ϥ�(Getting Started)

���� Worker docker-compose �]�t RabbitMQ

```bash
docker-compose up -d --scale worker=1
```

���槹������� Worker container

���}��J http://localhost:15672 �n�JRabbitMQ

�A���� Task ���}��J http://localhost:5000 �i�H���o�e10�ӽШD

RabbitMQ �� Messages 10 �N��10�����b Ready

�i�H�A�Ұ� Worker container �N�|��10�����ƤF

�ոզh�@�I worker �i�H�U
```bash
docker-compose up -d --scale worker=5
```

�� Task ���Ʀh�@�ǡA�p�G10000��

�i�H�ݨ� worker �����������F�ШD


### ���һݨD(Prerequisites)
- Visual Studio 2022 or later
- Basic knowledge of C#
- .NET 8
- Docker

## �^�m(Contributing)

Contributions are welcome! Please fork the repository and create a pull request with your changes.

�w��^�m�I�� fork �o�ӭܮw�óЫ� pull request ����A�����C