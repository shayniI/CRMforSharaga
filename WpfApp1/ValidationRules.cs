using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace WpfApp1.Validators
{
    public class PhoneRule
    {
        public static void Textbox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                if (string.IsNullOrEmpty(tb.Text))
                {
                    tb.BorderBrush = System.Windows.Media.Brushes.Red;
                    tb.ToolTip = "Телефон не может быть пустым.";
                    return;
                }
                else if(tb.Text.Length != 12 || tb.Text[0] != '+')
                {
                    tb.BorderBrush = System.Windows.Media.Brushes.Red;
                    tb.ToolTip = "Неверный формат телефона. Пример: +79999999999";
                    return;
                }
                else
                {

                    tb.BorderBrush = System.Windows.Media.Brushes.Black;
                    tb.ToolTip = null;
                }
            }
        }
    }
}

public class RussianPhoneNumberConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string rawNumber && !string.IsNullOrEmpty(rawNumber))
        {
            var digitsOnly = Regex.Replace(rawNumber, @"\D", "");

            if (digitsOnly.Length == 11 && digitsOnly.StartsWith("7"))
            {
                return Regex.Replace(digitsOnly, @"^(\d)(\d{3})(\d{3})(\d{2})(\d{2})$", "+$1 ($2) $3-$4-$5");
            }
            else if (digitsOnly.Length == 10)
            {
                return Regex.Replace(digitsOnly, @"^(\d{3})(\d{3})(\d{2})(\d{2})$", "($1) $2-$3-$4");
            }

            return rawNumber;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string formattedNumber)
        {
            return Regex.Replace(formattedNumber, @"\D", "");
        }
        return value;
    }
}
public class MainViewModel : INotifyPropertyChanged
{
    private string _phoneNumber;
    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            if (_phoneNumber != value)
            {
                _phoneNumber = value;
                OnPropertyChanged(nameof(PhoneNumber));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
