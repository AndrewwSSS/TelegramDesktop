using CommonLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Telegram.WPF_Entities
{
    public class IsLastMessageConverter : DependencyObject, IMultiValueConverter
    {

        

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
                throw new NullReferenceException("Конвертеру не был задан массив");

            IEnumerable<ChatMessage> list = values[0] as IEnumerable<ChatMessage>;
            ChatMessage msg = values[1] as ChatMessage;

            return list.Last().Equals(msg);
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

