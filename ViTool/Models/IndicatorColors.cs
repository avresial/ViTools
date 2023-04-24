using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ViTool.Models
{
   public class IndicatorColors
    {
        public SolidColorBrush busyColor = new SolidColorBrush(Color.FromRgb(255, 255, 0));
        public SolidColorBrush doneColor = new SolidColorBrush(Color.FromRgb(0, 204, 0));
        public SolidColorBrush errorColor = new SolidColorBrush(Color.FromRgb(216, 31, 42));
    }
}
