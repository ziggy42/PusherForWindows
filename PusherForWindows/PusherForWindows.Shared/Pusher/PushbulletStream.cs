﻿using Newtonsoft.Json;
using System;
using Windows.Data.Xml.Dom;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.Web;

namespace PusherForWindows.Pusher
{
    public class PushbulletStream
    {
        private MessageWebSocket messageWebSocket;
        private DataWriter messageWriter;

        public async void Connect()
        {
            try
            {
                Uri server = new Uri("wss://stream.pushbullet.com/websocket/" +
                    (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values[Pushbullet.ACCESS_TOKEN_KEY]);

                this.messageWebSocket = new MessageWebSocket();
                this.messageWebSocket.Control.MessageType = SocketMessageType.Utf8;
                this.messageWebSocket.MessageReceived += MessageReceived;

                await this.messageWebSocket.ConnectAsync(server);
                this.messageWriter = new DataWriter(messageWebSocket.OutputStream);
            }
            catch (Exception ex)
            {
                WebErrorStatus status = WebSocketError.GetStatus(ex.GetBaseException().HResult);
                System.Diagnostics.Debug.WriteLine(status.ToString());
            }
        }

        public void Close()
        {
            this.messageWebSocket.Close(1000, "App Closes");
        }

        private void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            try
            {
                using (DataReader reader = args.GetDataReader())
                {
                    reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    dynamic json = JsonConvert.DeserializeObject(reader.ReadString(reader.UnconsumedBufferLength));
                    if (((string)json.type).Equals("push"))
                    {
                        dynamic push = json.push;
                        if (((string)push.type).Equals("mirror"))
                        {
                            ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;
                            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

                            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
                            toastTextElements[0].AppendChild(toastXml.CreateTextNode((string)push.title));
                            toastTextElements[1].AppendChild(toastXml.CreateTextNode((string)push.body));

                            ToastNotification toast = new ToastNotification(toastXml);
                            toast.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(5);

                            ToastNotificationManager.CreateToastNotifier().Show(toast);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                WebErrorStatus status = WebSocketError.GetStatus(ex.GetBaseException().HResult);
                System.Diagnostics.Debug.WriteLine(status.ToString());
            }
        }
    }
}
