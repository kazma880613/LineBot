using System;
using System.Net.Http.Headers;
using System.Text;
using LineBot.Dtos;
using LineBot.Dtos.Message.Request;
using LineBot.Dtos.Message;
using LineBot.Enum;
using LineBot.Object;
using LineBot.Provider;
using LineBot.Dtos.Actions;
using LineBot.Service;
using System.Drawing;
using Newtonsoft.Json;
using Google.Protobuf;
using Org.BouncyCastle.Utilities;
using RabbitMQ.Client;

namespace LineBot.Domain
{
    public class LineBotService
    {
        List<Response> _responseList = new List<Response>();
        Carouseldata _carouseldata = new Carouseldata();
        Dbservice _dbservice = new Dbservice();
        RBMQService _RBMQ = new RBMQService();
        ChannelSetting _setting = new ChannelSetting();
        public static Dictionary<string, string> variables = new Dictionary<string, string>();
        public static string function_Select;
        private readonly string channelAccessToken;
        private readonly string channelSecret;
        private readonly string replyMessageUri = "https://api.line.me/v2/bot/message/reply";
        private readonly string broadcastMessageUri = "https://api.line.me/v2/bot/message/broadcast";
        private static HttpClient client = new HttpClient(); 
        private readonly JsonProvider _jsonProvider = new JsonProvider();

        public LineBotService()
        {
            channelAccessToken = _setting.ChanelchannelAccessToken;
            channelSecret = _setting.ChannelSecret;
        }

        public void ReceiveWebhook(WebhookRequestBodyDto requestBody)
        {
            dynamic replyMessage;
            foreach (var eventObject in requestBody.Events)
            {
                switch (eventObject.Type)
                {
                    case WebhookEventTypeEnum.Message:
                        if (eventObject.Message.Text == "UBike站點" || eventObject.Message.Text == "電動車充電站點" || eventObject.Message.Text == "電動車維修站點")
                        {
                            if (variables.ContainsKey(eventObject.Source.UserId))
                            {
                                variables[eventObject.Source.UserId] = eventObject.Message.Text;
                            }
                            else
                            {
                                variables.Add(eventObject.Source.UserId, eventObject.Message.Text);
                            }
                            
                            _RBMQ.Send_Message("Debug", eventObject.Source.UserId + "\n" + variables[eventObject.Source.UserId]);

                            replyMessage = new ReplyMessageRequestDto<TextMessageDto>()
                            {
                                ReplyToken = eventObject.ReplyToken,
                                Messages = new List<TextMessageDto>
                                {
                                    new TextMessageDto(){Text = $"您選擇查詢\"{eventObject.Message.Text}\"服務!\n請開啟定位"}
                                }
                            };
                            ReplyMessageHandler(replyMessage);
                        }
                        else if (eventObject.Message.Type == "location")
                        {
                            if (variables[eventObject.Source.UserId] == "Ubike站點" || variables[eventObject.Source.UserId] == "電動車充電站點" || variables[eventObject.Source.UserId] == "電動車維修站點")
                            {
                                var msg = eventObject.Message;
                                string latitude = msg.Latitude.ToString();
                                string longitude = msg.Longitude.ToString();
                                _responseList = _dbservice.query(latitude, longitude, variables[eventObject.Source.UserId]);
                                _RBMQ.Send_Message("Debug", variables[eventObject.Source.UserId]);

                                if(_responseList.Count() == 0)
                                {
                                    replyMessage = new ReplyMessageRequestDto<TextMessageDto>()
                                    {
                                        ReplyToken = eventObject.ReplyToken,
                                        Messages = new List<TextMessageDto>
                                        {
                                            new TextMessageDto(){Text = $"您周遭並無 {variables[eventObject.Source.UserId]} !"}
                                        }
                                    };
                                    ReplyMessageHandler(replyMessage);
                                }
                                else
                                {
                                    ReceiveMessageWebhookEvent(eventObject, _responseList);
                                }
                            }
                            else
                            {
                                replyMessage = new ReplyMessageRequestDto<TextMessageDto>()
                                {
                                    ReplyToken = eventObject.ReplyToken,
                                    Messages = new List<TextMessageDto>
                                    {
                                        new TextMessageDto(){Text = $"您並未選擇查詢項目\"!"}
                                    }
                                };
                                _RBMQ.Send_Message("Debug", eventObject.Source.UserId + "\n" + variables[eventObject.Source.UserId]);
                                ReplyMessageHandler(replyMessage);
                            }
                        }
                        else if (eventObject.Message.Text == "快速功能")
                        {
                            ReceiveMessageWebhookEvent(eventObject, _responseList);
                        }
                        else
                        {
                            replyMessage = new ReplyMessageRequestDto<TextMessageDto>()
                            {
                                ReplyToken = eventObject.ReplyToken,
                                Messages = new List<TextMessageDto>
                                {
                                    new TextMessageDto(){Text = $"您好，您傳送了\"{eventObject.Message.Text}\"!"}
                                }
                            };
                            ReplyMessageHandler(replyMessage);
                        }
                        DateTime _time = DateTime.Now;
                        Console.WriteLine("收到使用者傳送訊息！" + eventObject.Message.Text + "\n" + _time);
                        break;
                    case WebhookEventTypeEnum.Unsend:
                        Console.WriteLine($"使用者{eventObject.Source.UserId}在聊天室收回訊息！");
                        break;
                    case WebhookEventTypeEnum.Follow:
                        Console.WriteLine($"使用者{eventObject.Source.UserId}將我們新增為好友！");
                        break;
                    case WebhookEventTypeEnum.Unfollow:
                        Console.WriteLine($"使用者{eventObject.Source.UserId}封鎖了我們！");
                        break;
                    case WebhookEventTypeEnum.Join:
                        Console.WriteLine("我們被邀請進入聊天室了！");
                        break;
                    case WebhookEventTypeEnum.Leave:
                        Console.WriteLine("我們被聊天室踢出了");
                        break;
                    case WebhookEventTypeEnum.MemberJoined:
                        string joinedMemberIds = "";
                        foreach (var member in eventObject.Joined.Members)
                        {
                            joinedMemberIds += $"{member.UserId} ";
                        }
                        Console.WriteLine($"使用者{joinedMemberIds}加入了群組！");
                        break;
                    case WebhookEventTypeEnum.MemberLeft:
                        string leftMemberIds = "";
                        foreach (var member in eventObject.Left.Members)
                        {
                            leftMemberIds += $"{member.UserId} ";
                        }
                        Console.WriteLine($"使用者{leftMemberIds}離開了群組！");
                        break;
                    case WebhookEventTypeEnum.Postback:
                        Console.WriteLine($"使用者{eventObject.Source.UserId}觸發了postback事件");
                        break;
                    case ActionTypeEnum.Location:
                        replyMessage = new ReplyMessageRequestDto<TextMessageDto>()
                        {
                            ReplyToken = eventObject.ReplyToken,
                            Messages = new List<TextMessageDto>
                            {
                                new TextMessageDto(){Text = $"使用者您好，地址為{ActionTypeEnum.Location}"}
                            }
                        };
                        ReplyMessageHandler(replyMessage);
                        break;
                }
            }
        }

