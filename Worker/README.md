# Worker �]�w�P���ի��n

�o�ӱM�ץ]�t�F�@�� Worker �{���A�����ť RabbitMQ �T����C�óz�L WebSocket �s���T���C�p�G WebSocket �s�u���_�A�T���|�Q�Ȧs�æb�� WebSocket �s�u�ɶi��o�e�C

## �B�J 1: ���ҳ]�w

1. **�w�� RabbitMQ**  
   �Y�|���w�� RabbitMQ�A�Ш̷ӥH�U�B�J�w�ˡG
   - [RabbitMQ �w�˫��n](https://www.rabbitmq.com/download.html)

   �Y�O���a���աA�i�H�ϥ� Docker �ֳt�Ұ� RabbitMQ�G
   ```bash
   docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management

2. **�w�� .NET SDK**
�T�O�w�g�w�� .NET SDK 6.0 �ΥH�W�����C

�إ߻P�]�w�M��
�ƻs�ΧJ���M�ת��{���X�A�öi�J�M�ץؿ��G


## ��Ƨ����c

```
MyRabbitMQWebApp/
�u�w�w Controllers/
�x   �|�w�w WebSocketController.cs        # WebSocket ����A�B�z WebSocket �s��
�u�w�w Services/
�x   �|�w�w MessageService.cs             # �B�z RabbitMQ �������A��
�u�w�w wwwroot/                          # �R�A��󧨡A�s�� HTML�BCSS�BJS ��
�x   �|�w�w index.html                    # �e�ݭ����A��� WebSocket �����쪺����
�u�w�w Program.cs                        # �D�n�J�f�A�t�m�M�ҰʪA��
�u�w�w MyRabbitMQWebApp.csproj           # �M����
�|�w�w appsettings.json                  # �t�m�ɡ]�p�G�ݭn�A�i�H�Ω� RabbitMQ �t�m���^
```