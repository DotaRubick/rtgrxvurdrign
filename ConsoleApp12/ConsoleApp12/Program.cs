using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;
using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace TelegramBotTest
{
    class Program
    {

        public class Geo
        {
            public double lat { get; set; }
            public double lon { get; set; }
        }
        public class DeviceData
        {
            public string thingId { get; set; }
            public string type
            {
                get; set;
            }
            public long timeStamp
            {
                get; set;
            }
            public string hwVer
            {
                get; set;
            }
            public string swVer
            {
                get; set;
            }
            public string contractVer
            {
                get; set;
            }
            public Dictionary<string, Geo>? current
            {
                get; set;
            }
            public bool isWatering
            {
                get; set;
            }
            public bool isWorking
            {
                get; set;
            }
        }
        X509Certificate2 certificate = new X509Certificate2("rootCA.crt");
        private static X509Certificate2 rootCrt = new X509Certificate2("C:\\Users\\lebro\\source\\repos\\ConsoleApp9\\ConsoleApp9\\rootCA.crt");
        //ВОТ ТУТ ЛЕЖАТ ВСЕ ДАННЫЕ
        public static DeviceData device;


        private static bool CertificateValidationCallback(X509Certificate cert, X509Chain chain, SslPolicyErrors errors, IMqttClientOptions opts)
        {
            try
            {
                if (errors == SslPolicyErrors.None)
                {
                    return true;
                }

                if (errors == SslPolicyErrors.RemoteCertificateChainErrors)
                {
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                    chain.ChainPolicy.ExtraStore.Add(rootCrt);

                    chain.Build((X509Certificate2)rootCrt);
                    // Сравнение отпечатков сертификатов
                    var res = chain.ChainElements.Cast<X509ChainElement>().Any(a => a.Certificate.Thumbprint == rootCrt.Thumbprint);

                    return res;
                }
            }
            catch
            {
                Console.WriteLine("");
            }

            return false;
        }
        static async Task Main()
        {
            // Настройка TLS-соединения
            MqttClientOptionsBuilderTlsParameters tlsOptions = new MqttClientOptionsBuilderTlsParameters
            {
                SslProtocol = SslProtocols.Tls12,
                UseTls = true
            };

            // Подключение обработчика для валидации сервера
            tlsOptions.CertificateValidationCallback += CertificateValidationCallback;

            // Настройка параметров соединения
            var options = new MqttClientOptionsBuilder()
                .WithClientId($"Test1{Guid.NewGuid()}")
                .WithTcpServer("mqtt.cloud.yandex.net", 8883)
                .WithTls(tlsOptions)
                .WithCleanSession()
                .WithCredentials("aresmv64htqk8lkmqr61", "ICLinnocamp2022")
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(90))
                .WithKeepAliveSendInterval(TimeSpan.FromSeconds(60))
                .Build();

            var factory = new MqttFactory();
            IMqttClient mqttClient = factory.CreateMqttClient();
            await mqttClient.ConnectAsync(options, CancellationToken.None);

            // Подключение обработчика события о получении данных
            Console.WriteLine("Подключение обработчика события о получении данных");
            mqttClient.UseApplicationMessageReceivedHandler(DataHandler);

            string topic = "$devices/are9gnqohp4npug37mbs/events/raw";
            string topic2 = "$devices/are1suqff6jhlala2bsh/events/raw";
            string topic3 = "$devices/areg5dfne7179n4o24q2/events/raw";
            Console.WriteLine("Подписка на топик");
            await mqttClient.SubscribeAsync(topic);
            await mqttClient.SubscribeAsync(topic2);
            await mqttClient.SubscribeAsync(topic3);
            Console.WriteLine(mqttClient.IsConnected);


            // Подключение обработчика события о соединении с Yandex IoT Core
            Console.WriteLine("Подключение обработчика события о соединении с Yandex IoT Core");
            mqttClient.UseConnectedHandler(ConnectedHandler);

            // Подключение обработчика события о потери связи с Yandex IoT Core
            Console.WriteLine("Подключение обработчика события о потери связи с Yandex IoT Core");
            mqttClient.UseDisconnectedHandler(DisconnectedHandler);
            while (true) { }

            await mqttClient.DisconnectAsync();
        }

        private static Task ConnectedHandler(MqttClientConnectedEventArgs arg)
        {
            Console.WriteLine("connect");
            //oConnectedEvent.Set();
            return Task.CompletedTask;
        }

        private static Task DisconnectedHandler(MqttClientDisconnectedEventArgs arg)
        {
            Console.WriteLine($"Disconnected mqtt.cloud.yandex.net.");
            return Task.CompletedTask;
        }
        private static Task DataHandler(MqttApplicationMessageReceivedEventArgs arg)
        {
            //вывод данных датчиков 

            var message = arg.ApplicationMessage.ConvertPayloadToString();
            message = message.Replace('\'', '"');
            message = message.Replace("True", "true");
            //var jsonFile = JsonDocument.Parse(message);
            device = JsonSerializer.Deserialize<DeviceData>(message);
            //SubscribedData(arg.ApplicationMessage.Topic, arg.ApplicationMessage.Payload);
            return Task.CompletedTask;
        }

        private static string token = "5345675003:AAFkWfPz3DdZxJgvJZQe7c9xeqbPqQ20-eQ";
        static ITelegramBotClient bot = new TelegramBotClient(token);
        private static readonly ChatId chatId;

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text.ToLower() == "/start")
                {
                    string text = "Здравтсвтуйте, пользователь, нажмите на /buttons чтобы открыть меню.";
                    await botClient.SendTextMessageAsync(message.Chat, text);
                    return;
                }
                else if (message.Text.ToLower() == "/help")
                {
                    string text = "/start \n/help";
                    await botClient.SendTextMessageAsync(message.Chat, text);
                    return;
                }
                else if (message.Text.ToLower() == "/buttons")
                {
                    SendInline(botClient: botClient, chatId: message.Chat.Id, cancellationToken: cancellationToken);
                    return;
                }
            }
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                string codeOfButton = update.CallbackQuery.Data;
                if (codeOfButton == "post")
                {


                    InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(

                                   new[]
                                   {

                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Автополив1", callbackData: "2"),
                        InlineKeyboardButton.WithCallbackData(text: "Автополив2", callbackData: "3")
                    },
                                   });

                    Message sentMessage = await botClient.SendTextMessageAsync(
                        chatId: update.CallbackQuery.Message.Chat,
                        text: "Система полива",
                        replyMarkup: inlineKeyboard,
                        cancellationToken: cancellationToken);
                }

                if (codeOfButton == "1")
                {


                    InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(

                                   new[]
                                   {

                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Люк1", callbackData: "4"),
                        InlineKeyboardButton.WithCallbackData(text: "Люк2", callbackData: "5")
                    },
                                   });

                    Message sentMessage = await botClient.SendTextMessageAsync(
                        chatId: update.CallbackQuery.Message.Chat,
                        text: "Люки",
                        replyMarkup: inlineKeyboard,
                        cancellationToken: cancellationToken);
                }

                if (codeOfButton == "4")
                {
                    Console.WriteLine("Нажата кнопка 4");
                    string telegramMessage = "Затоплен";
                    await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat, telegramMessage);
                    return;
                }

                if (codeOfButton == "5")
                {
                    Console.WriteLine("Нажата кнопка 5");
                    string telegramMessage = "Незатоплен";
                    await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat, telegramMessage);
                    return;
                }

                if (codeOfButton == "2")
                {
                    Console.WriteLine("Нажата кнопка 2");
                    string telegramMessage = "На 5 метре балка сломана";
                    await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat, telegramMessage);
                    return;
                }

                if (codeOfButton == "3")
                {
                    Console.WriteLine("Нажата кнопка 3");
                    string telegramMessage = "Всё в порядке";
                    await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat, telegramMessage);
                    return;
                }
                if (codeOfButton == "device1")
                {
                    if(device.thingId == "device1")
                    {
                        string telegramMessage = device.isWorking.ToString();
                        await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat, telegramMessage);
                        return;
                    }
                }
            }
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }



        public static async void SendInline(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(

                new[]
                {

                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Система полива", callbackData: "post"),
                        InlineKeyboardButton.WithCallbackData(text: "Люки", callbackData: "1")
                    },
                });

            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Меню",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);


        }
    }
}

    




