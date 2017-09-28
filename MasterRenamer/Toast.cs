using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;

namespace MasterRenamer
{
    public class Toast
    {

        static Notifier notifier = new Notifier(cfg =>
        {
            cfg.PositionProvider = new WindowPositionProvider(
                parentWindow: Application.Current.MainWindow,
                corner: Corner.TopRight,
                offsetX: 10,
                offsetY: 10);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(3),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.Dispatcher = Application.Current.Dispatcher;
        });


        static public void Show(string message, Type type = Type.Err)
        {
            switch (type)
            {
                case Type.Msg: notifier.ShowInformation(message); break;
                case Type.Succ: notifier.ShowSuccess(message); break;
                case Type.Warn: notifier.ShowWarning(message); break;
                case Type.Err: notifier.ShowError(message); break;

            }
        }

       public enum Type {
            Msg,
            Succ,
            Warn,
            Err
        }

    }
}
