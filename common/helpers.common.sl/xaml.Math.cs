using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using helpers.extensions;

namespace helpers.sl.xaml
{
    public class Math : IValueConverter
    {
        public Math()
        { }
        virtual public object Convert(object value, Type targetType, object parameter,
           System.Globalization.CultureInfo culture)
        {
            string sRetVal = value.ToString();
            if (parameter is string)
            {
                string sParameters = (string)parameter;
                long nValue = sRetVal.Trim().ToLong();
                long nParameter = sParameters.Substring(1).Trim().ToLong();
                switch (sParameters[0])
                {
                    case '+':
                        sRetVal = (nValue + nParameter).ToString();
                        break;
                    case '-':
                        sRetVal = (nValue - nParameter).ToString();
                        break;
                }
            }
            return sRetVal;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
