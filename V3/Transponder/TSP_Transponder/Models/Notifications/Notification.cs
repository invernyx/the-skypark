using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static TSP_Transponder.App;
using static TSP_Transponder.Attributes.EnumAttr;

namespace TSP_Transponder.Models.Notifications
{
    public class Notification
    {
        public long UID = 0;
        public NotificationType Type = NotificationType.General;
        public DateTime Time = DateTime.UtcNow;
        public DateTime? Expire = null;
        public bool IsTransponder = false;
        public bool Persist = true;
        public bool CanOpen = false;
        public bool CanDismiss = true;
        public Dictionary<string, dynamic> LaunchArgument = null;
        public string AppName = "p42_system";
        public string Title = "";
        public string Message = "";
        public string Group = "";
        public Dictionary<string, dynamic> Data = null;
        public Action Act = null;
        public bool Changed = false;

        public Border NotificationListObj = null;
        public Border NotificationLiveObj = null;
        
        private Border GetNotifElement()
        {
            SolidColorBrush BackgroundColor = new SolidColorBrush(Color.FromArgb(0xee, 250, 250, 250));
            SolidColorBrush ForegroundColor = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            switch (Type)
            {
                case NotificationType.Success:
                    {
                        BackgroundColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#ee009900");
                        ForegroundColor = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                        break;
                    }
                case NotificationType.Fail:
                    {
                        BackgroundColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#ee990000");
                        ForegroundColor = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                        break;
                    }
                case NotificationType.Status:
                    {
                        BackgroundColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#eeFFFFFF");
                        ForegroundColor = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                        break;
                    }
            }


            Border NotificationObj = new Border()
            {
                Background = BackgroundColor,
                BorderBrush = new SolidColorBrush(Color.FromArgb(0x44, 0x66, 0x66, 0x66)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
            };
            if(CanDismiss)
            {
                NotificationObj.Cursor = Cursors.Hand;
                NotificationObj.PreviewMouseLeftButtonUp += (object sender, MouseButtonEventArgs e) =>
                {
                    Trigger(true);
                };
            }

            Grid NotificationGrid = new Grid()
            {
                Margin = new Thickness(12, 8, 12, 8),
            };
            NotificationObj.Child = NotificationGrid;

            TextBlock NotificationTime = new TextBlock()
            {
                Foreground = ForegroundColor,
                HorizontalAlignment = HorizontalAlignment.Right,
                Text = Time.ToLocalTime().ToShortTimeString(),
            };
            NotificationGrid.Children.Add(NotificationTime);

            StackPanel NotificationStack = new StackPanel();
            NotificationGrid.Children.Add(NotificationStack);

            TextBlock NotificationTitle = new TextBlock()
            {
                Foreground = ForegroundColor,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 50, 4),
                Text = Title,
            };
            NotificationStack.Children.Add(NotificationTitle);

            TextBlock NotificationMessage = new TextBlock()
            {
                Foreground = ForegroundColor,
                TextWrapping = TextWrapping.Wrap,
                Text = Message,
            };
            NotificationStack.Children.Add(NotificationMessage);

            return NotificationObj;
        }

        public Border GetNotifListElement(bool GetNew = false)
        {
            if(NotificationListObj == null || GetNew)
            {
                NotificationListObj = GetNotifElement();
                NotificationListObj.Margin = new Thickness(0, 0, 0, 5);
            }

            return NotificationListObj;
        }


        public void Trigger(bool TriggerAction)
        {
            Act?.Invoke();
            NotificationService.Remove(this);
        }

        public void Update(Notification New)
        {
            Expire = New.Expire;
            IsTransponder = New.IsTransponder;
            Persist = New.Persist;
            CanOpen = New.CanOpen;
            CanDismiss = New.CanDismiss;
            LaunchArgument = New.LaunchArgument;
            Title = New.Title;
            Message = New.Message;
            Group = New.Group;
            Data = New.Data;


        }

        public Dictionary<string, dynamic> ToDictionary()
        {
            Dictionary<string, dynamic> ss = new Dictionary<string, dynamic>()
            {
                { "UID", UID },
                { "Type", GetDescription(Type) },
                { "Time", Time.ToString("O") },
                { "Expire", Expire != null ? ((DateTime)Expire).ToString("O") : null },
                { "IsTransponder", IsTransponder },
                { "Persist", Persist },
                { "CanOpen", CanOpen },
                { "CanDismiss", CanDismiss },
                { "LaunchArgument", LaunchArgument },
                { "App", AppName },
                { "Title", Title },
                { "Message", Message },
                { "Group", Group },
                { "Data", Data }
            };

            return ss;
        }
    }

    public enum NotificationType
    {
        [EnumValue("General")]
        General,
        [EnumValue("Status")]
        Status,
        [EnumValue("Success")]
        Success,
        [EnumValue("Fail")]
        Fail,
        [EnumValue("DestinationContracts")]
        DestinationContracts
    }
}