        public void BroadcastMessageHandler(string messageType, object requestBody)
        {
            string strBody = requestBody.ToString();
            dynamic messageRequest = new BroadcastMessageRequestDto<BaseMessageDto>();
            switch (messageType)
            {
                case MessageEnum.Text:
                    messageRequest = _jsonProvider.Deserialize<BroadcastMessageRequestDto<TextMessageDto>>(strBody);
                    break;
                case MessageEnum.Sticker:
                    messageRequest = _jsonProvider.Deserialize<BroadcastMessageRequestDto<StickerMessageDto>>(strBody);
                    break;
                case MessageEnum.Image:
                    messageRequest = _jsonProvider.Deserialize<BroadcastMessageRequestDto<ImageMessageDto>>(strBody);
                    break;
                case MessageEnum.Video:
                    messageRequest = _jsonProvider.Deserialize<BroadcastMessageRequestDto<VideoMessageDto>>(strBody);
                    break;
                case MessageEnum.Audio:
                    messageRequest = _jsonProvider.Deserialize<BroadcastMessageRequestDto<AudioMessageDto>>(strBody);
                    break;
                case MessageEnum.Location:
                    messageRequest = _jsonProvider.Deserialize<BroadcastMessageRequestDto<LocationMessageDto>>(strBody);
                    break;
                case MessageEnum.Imagemap:
                    messageRequest = _jsonProvider.Deserialize<BroadcastMessageRequestDto<ImagemapMessageDto>>(strBody);
                    break;
                case MessageEnum.FlexBubble:
                    messageRequest = _jsonProvider.Deserialize<BroadcastMessageRequestDto<FlexMessageDto<FlexBubbleContainerDto>>>(strBody);
                    break;

                case MessageEnum.FlexCarousel:
                    messageRequest = _jsonProvider.Deserialize<BroadcastMessageRequestDto<FlexMessageDto<FlexCarouselContainerDto>>>(strBody);
                    break;

            }
            BroadcastMessage(messageRequest);

        }

        public async void BroadcastMessage<T>(BroadcastMessageRequestDto<T> request)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", channelAccessToken); //帶入 channel access token
            var json = _jsonProvider.Serialize(request);
            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(broadcastMessageUri),
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(requestMessage);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        public void ReplyMessageHandler<T>(ReplyMessageRequestDto<T> requestBody)
        {
            ReplyMessage(requestBody);
        }
        
        public async void ReplyMessage<T>(ReplyMessageRequestDto<T> request)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", channelAccessToken); //帶入 channel access token
            var json = _jsonProvider.Serialize(request);
            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(replyMessageUri),
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(requestMessage);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        private void ReceiveMessageWebhookEvent(WebhookEventDto eventDto, List<Response> dataList)
        {
            dynamic replyMessage;

            if (eventDto.Message.Text == "快速功能")
            {
                replyMessage = new ReplyMessageRequestDto<TextMessageDto>()
                {
                    ReplyToken = eventDto.ReplyToken,
                    Messages = new List<TextMessageDto>
                    {
                        new TextMessageDto
                        {
                            Text = "快速功能",
                            QuickReply = new QuickReplyItemDto
                            {
                                Items = new List<QuickReplyButtonDto>
                                {
                                    new QuickReplyButtonDto
                                    {
                                        Action = new ActionDto
                                        {
                                            Type = ActionTypeEnum.Location,
                                            Label = "開啟位置",
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
                ReplyMessageHandler(replyMessage);
            }

            if (eventDto.Message.Type == "location")
            {
                List<LocationMessageDto> items = new List<LocationMessageDto>();
                for (int i = 0; i < dataList.Count(); i++)
                {
                    LocationMessageDto e = new LocationMessageDto
                    {
                        Type = ActionTypeEnum.Location,
                        Title = dataList[i].station,
                        Address = dataList[i].address,
                        Latitude = dataList[i].latitude,
                        Longitude = dataList[i].longitude
                    };
                    items.Add(e);
                }

                replyMessage = new ReplyMessageRequestDto<LocationMessageDto>
                {
                    ReplyToken = eventDto.ReplyToken,
                    Messages = items
                };
                ReplyMessageHandler(replyMessage);
            }
        }
    }
}
