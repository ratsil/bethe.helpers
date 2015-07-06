using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using g = globalization;

namespace globalization.xaml
{
    public class Common : IValueConverter
    {
        public Common()
        { }

        private static g.Common _cCommon = new g.Common();

        public g.Common cCommon { get { return _cCommon; } }
        
        virtual public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            string sRetVal = value.ToString();
            if (parameter is string)
            {
                string sParameters = (string)parameter;
                bool bLower = false, bUpper = false;
                char sChar;
                foreach (char s in sParameters.ToCharArray())
                {
                    switch(s)
                    {
                        case 'l':
                            sRetVal = sRetVal.ToLower();
                            bLower = true;
                            break;
                        case 'U':
                            sRetVal = sRetVal.ToUpper();
                            bUpper = true;
                            break;
                        default:
                            sChar = s;
                            if ((bLower || bUpper) && char.IsLetter(sChar))
                            {
                                if (bLower)
                                    sChar = char.ToLower(sChar);
                                if (bLower)
                                    sChar = char.ToUpper(sChar);
                            }
                            sRetVal += sChar;
                            break;
                    }
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
